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
            自变量: InitialState.Epoch
            约束: Longitude
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **InitialState（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：Duration

          Resuts:            
            - Longitude

        ## 目标求解配置

        **微分修正: Epoch-Longitude）**
          - 控制变量：
            - InitialState.Epoch
          - 约束条件：
            - Longitude

        结果:
            与STK对比，初始时刻 < 0.3s

        */
        [TestMethod()]
        public void InitialStateEpoch_Lon_250430()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateEpoch_Lon_250430.json");

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
            var seg0 = seg.SegmentResults[1];

            //  初始段
            //Assert.AreEqual("2000-01-01T16:49:44", seg0.InitialState.Epoch.Split('.')[0]);
            Assert.AreEqual(100, (double)seg0.Results["Longitude"], 0.0001);
        }

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
          "TotalIterations": 5,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Epoch",
              "InitialValue": "2000-01-01T11:58:55.816Z",
              "FinalValue": "2000-01-01T16:49:44.0241858260415Z",
              "Correction": 17448.208185826028,
              "LastUpdate": 2448.208185826028,
              "Dimension": "DateFormat",
              "MaxStep": 5000,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                0,
                5000,
                10000,
                15000,
                17448.208185826028
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "Longitude",
              "DesiredValue": "100.0",
              "ParentName": "Propagate",
              "CurrentValue": "100.0000000007948",
              "Unit": null,
              "Difference": 7.947951704602811E-10,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                172.89990874263935,
                152.0095376611368,
                131.1191665796339,
                110.22879549813074,
                100.0000000007948
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "微分修正: Epoch-Longitude",
          "Description": null,
          "UserComment": null
        }
      ],
      "SegmentResults": [
        {
          "TypeName": "InitialState",
          "Name": "InitialState",
          "Description": "初始段参数",
          "UserComment": "初始段参数",
          "InitialState": {
            "Epoch": "2000-01-01T16:49:44.0241858260415Z",
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
            "Geodetic_Latitude": -0.0015488652412290742,
            "Geodetic_Longitude": 6.9076377963863,
            "Geodetic_Altitude": 1621863.0000155268,
            "Geocentric_Latitude": -0.0015405986235909287,
            "Geocentric_Longitude": 6.907637796386302
          },
          "FinalState": {
            "Epoch": "2000-01-01T16:49:44.0241858260415Z",
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
            "Geodetic_Latitude": -0.0015488652412290742,
            "Geodetic_Longitude": 6.9076377963863,
            "Geodetic_Altitude": 1621863.0000155268,
            "Geocentric_Latitude": -0.0015405986235909287,
            "Geocentric_Longitude": 6.907637796386302
          },
          "DurationSec": 0,
          "Results": {}
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
            "Epoch": "2000-01-01T16:49:44.0241858260415Z",
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
            "Geodetic_Latitude": -0.0015488652412290742,
            "Geodetic_Longitude": 6.9076377963863,
            "Geodetic_Altitude": 1621863.0000155268,
            "Geocentric_Latitude": -0.0015405986235909287,
            "Geocentric_Longitude": 6.907637796386302
          },
          "FinalState": {
            "Epoch": "2000-01-01T17:43:04.024185826041503Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -3426588.0649716076,
              "Y": 11596055.375864485,
              "Z": 0,
              "Vx": -4871.009579634761,
              "Vy": -1025.948585453522,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505537447,
              "Eccentricity": 0.26001779588761514,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.7309556712045,
              "MeanAnomaly": 158.31905709721508,
              "TrueAnomaly": 166.73124398517206,
              "Period": 9487.704220855707
            },
            "Spherical": {
              "RightAscension": 106.4621996563766,
              "Declination": 0,
              "RadiusMagnitude": 12091732.962942967,
              "HorizFPA": 4.568215434062458,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 4977.881559949745
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.0011076079310727146,
            "Geodetic_Longitude": 100.0000000007948,
            "Geodetic_Altitude": 5713595.962950908,
            "Geocentric_Latitude": -0.001103696805929546,
            "Geocentric_Longitude": 100.0000000007948
          },
          "DurationSec": 3200,
          "Results": {
            "Longitude": 100.0000000007948
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Inner_Target_List",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2000-01-01T16:49:44.0241858260415Z",
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
        "Geodetic_Latitude": -0.0015488652412290742,
        "Geodetic_Longitude": 6.9076377963863,
        "Geodetic_Altitude": 1621863.0000155268,
        "Geocentric_Latitude": -0.0015405986235909287,
        "Geocentric_Longitude": 6.907637796386302
      },
      "FinalState": {
        "Epoch": "2000-01-01T17:43:04.024185826041503Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -3426588.0649716076,
          "Y": 11596055.375864485,
          "Z": 0,
          "Vx": -4871.009579634761,
          "Vy": -1025.948585453522,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505537447,
          "Eccentricity": 0.26001779588761514,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.7309556712045,
          "MeanAnomaly": 158.31905709721508,
          "TrueAnomaly": 166.73124398517206,
          "Period": 9487.704220855707
        },
        "Spherical": {
          "RightAscension": 106.4621996563766,
          "Declination": 0,
          "RadiusMagnitude": 12091732.962942967,
          "HorizFPA": 4.568215434062458,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 4977.881559949745
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -0.0011076079310727146,
        "Geodetic_Longitude": 100.0000000007948,
        "Geodetic_Altitude": 5713595.962950908,
        "Geocentric_Latitude": -0.001103696805929546,
        "Geocentric_Longitude": 100.0000000007948
      },
      "DurationSec": 3200,
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
        "epoch": "2000-01-01T16:49:44.024Z",
        "interval": "2000-01-01T16:49:44.024Z/2000-01-01T17:43:04.024Z",
        "cartesian": null,
        "cartesianVelocity": [
          0,
          8000000,
          0,
          0,
          1500,
          7500,
          0,
          35.79999999999927,
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
          146.5020000000004,
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
          600.4340000000011,
          7882376.398988231,
          4327908.156699589,
          0,
          -1697.352987046598,
          6679.966738117362,
          0,
          710.4269999999997,
          7670295.841615413,
          5047824.015041708,
          0,
          -2152.0858028291145,
          6406.095751282065,
          0,
          825.7860000000001,
          7396601.381345681,
          5769090.448347505,
          0,
          -2585.7518938319927,
          6095.037588641224,
          0,
          945.6909999999989,
          7061788.271107377,
          6479508.005955111,
          0,
          -2991.404528770341,
          5751.6834047494185,
          0,
          1070.2999999999993,
          6665173.786549361,
          7173127.513342507,
          0,
          -3366.7037892246835,
          5378.735134046407,
          0,
          1199.7530000000006,
          6206648.38068578,
          7843658.015207024,
          0,
          -3709.625358595363,
          4979.010478312936,
          0,
          1334.1879999999983,
          5686663.818434907,
          8484607.189842705,
          0,
          -4018.493318847234,
          4555.335697988161,
          0,
          1473.7330000000002,
          5106288.239882091,
          9089283.448995128,
          0,
          -4291.92420260911,
          4110.51695377236,
          0,
          1618.512999999999,
          4467204.530617155,
          9650871.85523167,
          0,
          -4528.800369069821,
          3647.2760243038906,
          0,
          1767.6219999999994,
          3776590.51942749,
          10159208.063623698,
          0,
          -4727.000943426495,
          3171.488631656279,
          0,
          1920.6110000000008,
          3040692.1383666815,
          10607308.177185126,
          0,
          -4886.1332109680725,
          2687.3089627785407,
          0,
          2077.4000000000015,
          2264614.047562626,
          10990148.134788055,
          0,
          -5006.640295453031,
          2197.408207693745,
          0,
          2237.8539999999994,
          1454137.6840504555,
          11303052.267630588,
          0,
          -5089.037464847329,
          1704.338983910773,
          0,
          2400.6850000000013,
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
          2696.4480000000003,
          -898359.6554985718,
          11770983.07551612,
          0,
          -5124.077007363629,
          351.1107487823552,
          0,
          2826.9680000000008,
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
          3142.361000000001,
          -3144577.35888715,
          11650831.51046453,
          0,
          -4913.833204451445,
          -874.4441464095352,
          0,
          3200,
          -3426588.0649716076,
          11596055.375864485,
          0,
          -4871.009579634761,
          -1025.948585453522,
          0
        ]
      }
    ]
  }
}

         */
    }
}