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
            自变量: InitialState.Cartesian.Vx(Vy,Vz)
            约束: X,Y,Z
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **InitialState（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg
          - 结果计算：远地点距离（RadiusOfApoapsis）

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：Duration

          Resuts:            
            - X，Y，Z

        ## 目标求解配置

        **DC: Epoch_Altitude（霍曼转移微分校正器）**
          输出优化过程
          - 控制变量：
            - InitialState.Cartesian.Vx
            - InitialState.Cartesian.Vy
            - InitialState.Cartesian.Vz
          - 约束条件：
            - X
            - Y
            - Z

        结果:
            与STK对比，位置速度相对误差： 为1e-9

        */
        [TestMethod()]
        public void InitialStateVxVyVz_XYZ_250326()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateVxVyVz_XYZ_250326.json");

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
            //Assert.AreEqual(9686497.505536763, seg0.InitialState.Keplerian.SemiMajorAxis, 1e-6);
            //Assert.AreEqual(0.2600177958875259, seg0.InitialState.Keplerian.Eccentricity, 1e-9);            
            //Assert.AreEqual(0, seg0.InitialState.Keplerian.RAAN, 1e-9);
            //Assert.AreEqual(299.730955671197, seg0.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);
            //Assert.AreEqual(60.26904432880302, seg0.InitialState.Keplerian.TrueAnomaly, 1e-9);
            //// 与STK对比
            //Assert.AreEqual(36.523792891467730, seg0.InitialState.Keplerian.Inclination, 1e-5);
            /*
              STK 结果                                                                         
                                                                                                          
            UTC Gregorian Date: 1 Jan 2000 12:48:55.816  UTC Julian Date: 2451545.03397935                            
            Julian Ephemeris Date: 2451545.03472222                                                                   
            Time past epoch: -7.08967e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)               
                                                                                                                      
            State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                                      
            Parameter Set Type:  Cartesian                                                                            
                    X:    -2500.0000044930984586 km              Vx:       -5.1588478951313306 km/sec                
                    Y:    10000.0000034961412894 km              Vy:       -1.5928202611299607 km/sec                
                    Z:     1999.9999664270853827 km              Vz:       -0.3185640467670559 km/sec                
                                                                                                                      
            Parameter Set Type:  Keplerian                                                                            
                  sma:     8540.5310413715178584 km            RAAN:                         0 deg                   
                  ecc:        0.2378897950548192                  w:         271.7226120273268 deg                   
                  inc:          11.3099322852078 deg             TA:         192.0515349986128 deg                   
                                                                                                                      
            Parameter Set Type:  Spherical                                                                            
            Right Asc:         104.0362434874424 deg     Horiz. FPA:        -3.703482538098315 deg                   
                  Decl:         10.98057524310953 deg        Azimuth:         92.72631095266826 deg                   
                  |R|:    10499.9999980046050041 km             |V|:        5.4085368669594631 km/sec         
            

                        "Cartesian": {
              "X": -2499999.9999998566,
              "Y": 9999999.999998543,
              "Z": 1999999.9999990107,
              "Vx": -5158.84789480112,
              "Vy": -1592.8202572540695,
              "Vz": -318.5640514507027
                                                                                              
            
            */
            //  最后一段
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(-2500.0000044930984586, seg3.FinalState.Cartesian.X*0.001, 1e-5);
            Assert.AreEqual(10000.0000034961412894, seg3.FinalState.Cartesian.Y*0.001, 1e-5);
            Assert.AreEqual(1999.9999664270853827, seg3.FinalState.Cartesian.Z*0.001, 5e-5);
            Assert.AreEqual(-5.1588478951313306, seg3.FinalState.Cartesian.Vx*0.001, 1e-9);
            Assert.AreEqual(-1.5928202611299607, seg3.FinalState.Cartesian.Vy*0.001, 1e-8);
            Assert.AreEqual(-0.3185640467670559, seg3.FinalState.Cartesian.Vz*0.001, 1e-8);
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
          "TotalIterations": 17,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vx",
              "InitialValue": "1500.0",
              "FinalValue": "1672.4612701156518",
              "Correction": 172.4612701156518,
              "LastUpdate": -2.620022024757418E-05,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                1500,
                1515.0586981481867,
                1529.7174521572545,
                1543.9755050752665,
                1557.8321646800584,
                1571.2866998170289,
                1584.3382056924356,
                1596.9854187121857,
                1609.2264442514315,
                1621.0583236164161,
                1632.4762765053038,
                1643.4722032020084,
                1654.0311719855476,
                1664.1205869617102,
                1672.5698697331109,
                1672.461296315872,
                1672.4612701156518
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "7500.0",
              "FinalValue": "6946.316198892271",
              "Correction": -553.6838011077286,
              "LastUpdate": 2.4942148797890695E-05,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                7500,
                7462.97946439827,
                7425.689029147715,
                7388.089697930617,
                7350.142226073506,
                7311.8070258041325,
                7273.044014503029,
                7233.812372522637,
                7194.070147689288,
                7153.7735788065675,
                7112.875851749423,
                7071.324556300417,
                7029.0555797072775,
                6985.973941549261,
                6946.771899844202,
                6946.316173950123,
                6946.316198892271
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vz",
              "InitialValue": "0.0",
              "FinalValue": "1389.263239777969",
              "Correction": 1389.263239777969,
              "LastUpdate": 9.462600837650825E-05,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                0,
                100,
                200,
                300,
                400,
                500,
                600,
                700,
                800,
                900,
                1000,
                1100,
                1200,
                1300,
                1388.840927291291,
                1389.2631451519605,
                1389.263239777969
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "X",
              "DesiredValue": "-2500000.0",
              "ParentName": "Propagate",
              "CurrentValue": "-2499999.9999998566",
              "Unit": null,
              "Difference": 1.434236764907837E-07,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                -2438315.4232145697,
                -2442234.4515713775,
                -2446133.487128895,
                -2450022.136561363,
                -2453911.4314638134,
                -2457814.2616258324,
                -2461746.0076293694,
                -2465725.49889684,
                -2469776.5341865257,
                -2473930.4429211337,
                -2478230.7536080107,
                -2482742.6842169366,
                -2487575.810274992,
                -2492954.806731596,
                -2498993.266250391,
                -2499999.9781223466,
                -2499999.9999998566
              ]
            },
            {
              "Enable": true,
              "Name": "Y",
              "DesiredValue": "10000000.0",
              "ParentName": "Propagate",
              "CurrentValue": "9999999.999998543",
              "Unit": null,
              "Difference": -1.4565885066986084E-06,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                11748308.707777388,
                11613409.805640506,
                11480292.780274013,
                11348892.170739032,
                11219139.255460642,
                11090961.400264064,
                10964281.048624191,
                10839014.129664147,
                10715067.462123193,
                10592334.300250756,
                10470686.11107041,
                10349955.70733018,
                10229896.682696898,
                10110056.116688414,
                10002283.084655745,
                9999999.890968636,
                9999999.999998543
              ]
            },
            {
              "Enable": true,
              "Name": "Z",
              "DesiredValue": "2000000.0",
              "ParentName": "Propagate",
              "CurrentValue": "1999999.9999990107",
              "Unit": null,
              "Difference": -9.89297404885292E-07,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                0,
                155613.5838379516,
                309204.7818111142,
                460831.9322619143,
                610553.587148965,
                758428.2080423416,
                904513.7931320542,
                1048867.3883200253,
                1191544.396109603,
                1332597.5116780314,
                1472074.9144658735,
                1610016.792104306,
                1746447.425266696,
                1881351.5569427158,
                1999717.3240472788,
                1999999.849149697,
                1999999.9999990107
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.VxVyVz_XYZ",
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
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 1672.4612701156518,
              "Vy": 6946.316198892271,
              "Vz": 1389.263239777969
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 8540531.041672494,
              "Eccentricity": 0.2378897952366065,
              "Inclination": 11.309932474016364,
              "RAAN": 0,
              "ArgOfPeriapsis": 271.722612012295,
              "MeanAnomaly": 61.431917013002746,
              "TrueAnomaly": 88.277387987705,
              "Period": 7854.857198172786
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 13.283934931605735,
              "VelocityAzimuth": 78.69006752598364,
              "VelocityMagnitude": 7278.632274296347
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
              "Vx": 1672.4612701156518,
              "Vy": 6946.316198892271,
              "Vz": 1389.263239777969
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 8540531.041672494,
              "Eccentricity": 0.2378897952366065,
              "Inclination": 11.309932474016364,
              "RAAN": 0,
              "ArgOfPeriapsis": 271.722612012295,
              "MeanAnomaly": 61.431917013002746,
              "TrueAnomaly": 88.277387987705,
              "Period": 7854.857198172786
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 13.283934931605735,
              "VelocityAzimuth": 78.69006752598364,
              "VelocityMagnitude": 7278.632274296347
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
            "RadiusOfApoapsis": 10572236.222387847
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
              "Vx": 1672.4612701156518,
              "Vy": 6946.316198892271,
              "Vz": 1389.263239777969
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 8540531.041672494,
              "Eccentricity": 0.2378897952366065,
              "Inclination": 11.309932474016364,
              "RAAN": 0,
              "ArgOfPeriapsis": 271.722612012295,
              "MeanAnomaly": 61.431917013002746,
              "TrueAnomaly": 88.277387987705,
              "Period": 7854.857198172786
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 13.283934931605735,
              "VelocityAzimuth": 78.69006752598364,
              "VelocityMagnitude": 7278.632274296347
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
            "Epoch": "2000-01-01T12:48:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -2499999.9999998566,
              "Y": 9999999.999998543,
              "Z": 1999999.9999990107,
              "Vx": -5158.84789480112,
              "Vy": -1592.8202572540695,
              "Vz": -318.5640514507027
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 8540531.04167284,
              "Eccentricity": 0.23788979523665363,
              "Inclination": 11.309932474016366,
              "RAAN": 0,
              "ArgOfPeriapsis": 271.7226120123081,
              "MeanAnomaly": 198.92645991201323,
              "TrueAnomaly": 192.05153498571994,
              "Period": 7854.857198173263
            },
            "Spherical": {
              "RightAscension": 104.03624346792768,
              "Declination": 10.980575427608365,
              "RadiusMagnitude": 10499999.99999839,
              "HorizFPA": -3.7034825379383465,
              "VelocityAzimuth": 92.72631099390557,
              "VelocityMagnitude": 5408.5368657789095
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 11.023158251026699,
            "Geodetic_Longitude": 171.30805858640613,
            "Geodetic_Altitude": 4122640.4948405493,
            "Geocentric_Latitude": 10.979425773750593,
            "Geocentric_Longitude": 171.30805858640613
          },
          "DurationSec": 3000,
          "Results": {
            "X": -2499999.9999998566,
            "Y": 9999999.999998543,
            "Z": 1999999.9999990107
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
          "Vx": 1672.4612701156518,
          "Vy": 6946.316198892271,
          "Vz": 1389.263239777969
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 8540531.041672494,
          "Eccentricity": 0.2378897952366065,
          "Inclination": 11.309932474016364,
          "RAAN": 0,
          "ArgOfPeriapsis": 271.722612012295,
          "MeanAnomaly": 61.431917013002746,
          "TrueAnomaly": 88.277387987705,
          "Period": 7854.857198172786
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 8000000,
          "HorizFPA": 13.283934931605735,
          "VelocityAzimuth": 78.69006752598364,
          "VelocityMagnitude": 7278.632274296347
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
        "Epoch": "2000-01-01T12:48:55.81600000000617Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -2499999.9999998566,
          "Y": 9999999.999998543,
          "Z": 1999999.9999990107,
          "Vx": -5158.84789480112,
          "Vy": -1592.8202572540695,
          "Vz": -318.5640514507027
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 8540531.04167284,
          "Eccentricity": 0.23788979523665363,
          "Inclination": 11.309932474016366,
          "RAAN": 0,
          "ArgOfPeriapsis": 271.7226120123081,
          "MeanAnomaly": 198.92645991201323,
          "TrueAnomaly": 192.05153498571994,
          "Period": 7854.857198173263
        },
        "Spherical": {
          "RightAscension": 104.03624346792768,
          "Declination": 10.980575427608365,
          "RadiusMagnitude": 10499999.99999839,
          "HorizFPA": -3.7034825379383465,
          "VelocityAzimuth": 92.72631099390557,
          "VelocityMagnitude": 5408.5368657789095
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 11.023158251026699,
        "Geodetic_Longitude": 171.30805858640613,
        "Geodetic_Altitude": 4122640.4948405493,
        "Geocentric_Latitude": 10.979425773750593,
        "Geocentric_Longitude": 171.30805858640613
      },
      "DurationSec": 3000,
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
          1672.4612701156518,
          6946.316198892271,
          1389.263239777969,
          43.41599999999744,
          8066777.715988401,
          301508.5030528735,
          60301.70061055362,
          1404.5583468220266,
          6941.311369564383,
          1388.2622739123917,
          86.67299999999523,
          8121853.598131758,
          601486.997327771,
          120297.39946551216,
          1142.7621531004331,
          6926.729900697444,
          1385.3459801390047,
          176.61500000000524,
          8200753.143362474,
          1222126.9012438292,
          244425.38024868042,
          615.650170012407,
          6868.019466145295,
          1373.6038932285792,
          268.39800000000105,
          8233527.067083074,
          1848364.312083541,
          369672.86241657904,
          102.81477848132874,
          6772.379358703244,
          1354.4758717401758,
          362.3920000000071,
          8219565.157313438,
          2479022.2808716097,
          495804.45617414877,
          -395.2832238410371,
          6641.545218898716,
          1328.3090437792791,
          458.39599999999336,
          8158339.458037141,
          3109008.542793451,
          621801.708558473,
          -875.3624655824783,
          6477.91385482712,
          1295.5827709649716,
          566.4470000000001,
          8036104.993333859,
          3797589.4673718167,
          759517.8934740982,
          -1381.0955681346932,
          6262.448742257901,
          1252.4897484511428,
          678.369000000006,
          7853987.281070888,
          4484597.747231154,
          896919.5494459175,
          -1866.8891385717593,
          6009.467685251156,
          1201.8935370498114,
          793.2259999999951,
          7612811.016341518,
          5158629.609682817,
          1031725.921936203,
          -2326.1810841089205,
          5723.329119774824,
          1144.665823954565,
          911.1410000000033,
          7312685.496325072,
          5815013.250400603,
          1163002.6500797144,
          -2757.73924163217,
          5406.254566775117,
          1081.2509133546457,
          1032.1199999999953,
          6954314.696151515,
          6448337.310473567,
          1289667.462094263,
          -3160.0976342500826,
          5060.621448572073,
          1012.1242897140611,
          1156.119000000006,
          6539000.178610154,
          7052964.003245507,
          1410592.8006486087,
          -3531.882972522273,
          4688.833963112054,
          937.7667926220835,
          1283.0580000000045,
          6068663.48130198,
          7623198.124054212,
          1524639.6248103098,
          -3871.8695088586214,
          4293.284887998918,
          858.6569775994839,
          1412.8220000000001,
          5545877.10970387,
          8153382.9938373165,
          1630676.5987668938,
          -4178.974456195262,
          3876.3481236718885,
          775.2696247341071,
          1544.5069999999978,
          4977241.196278921,
          8635400.947072623,
          1727080.1894139217,
          -4450.802827348986,
          3442.8837110459476,
          688.5767422089492,
          1677.8239999999932,
          4367702.228767382,
          9064669.346367594,
          1812933.869272886,
          -4687.010281444434,
          2995.701282161798,
          599.1402564321505,
          1812.7170000000042,
          3721491.505106369,
          9437870.998323245,
          1887574.1996639904,
          -4887.803038630201,
          2536.637537498422,
          507.3275074995073,
          1949.0580000000045,
          3043385.37623945,
          9751792.03396853,
          1950358.4067930256,
          -5053.234580798098,
          2067.578069298791,
          413.5156138596139,
          2086.6630000000005,
          2338668.8755901568,
          10003508.624421233,
          2000701.7248835484,
          -5183.281834117025,
          1590.445359565654,
          318.0890719130198,
          2225.3040000000037,
          1613078.0802838323,
          10190532.82013225,
          2038106.5640257387,
          -5277.89323144989,
          1107.1909145156546,
          221.43818290305364,
          2364.720000000001,
          872718.0676201225,
          10310927.801752439,
          2062185.560349768,
          -5337.020309233856,
          619.7860754926742,
          123.95721509849159,
          2504.620999999999,
          123996.91168513242,
          10363391.808933815,
          2072678.3617860395,
          -5360.636928882016,
          130.23511210993752,
          26.04702242197844,
          2643.150999999998,
          -618212.3175130866,
          10347879.701183386,
          2069575.940235955,
          -5349.082548671784,
          -354.03187478374707,
          -70.80637495672465,
          2778.8859999999986,
          -1341594.1330559528,
          10267693.437527727,
          2053538.687504829,
          -5304.091078245484,
          -827.1864102895596,
          -165.43728205785416,
          2918.399000000005,
          -2076335.0430178451,
          10118494.091363488,
          2023698.8182719916,
          -5222.914925618443,
          -1311.2025372442931,
          -262.2405074487671,
          3000,
          -2499999.9999998566,
          9999999.999998543,
          1999999.9999990107,
          -5158.84789480112,
          -1592.8202572540695,
          -318.5640514507027
        ]
      }
    ]
  }
}
             */

        }

    }
}