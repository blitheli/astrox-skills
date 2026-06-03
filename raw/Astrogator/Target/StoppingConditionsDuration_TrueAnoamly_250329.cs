using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator 
            自变量: StoppingConditions.Duration
            约束: TrueAnomaly (-60° 或300°都测试了)

            角度为停止条件时，内部会将微分修正的约束设置为和要求角度的差值，并归一化为(-180°,180°)
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **Initial_State（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg
          - 结果计算：远地点距离（RadiusOfApoapsis）

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：积分时长

          Resuts:
            - TrueAnomaly
                     
        ## 目标求解配置

        **Solve_For_Hohmann_Transfer（霍曼转移微分校正器）**
        输出优化过程
        - 控制变量：
          - StoppingConditions.Duration
        - 约束条件：
          - TrueAnomaly

        结果:
            - 微分修正从初值开始；6次收敛
            TA的精度为1e-9°
        */
        [TestMethod()]
        public void StoppingConditionsDuration_TrueAnoamly_250329()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "StoppingConditionsDuration_TrueAnoamly_250329.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output, 
                new JsonSerializerOptions { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var seg0 = seg.SegmentResults[0];
            //  初始段
            Assert.AreEqual(9686497.505536763, seg0.InitialState.Keplerian.SemiMajorAxis, 1e-6);
            Assert.AreEqual(0.2600177958875259, seg0.InitialState.Keplerian.Eccentricity, 1e-9);
            Assert.AreEqual(0, seg0.InitialState.Keplerian.Inclination, 1e-9);
            Assert.AreEqual(0, seg0.InitialState.Keplerian.RAAN, 1e-9);
            Assert.AreEqual(299.730955671197, seg0.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);
            Assert.AreEqual(60.26904432880302, seg0.InitialState.Keplerian.TrueAnomaly, 1e-9);
            /*
               "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505537098,
              "Eccentricity": 0.26001779588753504,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.73095567119816,
              "MeanAnomaly": 323.2911275404581,
              "TrueAnomaly": 300.00000000011454,
              "Period": 9487.704220855194
            },
          },
          "DurationSec": 7547.795025188068,
          "Results": {
            "Epoch": "2000-01-01T14:04:43.611025188074564Z",
            "TrueAnomaly": -59.99999999988546
             * 
             */

            //  最后一段
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(7547.79502518806, seg3.DurationSec, 0.001);
            Assert.AreEqual(9686497.505537098, seg3.FinalState.Keplerian.SemiMajorAxis, 1e-4);
            Assert.AreEqual(0.26001779588753504, seg3.FinalState.Keplerian.Eccentricity, 1e-9);
            Assert.AreEqual(299.73095567119816, seg3.FinalState.Keplerian.ArgOfPeriapsis, 1e-9);
            Assert.AreEqual(300.00000000011454, seg3.FinalState.Keplerian.TrueAnomaly, 1e-9);

            /*
 {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "$type": "TargetSequenceResult",
      "OperatorResults": [
        {
          "$type": "DifferentialCorrectorResults",
          "Converged": true,
          "TotalIterations": 6,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "StoppingConditions.Duration",
              "InitialValue": "5000.0",
              "FinalValue": "7547.795025188074",
              "Correction": 2547.795025188074,
              "LastUpdate": -0.00011363397015884402,
              "Dimension": "",
              "MaxStep": 2000,
              "ParentName": "Propagate",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                5000,
                7000,
                7610.996050831391,
                7548.525920490647,
                7547.7951388220445,
                7547.795025188074
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "TrueAnomaly",
              "DesiredValue": "300.0",
              "ParentName": "Propagate",
              "CurrentValue": "300.0000000001145",
              "Unit": null,
              "Difference": 1.1449996853268661E-10,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1E-06,
              "Weight": 1,
              "Values": [
                209.19554985658905,
                273.4394952748201,
                303.4414295277133,
                300.03933894774696,
                300.0000061153946,
                300.0000000001145
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: Duration_TrueAnomaly",
          "Description": null,
          "UserComment": null
        }
      ],
      "SegmentResults": [
        {
          "TypeName": "InitialState",
          "Name": "Initial_State",
          "Description": "初始段参数",
          "UserComment": "初始段参数",
          "InitialState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 1500,
              "Vy": 7500,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505536763,
              "Eccentricity": 0.2600177958875259,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.73095567119697,
              "MeanAnomaly": 36.8987457991922,
              "TrueAnomaly": 60.26904432880302,
              "Period": 9487.704220854705
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 11.309932474020208,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 7648.529270389177
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.0014761394438196909,
            "Geodetic_Longitude": 79.80638890632865,
            "Geodetic_Altitude": 1621863.000014101,
            "Geocentric_Latitude": -0.0014682609789671997,
            "Geocentric_Longitude": 79.80638890632865
          },
          "FinalState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 1500,
              "Vy": 7500,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505536763,
              "Eccentricity": 0.2600177958875259,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.73095567119697,
              "MeanAnomaly": 36.8987457991922,
              "TrueAnomaly": 60.26904432880302,
              "Period": 9487.704220854705
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 11.309932474020208,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 7648.529270389177
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.0014761394438196909,
            "Geodetic_Longitude": 79.80638890632865,
            "Geodetic_Altitude": 1621863.000014101,
            "Geocentric_Latitude": -0.0014682609789671997,
            "Geocentric_Longitude": 79.80638890632865
          },
          "DurationSec": 0,
          "Results": {
            "RadiusOfApoapsis": 12205159.23679645
          }
        },
        {
          "$type": "PropagateResult",
          "StoppedOnMaximumDuration": false,
          "StoppingConditionName": "Duration",
          "TypeName": "Propagate",
          "Name": "Propagate",
          "Description": "轨道递推段",
          "UserComment": "轨道递推段",
          "InitialState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 1500,
              "Vy": 7500,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505536763,
              "Eccentricity": 0.2600177958875259,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.73095567119697,
              "MeanAnomaly": 36.8987457991922,
              "TrueAnomaly": 60.26904432880302,
              "Period": 9487.704220854705
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 11.309932474020208,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 7648.529270389177
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.0014761394438196909,
            "Geodetic_Longitude": 79.80638890632865,
            "Geodetic_Altitude": 1621863.000014101,
            "Geocentric_Latitude": -0.0014682609789671997,
            "Geocentric_Longitude": 79.80638890632865
          },
          "FinalState": {
            "Epoch": "2000-01-01T14:04:43.611025188074564Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -4028710.1666888827,
              "Y": -6902870.021710628,
              "Z": 0,
              "Vx": 7237.640834599449,
              "Vy": -2491.9901505813523,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505537098,
              "Eccentricity": 0.26001779588753504,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.73095567119816,
              "MeanAnomaly": 323.2911275404581,
              "TrueAnomaly": 300.00000000011454,
              "Period": 9487.704220855194
            },
            "Spherical": {
              "RightAscension": 239.7309556713127,
              "Declination": 0,
              "RadiusMagnitude": 7992503.997109645,
              "HorizFPA": -11.269960442704656,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 7654.636487858576
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.0020917786751370455,
            "Geodetic_Longitude": -71.9979031766287,
            "Geodetic_Altitude": 1614366.9971379642,
            "Geocentric_Latitude": 0.002080603944262351,
            "Geocentric_Longitude": -71.9979031766287
          },
          "DurationSec": 7547.795025188068,
          "Results": {
            "Epoch": "2000-01-01T14:04:43.611025188074564Z",
            "TrueAnomaly": -59.99999999988546
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Inner_Target_List",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2000-01-01T11:58:55.81600000000617Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8000000,
          "Y": 0,
          "Z": 0,
          "Vx": 1500,
          "Vy": 7500,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505536763,
          "Eccentricity": 0.2600177958875259,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.73095567119697,
          "MeanAnomaly": 36.8987457991922,
          "TrueAnomaly": 60.26904432880302,
          "Period": 9487.704220854705
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 8000000,
          "HorizFPA": 11.309932474020208,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 7648.529270389177
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -0.0014761394438196909,
        "Geodetic_Longitude": 79.80638890632865,
        "Geodetic_Altitude": 1621863.000014101,
        "Geocentric_Latitude": -0.0014682609789671997,
        "Geocentric_Longitude": 79.80638890632865
      },
      "FinalState": {
        "Epoch": "2000-01-01T14:04:43.611025188074564Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -4028710.1666888827,
          "Y": -6902870.021710628,
          "Z": 0,
          "Vx": 7237.640834599449,
          "Vy": -2491.9901505813523,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505537098,
          "Eccentricity": 0.26001779588753504,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.73095567119816,
          "MeanAnomaly": 323.2911275404581,
          "TrueAnomaly": 300.00000000011454,
          "Period": 9487.704220855194
        },
        "Spherical": {
          "RightAscension": 239.7309556713127,
          "Declination": 0,
          "RadiusMagnitude": 7992503.997109645,
          "HorizFPA": -11.269960442704656,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 7654.636487858576
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 0.0020917786751370455,
        "Geodetic_Longitude": -71.9979031766287,
        "Geodetic_Altitude": 1614366.9971379642,
        "Geocentric_Latitude": 0.002080603944262351,
        "Geocentric_Longitude": -71.9979031766287
      },
      "DurationSec": 7547.795025188068,
      "Results": {}
    }
  ],
  "Positions": {
    "CentralBody": "Earth",
    "CzmlPositions": [
      {
        "CentralBody": "Earth",
        "interpolationAlgorithm": "LAGRANGE",
        "interpolationDegree": 7,
        "referenceFrame": "INERTIAL",
        "epoch": "2000-01-01T11:58:55.816Z",
        "interval": "2000-01-01T11:58:55.816Z/2000-01-01T14:04:43.611Z",
        "cartesian": null,
        "cartesianVelocity": [
          0,
          8000000,
          0,
          0,
          1500,
          7500,
          0,
          35.80000000000291,
          8049727.111280065,
          268455.8001028468,
          0,
          1278.5698361053164,
          7496.30871384201,
          0,
          71.45100000000093,
          8091426.051880099,
          535534.6856668623,
          0,
          1061.267384681829,
          7485.497007192027,
          0,
          146.50199999999313,
          8154235.378810973,
          1095832.0689696833,
          0,
          615.1685174756683,
          7440.8106426427075,
          0,
          230.0109999999986,
          8185552.3356060935,
          1714012.8486113944,
          0,
          138.4476408927822,
          7358.978180779043,
          0,
          317.8790000000008,
          8176550.059530824,
          2355648.9847529037,
          0,
          -339.13099564400795,
          7240.355159986629,
          0,
          405.53800000000047,
          8126876.719936376,
          2984065.790635518,
          0,
          -789.8490727527451,
          7092.889481247682,
          0,
          498.9579999999987,
          8031772.26836538,
          3638256.9054579823,
          0,
          -1241.1959459366867,
          6908.0905713928505,
          0,
          600.4339999999938,
          7882376.398988231,
          4327908.156699589,
          0,
          -1697.352987046598,
          6679.966738117362,
          0,
          710.426999999996,
          7670295.841615413,
          5047824.015041708,
          0,
          -2152.0858028291145,
          6406.095751282065,
          0,
          825.7860000000073,
          7396601.381345681,
          5769090.448347505,
          0,
          -2585.7518938319927,
          6095.037588641224,
          0,
          945.6910000000062,
          7061788.271107377,
          6479508.005955111,
          0,
          -2991.404528770341,
          5751.6834047494185,
          0,
          1070.300000000003,
          6665173.786549361,
          7173127.513342507,
          0,
          -3366.7037892246835,
          5378.735134046407,
          0,
          1199.752999999997,
          6206648.38068578,
          7843658.015207024,
          0,
          -3709.625358595363,
          4979.010478312936,
          0,
          1334.1879999999946,
          5686663.818434907,
          8484607.189842705,
          0,
          -4018.493318847234,
          4555.335697988161,
          0,
          1473.732999999993,
          5106288.239882091,
          9089283.448995128,
          0,
          -4291.92420260911,
          4110.51695377236,
          0,
          1618.5130000000063,
          4467204.530617155,
          9650871.85523167,
          0,
          -4528.800369069821,
          3647.2760243038906,
          0,
          1767.622000000003,
          3776590.51942749,
          10159208.063623698,
          0,
          -4727.000943426495,
          3171.488631656279,
          0,
          1920.6110000000044,
          3040692.1383666815,
          10607308.177185126,
          0,
          -4886.1332109680725,
          2687.3089627785407,
          0,
          2077.399999999994,
          2264614.047562626,
          10990148.134788055,
          0,
          -5006.640295453031,
          2197.408207693745,
          0,
          2237.8540000000066,
          1454137.6840504555,
          11303052.267630588,
          0,
          -5089.037464847329,
          1704.338983910773,
          0,
          2400.6849999999977,
          621335.0236869401,
          11540493.560113134,
          0,
          -5133.733056152427,
          1213.8165359781235,
          0,
          2565.7119999999995,
          -227051.28258825012,
          11700541.610190416,
          0,
          -5142.090231433516,
          727.7682572733443,
          0,
          2696.448000000004,
          -898359.6554985718,
          11770983.07551612,
          0,
          -5124.077007363629,
          351.1107487823552,
          0,
          2826.9679999999935,
          -1564860.9277866138,
          11792687.022654612,
          0,
          -5085.611862138959,
          -17.23539173177477,
          0,
          2979.947,
          -2337844.08028738,
          11757656.561116755,
          0,
          -5015.7860957894745,
          -438.9128044660541,
          0,
          3142.3610000000044,
          -3144577.35888715,
          11650831.51046453,
          0,
          -4913.833204451445,
          -874.4441464095352,
          0,
          3311.4260000000068,
          -3964315.041823771,
          11465620.804130023,
          0,
          -4778.634857835216,
          -1314.2206670505743,
          0,
          3484.3380000000034,
          -4776516.373495231,
          11200541.063211065,
          0,
          -4610.868559241126,
          -1749.3454877528682,
          0,
          3657.5139999999956,
          -5558363.582040202,
          10860957.733785009,
          0,
          -4413.869754619348,
          -2169.909717964304,
          0,
          3830.555999999997,
          -6303071.034926211,
          10450238.684704153,
          0,
          -4188.694488493145,
          -2574.481983151645,
          0,
          4003.0130000000063,
          -7004089.77013781,
          9972639.111685017,
          0,
          -3936.4776008072677,
          -2961.5453537383764,
          0,
          4174.432000000001,
          -7655433.281414442,
          9433184.305390745,
          0,
          -3658.4052648942024,
          -3329.6076048790665,
          0,
          4344.364000000001,
          -8251778.535247468,
          8837593.256686488,
          0,
          -3355.7287479610095,
          -3677.199299052869,
          0,
          4512.369999999995,
          -8788554.629506795,
          8192185.2100628195,
          0,
          -3029.778897228016,
          -4002.8755138253227,
          0,
          4678.0320000000065,
          -9262029.866382403,
          7503749.129192452,
          0,
          -2681.9694953742182,
          -4305.230528277309,
          0,
          4840.956000000006,
          -9669351.622393955,
          6779427.73471851,
          0,
          -2313.813042862556,
          -4582.900013857769,
          0,
          5000.779999999999,
          -10008581.124340555,
          6026572.28413305,
          0,
          -1926.9238829766382,
          -4834.576782871193,
          0,
          5157.159,
          -10278665.159500003,
          5252703.068429143,
          0,
          -1523.0759205780464,
          -5058.99682805305,
          0,
          5309.784,
          -10479472.238155724,
          4465312.138486429,
          0,
          -1104.1781402581323,
          -5254.9879131075795,
          0,
          5458.357000000004,
          -10611741.971905192,
          3671882.46336022,
          0,
          -672.3604809286144,
          -5421.463459377125,
          0,
          5602.572,
          -10677089.716268485,
          2879899.701211706,
          0,
          -230.06011337880398,
          -5557.4554045199275,
          0,
          5742.069999999992,
          -10678058.688225497,
          2097017.9987274974,
          0,
          219.79638684285948,
          -5662.163764461671,
          0,
          5876.3139999999985,
          -10618337.974718314,
          1331691.012582359,
          0,
          673.3063924955939,
          -5735.044054596209,
          0,
          6011.528999999995,
          -10495267.084167395,
          552987.5997110909,
          0,
          1150.4523204447705,
          -5777.479065657905,
          0,
          6133.222999999998,
          -10328251.823297642,
          -150915.35293686506,
          0,
          1597.0614495108284,
          -5785.972295222944,
          0,
          6237.048999999999,
          -10142105.180069515,
          -750951.6081792673,
          0,
          1990.5498398700274,
          -5768.545322479926,
          0,
          6340.709000000003,
          -9914930.00925125,
          -1346950.039992869,
          0,
          2394.2878909134056,
          -5726.21428256235,
          0,
          6468.5669999999955,
          -9576237.21342774,
          -2073803.378436111,
          0,
          2906.0709084146933,
          -5636.178295215411,
          0,
          6605.0610000000015,
          -9141449.107626693,
          -2833686.3498718995,
          0,
          3466.9818694853766,
          -5488.808197742447,
          0,
          6734.770999999993,
          -8656575.535145195,
          -3533613.6462294823,
          0,
          4010.6918783477695,
          -5293.98308393329,
          0,
          6856.130999999994,
          -8138665.315774974,
          -4162325.663959371,
          0,
          4524.934791496514,
          -5058.046521439054,
          0,
          6971.010999999999,
          -7590828.544312161,
          -4728063.647193499,
          0,
          5012.303192612249,
          -4782.285790593947,
          0,
          7080.516000000003,
          -7016690.293093355,
          -5235015.485033635,
          0,
          5472.634835479616,
          -4468.0142036751395,
          0,
          7185.3399999999965,
          -6420302.305128058,
          -5685410.714397683,
          0,
          5904.27292311481,
          -4116.906370761401,
          0,
          7285.9909999999945,
          -5805728.744320148,
          -6080768.96855183,
          0,
          6304.96118519612,
          -3730.9679165303564,
          0,
          7382.862999999998,
          -5176998.398181109,
          -6422308.045511688,
          0,
          6672.161050510933,
          -3312.600291779124,
          0,
          7476.286999999997,
          -4537990.486195847,
          -6711198.606079185,
          0,
          7003.306325727727,
          -2864.576377666384,
          0,
          7547.795025188068,
          -4028710.1666888827,
          -6902870.021710628,
          0,
          7237.640834599449,
          -2491.9901505813523,
          0
        ]
      }
    ]
  }
}

             */


        }

    }
}