using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator  约束为 另一卫星的LVLH坐标系 XYZ

            自变量: InitialState.Cartesian.Vx(Vy,Vz)
            约束: X,Y,Z
        
           # 飞行任务各段简要说明

        ## 主序列飞行段 TargetSequence

            Initial_State（初始状态）
                - 初始化航天器在地心惯性坐标系中的位置和速度
          
            Propagate
                - 使用地球点质量模型传播
                - 停止条件：1000s
                - Resuts:            
                    卫星1 LVLH: X，Y，Z
            
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
            与STK对比，优化后的初始速度相对误差： < 0.001 m/s

        */
        [TestMethod()]
        public void InitialStateVxVyVz_OrtherXYZ_250620()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateVxVyVz_OrtherXYZ_250620.json");

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
            /*  STK 初始段结果
             Parameter Set Type:  Cartesian                                                                 
         X:     8000.0000000000000000 km              Vx:        2.0095564878588523 km/sec     
         Y:        0.0000000000000000 km              Vy:        9.2829199774917921 km/sec     
         Z:        0.0000000000000000 km              Vz:        1.0049222479755064 km/sec     
             */
            //  初始段 微分迭代后的初始速度，
            Assert.AreEqual(2009.5564878588523, seg0.InitialState.Cartesian.Vx, 0.001);
            Assert.AreEqual(9282.9199774917921, seg0.InitialState.Cartesian.Vy, 0.001);
            Assert.AreEqual(1004.9222479755064, seg0.InitialState.Cartesian.Vz, 0.001);

            /*
              STK 结果                                                                         
                                                                                                                  
Parameter Set Type:  Cartesian                                                                            
         X:     7641.7011520073947395 km              Vx:       -1.9809060714193234 km/sec                
         Y:     8557.3148415301729983 km              Vy:        7.4999168058600816 km/sec                
         Z:      926.3718838507321607 km              Vz:        0.8119032884532756 km/sec   
            User-selected results:                                                                                    
    X =        9.9999418611545110 km                                                                      
    Y =       20.0000200340842120 km                                                                      
    Z =       29.9999734791520751 km  
            
            */
            //  最后一段 Propagate结果 地心惯性系下位置
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(7641701.1520073947395, seg3.FinalState.Cartesian.X, 0.1);
            Assert.AreEqual(8557314.8415301729983, seg3.FinalState.Cartesian.Y, 0.1);
            Assert.AreEqual(926371.8838507321607, seg3.FinalState.Cartesian.Z, 0.1);

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
          "TotalIterations": 20,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vx",
              "InitialValue": "1500",
              "FinalValue": "2009.5565403866578",
              "Correction": 509.55654038665784,
              "LastUpdate": 0.022911982676134812,
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
                1527.857538721962,
                1555.7912112877239,
                1583.8029142102669,
                1611.8944503368248,
                1640.0675375013006,
                1668.323818629149,
                1696.6648740459489,
                1725.0922372527098,
                1753.607415183523,
                1782.2119163484417,
                1810.9072912773056,
                1839.6951943515837,
                1868.5774858654966,
                1897.5564151664485,
                1926.6349911803386,
                1955.8178716266534,
                1985.1141860731382,
                2009.5336284039818,
                2009.5565403866578
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "7500",
              "FinalValue": "9282.919995560882",
              "Correction": 1782.9199955608824,
              "LastUpdate": -0.027146348368660256,
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
                7600,
                7700,
                7800,
                7900,
                8000,
                8100,
                8200,
                8300,
                8400,
                8500,
                8600,
                8700,
                8800,
                8900,
                9000,
                9100,
                9200,
                9282.947141909252,
                9282.919995560882
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vz",
              "InitialValue": "2000",
              "FinalValue": "1004.9222672558042",
              "Correction": -995.0777327441958,
              "LastUpdate": 0.035573434939578874,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                2000,
                1943.6526069371143,
                1887.365384781149,
                1831.13903137764,
                1774.9741795600694,
                1718.8714040296181,
                1662.8312293891015,
                1606.8541395577586,
                1550.9405890115543,
                1495.0910175894248,
                1439.3058699102858,
                1383.5856231553225,
                1327.9308292076344,
                1272.3421830499235,
                1216.8206454990818,
                1161.3676907860515,
                1105.9858991945398,
                1050.6808394066584,
                1004.8866938208646,
                1004.9222672558042
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "X",
              "DesiredValue": "10000.0",
              "ParentName": "Propagate",
              "CurrentValue": "10000.000018001567",
              "Unit": null,
              "Difference": 1.8001566786551848E-05,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1,
              "Weight": 1,
              "Values": [
                -1706553.758279188,
                -1611207.7875204077,
                -1515737.8663271628,
                -1420146.948906518,
                -1324437.932775224,
                -1228613.652064002,
                -1132676.8698398245,
                -1036630.2688029461,
                -940476.4392850765,
                -844217.8635855996,
                -747856.893803071,
                -651395.7193051409,
                -554836.3160400798,
                -458180.3615686944,
                -361429.0806867549,
                -264582.9303522452,
                -167640.83916944335,
                -70597.78377582802,
                9999.682232604067,
                10000.000018001567
              ]
            },
            {
              "Enable": true,
              "Name": "Y",
              "DesiredValue": "20000.0",
              "ParentName": "Propagate",
              "CurrentValue": "20000.000012012253",
              "Unit": null,
              "Difference": 1.2012253137072548E-05,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1,
              "Weight": 1,
              "Values": [
                -54485.21401566279,
                -50332.40101810545,
                -46174.64542744728,
                -42012.1927305077,
                -37845.29524633521,
                -33674.214173495886,
                -29499.222131576855,
                -25320.60663078248,
                -21138.675171421084,
                -16953.761942162004,
                -12766.238064220117,
                -8576.526876546384,
                -4385.128080561117,
                -192.65889195728232,
                4000.0711114642327,
                8191.906886505691,
                12380.977806974057,
                16563.51915910047,
                20014.10725772503,
                20000.000012012253
              ]
            },
            {
              "Enable": true,
              "Name": "Z",
              "DesiredValue": "30000.0",
              "ParentName": "Propagate",
              "CurrentValue": "29999.999995027218",
              "Unit": null,
              "Difference": -4.972782335244119E-06,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1,
              "Weight": 1,
              "Values": [
                1230496.0685949973,
                1163755.0857292244,
                1096929.978355799,
                1030023.1901295881,
                963037.1533420673,
                895974.2915041931,
                828837.0235372565,
                761627.7699999037,
                694348.962117302,
                627003.0553339799,
                559592.549328623,
                492120.0189256036,
                424588.1636444527,
                356999.8915196855,
                289358.4729240013,
                221667.8553474247,
                153933.42302495387,
                86164.41754561279,
                29955.040956291035,
                29999.999995027218
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
            "Epoch": "2022-06-20T04:00:00Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 2009.5565403866578,
              "Vy": 9282.919995560882,
              "Vz": 1004.9222672558042
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 47287375.35884462,
              "Eccentricity": 0.8390330822101191,
              "Inclination": 6.1784925769661285,
              "RAAN": 0,
              "ArgOfPeriapsis": 333.330955109368,
              "MeanAnomaly": 1.3133379688401896,
              "TrueAnomaly": 26.669044890632023,
              "Period": 102336.0841923511
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 12.146009516790052,
              "VelocityAzimuth": 83.82150742303388,
              "VelocityMagnitude": 9550.957538185445
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12431296638091047,
            "Geodetic_Longitude": 31.933479895133658,
            "Geodetic_Altitude": 1621863.099962477,
            "Geocentric_Latitude": 0.1236494841586833,
            "Geocentric_Longitude": 31.933479895133658
          },
          "FinalState": {
            "Epoch": "2022-06-20T04:00:00Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 2009.5565403866578,
              "Vy": 9282.919995560882,
              "Vz": 1004.9222672558042
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 47287375.35884462,
              "Eccentricity": 0.8390330822101191,
              "Inclination": 6.1784925769661285,
              "RAAN": 0,
              "ArgOfPeriapsis": 333.330955109368,
              "MeanAnomaly": 1.3133379688401896,
              "TrueAnomaly": 26.669044890632023,
              "Period": 102336.0841923511
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 12.146009516790052,
              "VelocityAzimuth": 83.82150742303388,
              "VelocityMagnitude": 9550.957538185445
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12431296638091047,
            "Geodetic_Longitude": 31.933479895133658,
            "Geodetic_Altitude": 1621863.099962477,
            "Geocentric_Latitude": 0.1236494841586833,
            "Geocentric_Longitude": 31.933479895133658
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
            "Epoch": "2022-06-20T04:00:00Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 8000000,
              "Y": 0,
              "Z": 0,
              "Vx": 2009.5565403866578,
              "Vy": 9282.919995560882,
              "Vz": 1004.9222672558042
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 47287375.35884462,
              "Eccentricity": 0.8390330822101191,
              "Inclination": 6.1784925769661285,
              "RAAN": 0,
              "ArgOfPeriapsis": 333.330955109368,
              "MeanAnomaly": 1.3133379688401896,
              "TrueAnomaly": 26.669044890632023,
              "Period": 102336.0841923511
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 12.146009516790052,
              "VelocityAzimuth": 83.82150742303388,
              "VelocityMagnitude": 9550.957538185445
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12431296638091047,
            "Geodetic_Longitude": 31.933479895133658,
            "Geodetic_Altitude": 1621863.099962477,
            "Geocentric_Latitude": 0.1236494841586833,
            "Geocentric_Longitude": 31.933479895133658
          },
          "FinalState": {
            "Epoch": "2022-06-20T04:16:40Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 7641701.212413893,
              "Y": 8557314.863866415,
              "Z": 926371.9022388109,
              "Vx": -1980.906001366384,
              "Vy": 7499.916838147135,
              "Vz": 811.9033059452128
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 47287375.35884712,
              "Eccentricity": 0.8390330822101283,
              "Inclination": 6.178492576966128,
              "RAAN": 0,
              "ArgOfPeriapsis": 333.3309551093672,
              "MeanAnomaly": 4.8311587144855865,
              "TrueAnomaly": 75.0698902149879,
              "Period": 102336.0841923592
            },
            "Spherical": {
              "RightAscension": 48.235075314650295,
              "Declination": 4.616369210303723,
              "RadiusMagnitude": 11510056.472511966,
              "HorizFPA": 33.687741513133474,
              "VelocityAzimuth": 85.88909951147016,
              "VelocityMagnitude": 7799.482556142878
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 4.717031752132363,
            "Geodetic_Longitude": 75.99793252747962,
            "Geodetic_Altitude": 5132063.314894756,
            "Geocentric_Latitude": 4.699612030137949,
            "Geocentric_Longitude": 75.99793252747962
          },
          "DurationSec": 1000,
          "Results": {
            "X": 10000.000018001567,
            "Y": 20000.000012012253,
            "Z": 29999.999995027218,
            "Duration": 1000
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Target_XYZ",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2022-06-20T04:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8000000,
          "Y": 0,
          "Z": 0,
          "Vx": 2009.5565403866578,
          "Vy": 9282.919995560882,
          "Vz": 1004.9222672558042
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 47287375.35884462,
          "Eccentricity": 0.8390330822101191,
          "Inclination": 6.1784925769661285,
          "RAAN": 0,
          "ArgOfPeriapsis": 333.330955109368,
          "MeanAnomaly": 1.3133379688401896,
          "TrueAnomaly": 26.669044890632023,
          "Period": 102336.0841923511
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 8000000,
          "HorizFPA": 12.146009516790052,
          "VelocityAzimuth": 83.82150742303388,
          "VelocityMagnitude": 9550.957538185445
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 0.12431296638091047,
        "Geodetic_Longitude": 31.933479895133658,
        "Geodetic_Altitude": 1621863.099962477,
        "Geocentric_Latitude": 0.1236494841586833,
        "Geocentric_Longitude": 31.933479895133658
      },
      "FinalState": {
        "Epoch": "2022-06-20T04:16:40Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 7641701.212413893,
          "Y": 8557314.863866415,
          "Z": 926371.9022388109,
          "Vx": -1980.906001366384,
          "Vy": 7499.916838147135,
          "Vz": 811.9033059452128
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 47287375.35884712,
          "Eccentricity": 0.8390330822101283,
          "Inclination": 6.178492576966128,
          "RAAN": 0,
          "ArgOfPeriapsis": 333.3309551093672,
          "MeanAnomaly": 4.8311587144855865,
          "TrueAnomaly": 75.0698902149879,
          "Period": 102336.0841923592
        },
        "Spherical": {
          "RightAscension": 48.235075314650295,
          "Declination": 4.616369210303723,
          "RadiusMagnitude": 11510056.472511966,
          "HorizFPA": 33.687741513133474,
          "VelocityAzimuth": 85.88909951147016,
          "VelocityMagnitude": 7799.482556142878
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 4.717031752132363,
        "Geodetic_Longitude": 75.99793252747962,
        "Geodetic_Altitude": 5132063.314894756,
        "Geocentric_Latitude": 4.699612030137949,
        "Geocentric_Longitude": 75.99793252747962
      },
      "DurationSec": 1000,
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
        "epoch": "2022-06-20T04:00:00.000Z",
        "interval": "2022-06-20T04:00:00.000Z/2022-06-20T04:16:40.000Z",
        "cartesian": null,
        "cartesianVelocity": [
          0,
          8000000,
          0,
          0,
          2009.5565403866578,
          9282.919995560882,
          1004.9222672558042,
          28.00499999999738,
          8053847.121598148,
          259942.0023857197,
          28140.014835567683,
          1836.412509106152,
          9280.126575643179,
          1004.6198656538903,
          56.286999999996624,
          8103344.218404956,
          522297.5965595137,
          56541.31298856797,
          1664.328433573576,
          9271.805896457046,
          1003.7191107409067,
          112.26200000000244,
          8187170.359123762,
          1040490.6056865109,
          112638.2839693618,
          1332.9317984179854,
          9240.098796094713,
          1000.2866594002228,
          174.99900000000343,
          8259555.877113677,
          1618501.3876314973,
          175210.82642025256,
          977.6425953860283,
          9182.778952060491,
          994.0815011468752,
          245.02700000000186,
          8314764.160372479,
          2258623.841834497,
          244507.26636658877,
          603.1107305706412,
          9095.334369223085,
          984.6151900630539,
          314.26000000000204,
          8344395.5790225705,
          2884731.417025409,
          312286.52594307606,
          256.9384177854663,
          8988.614882912745,
          973.06227479559,
          382.35599999999977,
          8350977.0075208545,
          3492785.917766935,
          378111.4504056885,
          -59.69331138341545,
          8867.808394204498,
          959.9843714429855,
          449.46600000000035,
          8337145.567120142,
          4083563.9235544545,
          442066.1083478833,
          -348.7414019036885,
          8736.713491519838,
          945.7926960978845,
          516.5469999999987,
          8304684.581205498,
          4664976.289298766,
          505006.8891661749,
          -615.436445801861,
          8596.637577159836,
          930.6287815630293,
          587.4420000000027,
          8251745.772189823,
          5268972.662703793,
          570392.5011575222,
          -874.140354671116,
          8441.551673479787,
          913.8399610173285,
          662.1399999999994,
          8177026.498686038,
          5893270.1803704845,
          637975.8130029106,
          -1122.4051962989063,
          8273.022339075982,
          895.5958222216508,
          740.8899999999994,
          8079159.062746712,
          6537666.704606285,
          707734.9422915902,
          -1358.9818260885154,
          8092.276587221441,
          876.0291954665588,
          824.0270000000019,
          7956690.266475015,
          7202461.634398611,
          779702.3004533042,
          -1583.0055008307472,
          7900.499010571742,
          855.2683187997557,
          911.9539999999979,
          7808044.964452599,
          7888247.321905137,
          853942.0125557673,
          -1793.8640066786622,
          7698.843601902395,
          833.4381176797091,
          1000,
          7641701.212413893,
          8557314.863866415,
          926371.9022388109,
          -1980.906001366384,
          7499.916838147135,
          811.9033059452128
        ]
      }
    ]
  }
}
             */

        }

    }
}