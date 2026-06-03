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
            自变量: StoppingConditions.Altitude
            约束: Duration
        
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
          - 停止条件：Alitude

          Resuts:            
            - Duration

        ## 目标求解配置

        **DC: Epoch_Altitude（霍曼转移微分校正器）**
          输出优化过程
          - 控制变量：
            - StoppingConditions.Altitude
          - 约束条件：
            - Duration

        结果:
            由于是二体轨道积分器，所以最终的轨道参数与初始段一致!
            由于约束条件为0.001s，最终的TA的误差为1e-6°

        */
        [TestMethod()]
        public void StoppingConditionsAltitude_Duration_250323()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "StoppingConditionsAltitude_Duration_250323.json");

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
              STK 结果
                                                                                                                      
            UTC Gregorian Date: 1 Jan 2000 12:48:55.816  UTC Julian Date: 2451545.03397935                            
            Julian Ephemeris Date: 2451545.03472222                                                                   
            Time past epoch: -7.08967e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)               
                                                                                                          
            State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                          
            Parameter Set Type:  Cartesian                                                                            
                     X:    -2438.3154685197450817 km              Vx:       -5.0047210699859006 km/sec                
                     Y:    11748.3087033135543606 km              Vy:       -0.4933700792046631 km/sec                
                     Z:        0.0000000000000000 km              Vz:        0.0000000000000000 km/sec                
                                                                                                          
            Parameter Set Type:  Keplerian                                                                            
                   sma:     9686.4975055385511951 km            RAAN:                         0 deg                   
                   ecc:        0.2600177958877598                  w:         299.7309556712133 deg                   
                   inc:                         0 deg             TA:         161.9940967171902 deg                   
                                                                                                          
            Parameter Set Type:  Spherical                                                                            
             Right Asc:         101.7250523884036 deg     Horiz. FPA:         6.094971968500162 deg                   
                  Decl:                         0 deg        Azimuth:                        90 deg                   
                   |R|:    11998.6724145788593887 km             |V|:        5.0289807141621887 km/sec    


              "Epoch": "2000-01-01T12:48:55.81600003896165Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -2438315.423409531,
              "Y": 11748308.70775817,
              "Z": 0,
              "Vx": -5004.721075057972,
              "Vy": -493.370054770867,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505537441,
              "Eccentricity": 0.26001779588761503,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.7309556712041,
              "MeanAnomaly": 150.73028764256665,
              "TrueAnomaly": 161.99409650197194,
              "Period": 9487.704220855696
            },
            */
            //  最后一段
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual("2000-01-01T12:48:55", seg3.FinalState.Epoch.Split('.')[0]);
            Assert.AreEqual(9686.4975055385511951, seg3.FinalState.Keplerian.SemiMajorAxis*0.001, 1e-8);
            Assert.AreEqual(0.2600177958877598, seg3.FinalState.Keplerian.Eccentricity, 1e-12);
            Assert.AreEqual(299.7309556712133, seg3.FinalState.Keplerian.ArgOfPeriapsis, 1e-11);
            Assert.AreEqual(161.9940967171902, seg3.FinalState.Keplerian.TrueAnomaly, 1e-6);
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
          "TotalIterations": 8,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "StoppingConditions.Altitude",
              "InitialValue": "4000000",
              "FinalValue": "5620535.409774607",
              "Correction": 1620535.409774607,
              "LastUpdate": -3.6917304245289415,
              "Dimension": "",
              "MaxStep": 1000000,
              "ParentName": "Propagate",
              "Perturbation": 1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.001,
              "Unit": null,
              "Values": [
                4000000,
                5000000,
                5819968.897397317,
                5757664.0114774965,
                5656916.799951726,
                5622284.640494623,
                5620539.101505032,
                5620535.409774607
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "Duration1",
              "DesiredValue": "3000.0",
              "ParentName": "Propagate",
              "CurrentValue": "3000.000000038941",
              "Unit": null,
              "Difference": 3.8940925151109695E-08,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.001,
              "Weight": 1,
              "Values": [
                1441.9883619252068,
                2220.588738038772,
                3628.9976064018265,
                3324.6977517658815,
                3071.407380782737,
                3003.282860962281,
                3000.006913925492,
                3000.000000038941
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: Altitude_Vmag",
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
          "StoppingConditionName": "Altitude",
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
            "Epoch": "2000-01-01T12:48:55.816000038947095Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -2438315.4234094764,
              "Y": 11748308.707758175,
              "Z": 0,
              "Vx": -5004.721075057978,
              "Vy": -493.37005477083744,
              "Vz": 0
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 9686497.505537443,
              "Eccentricity": 0.26001779588761503,
              "Inclination": 0,
              "RAAN": 0,
              "ArgOfPeriapsis": 299.7309556712041,
              "MeanAnomaly": 150.73028764256617,
              "TrueAnomaly": 161.99409650197165,
              "Period": 9487.704220855701
            },
            "Spherical": {
              "RightAscension": 101.72505217317578,
              "Declination": 0,
              "RadiusMagnitude": 11998672.409763636,
              "HorizFPA": 6.094972035976189,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 5028.980716812708
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.0012260652741245498,
            "Geodetic_Longitude": 168.99721846967793,
            "Geodetic_Altitude": 5620535.409773368,
            "Geocentric_Latitude": -0.0012217022802961139,
            "Geocentric_Longitude": 168.99721846967793
          },
          "DurationSec": 3000.000000038941,
          "Results": {
            "Vel": 5028.980716812708,
            "Duration1": 3000.000000038941
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
        "Epoch": "2000-01-01T12:48:55.816000038947095Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -2438315.4234094764,
          "Y": 11748308.707758175,
          "Z": 0,
          "Vx": -5004.721075057978,
          "Vy": -493.37005477083744,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505537443,
          "Eccentricity": 0.26001779588761503,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.7309556712041,
          "MeanAnomaly": 150.73028764256617,
          "TrueAnomaly": 161.99409650197165,
          "Period": 9487.704220855701
        },
        "Spherical": {
          "RightAscension": 101.72505217317578,
          "Declination": 0,
          "RadiusMagnitude": 11998672.409763636,
          "HorizFPA": 6.094972035976189,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 5028.980716812708
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -0.0012260652741245498,
        "Geodetic_Longitude": 168.99721846967793,
        "Geodetic_Altitude": 5620535.409773368,
        "Geocentric_Latitude": -0.0012217022802961139,
        "Geocentric_Longitude": 168.99721846967793
      },
      "DurationSec": 3000.000000038941,
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
        "interval": "2000-01-01T11:58:55.816Z/2000-01-01T12:48:55.816Z",
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
          3000.000000038941,
          -2438315.4234094764,
          11748308.707758175,
          0,
          -5004.721075057978,
          -493.37005477083744,
          0
        ]
      }
    ]
  }
}

         */
    }
}