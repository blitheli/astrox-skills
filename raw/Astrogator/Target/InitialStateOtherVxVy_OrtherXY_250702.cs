using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator  自变量和约束均为 另一卫星的LVLH坐标系 下参数

        ## 主序列飞行段 TargetSequence

            Initial_State（初始状态）
                - 初始化航天器在 GEO1 LVLH 坐标系中的位置和速度
          
            Propagate
                - 使用地球点质量模型传播
                - 停止条件：43200s
                - Resuts:            
                    卫星1 LVLH: X，Y
            
        ## 目标求解配置

        **DC: 微分校正器**
          输出优化过程
          - 控制变量：
            - InitialState.Cartesian.Vx
            - InitialState.Cartesian.Vy
          - 约束条件：
            - X
            - Y

        结果:
            与STK对比，优化后的初始速度相对误差： < 0.001 m/s

            TBD:    自变量默认为地心惯性系了！！！待后续修正算法
        20250727    更新了测试用例，自变量为LVLH坐标系下的速度
        */
        [TestMethod()]
        public void InitialStateOtherVxVy_OrtherXY_250702()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateOtherVxVy_OrtherXY_250702.json");

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
         X:    42165.9999999998981366 km              Vx:        0.0030652055862014 km/sec     
         Y:        9.9619469809174568 km              Vy:        3.0629061978473899 km/sec     
         Z:        0.8715574274765803 km              Vz:        0.2679695697549426 km/sec      
             */
            //  初始段 微分迭代后的初始速度，
            Assert.AreEqual(42165999.999999898, seg0.InitialState.Cartesian.X, 0.001);
            Assert.AreEqual(9961.9469809174568, seg0.InitialState.Cartesian.Y, 0.001);
            Assert.AreEqual(871.5574274765803, seg0.InitialState.Cartesian.Z, 0.001);
            Assert.AreEqual(3.0652055862014, seg0.InitialState.Cartesian.Vx, 0.001);
            Assert.AreEqual(3062.9061978473899, seg0.InitialState.Cartesian.Vy, 0.001);
            Assert.AreEqual(267.9695697549426, seg0.InitialState.Cartesian.Vz, 0.002);


            /*
              STK 结果                                                                         
Parameter Set Type:  Cartesian                                                                            
         X:   -42166.1930949228408281 km              Vx:        0.0150262821644949 km/sec                
         Y:     -153.4543961850153266 km              Vy:       -3.0628367626934803 km/sec                
         Z:      -13.4255200344052277 km              Vz:       -0.2679634949661253 km/sec  
            
            */
            //  最后一段 Propagate结果 地心惯性系下位置
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(-42166193.0949228408281, seg3.FinalState.Cartesian.X, 0.5);
            Assert.AreEqual(-153454.3961850153266, seg3.FinalState.Cartesian.Y, 0.5);
            Assert.AreEqual(-13425.5200344052277, seg3.FinalState.Cartesian.Z, 0.5);

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
          "TotalIterations": 3,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vx",
              "InitialValue": "0",
              "FinalValue": "3.7943782942739315",
              "Correction": 3.7943782942739315,
              "LastUpdate": -0.016433902174369575,
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
                3.810812196448301,
                3.7943782942739315
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "0",
              "FinalValue": "0.012636046276486335",
              "Correction": 0.012636046276486335,
              "LastUpdate": 0.0046431074620706374,
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
                0.007992938814415698,
                0.012636046276486335
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "X",
              "DesiredValue": "0.0",
              "ParentName": "Propagate",
              "CurrentValue": "0.04375969928059931",
              "Unit": null,
              "Difference": 0.04375969928059931,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1,
              "Weight": 1,
              "Values": [
                7.119940532043984,
                -256.7013189393201,
                0.04375969928059931
              ]
            },
            {
              "Enable": true,
              "Name": "Y",
              "DesiredValue": "-200000.0",
              "ParentName": "Propagate",
              "CurrentValue": "-200000.03925802436",
              "Unit": null,
              "Difference": -0.03925802436424419,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 1,
              "Weight": 1,
              "Values": [
                9977.530596371775,
                -200299.47963191802,
                -200000.03925802436
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.VxVy_XY",
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
              "X": 42166000.000000015,
              "Y": 9961.94698091746,
              "Z": 871.5574274765819,
              "Vx": 3.0652141895530294,
              "Vy": 3062.906195933456,
              "Vz": 267.96956958748996
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 42166390.874964274,
              "Eccentricity": 0.0012341372162056631,
              "Inclination": 4.999999999999999,
              "RAAN": 4.7225327901527634E-18,
              "ArgOfPeriapsis": 270.3719360906416,
              "MeanAnomaly": 89.50023396872892,
              "TrueAnomaly": 89.64165205583629,
              "Period": 86170.89943856942
            },
            "Spherical": {
              "RightAscension": 0.01353643948015075,
              "Declination": 0.0011842849878321527,
              "RadiusMagnitude": 42166001.185789496,
              "HorizFPA": 0.07070888916176606,
              "VelocityAzimuth": 85.0000001398969,
              "VelocityMagnitude": 3074.607527937588
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12498316483816448,
            "Geodetic_Longitude": 31.947353747521998,
            "Geodetic_Altitude": 35787864.287272036,
            "Geocentric_Latitude": 0.12485660615883398,
            "Geocentric_Longitude": 31.947353747522
          },
          "FinalState": {
            "Epoch": "2022-06-20T04:00:00Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 42166000.000000015,
              "Y": 9961.94698091746,
              "Z": 871.5574274765819,
              "Vx": 3.0652141895530294,
              "Vy": 3062.906195933456,
              "Vz": 267.96956958748996
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 42166390.874964274,
              "Eccentricity": 0.0012341372162056631,
              "Inclination": 4.999999999999999,
              "RAAN": 4.7225327901527634E-18,
              "ArgOfPeriapsis": 270.3719360906416,
              "MeanAnomaly": 89.50023396872892,
              "TrueAnomaly": 89.64165205583629,
              "Period": 86170.89943856942
            },
            "Spherical": {
              "RightAscension": 0.01353643948015075,
              "Declination": 0.0011842849878321527,
              "RadiusMagnitude": 42166001.185789496,
              "HorizFPA": 0.07070888916176606,
              "VelocityAzimuth": 85.0000001398969,
              "VelocityMagnitude": 3074.607527937588
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12498316483816448,
            "Geodetic_Longitude": 31.947353747521998,
            "Geodetic_Altitude": 35787864.287272036,
            "Geocentric_Latitude": 0.12485660615883398,
            "Geocentric_Longitude": 31.947353747522
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
              "X": 42166000.000000015,
              "Y": 9961.94698091746,
              "Z": 871.5574274765819,
              "Vx": 3.0652141895530294,
              "Vy": 3062.906195933456,
              "Vz": 267.96956958748996
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 42166390.874964274,
              "Eccentricity": 0.0012341372162056631,
              "Inclination": 4.999999999999999,
              "RAAN": 4.7225327901527634E-18,
              "ArgOfPeriapsis": 270.3719360906416,
              "MeanAnomaly": 89.50023396872892,
              "TrueAnomaly": 89.64165205583629,
              "Period": 86170.89943856942
            },
            "Spherical": {
              "RightAscension": 0.01353643948015075,
              "Declination": 0.0011842849878321527,
              "RadiusMagnitude": 42166001.185789496,
              "HorizFPA": 0.07070888916176606,
              "VelocityAzimuth": 85.0000001398969,
              "VelocityMagnitude": 3074.607527937588
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 0.12498316483816448,
            "Geodetic_Longitude": 31.947353747521998,
            "Geodetic_Altitude": 35787864.287272036,
            "Geocentric_Latitude": 0.12485660615883398,
            "Geocentric_Longitude": 31.947353747522
          },
          "FinalState": {
            "Epoch": "2022-06-20T16:00:00Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -42166192.990104534,
              "Y": -153454.17310004192,
              "Z": -13425.500516997126,
              "Vx": 15.0262744748661,
              "Vy": -3062.836768498723,
              "Vz": -267.9634954740132
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 42166390.87496471,
              "Eccentricity": 0.0012341372161950364,
              "Inclination": 5.000000000000002,
              "RAAN": 2.0816924538993263E-14,
              "ArgOfPeriapsis": 270.37193609550116,
              "MeanAnomaly": 269.97879576750205,
              "TrueAnomaly": 269.83737429386935,
              "Period": 86170.89943857075
            },
            "Spherical": {
              "RightAscension": 180.2085139071924,
              "Declination": -0.018242562182391474,
              "RadiusMagnitude": 42166474.357077174,
              "HorizFPA": -0.07071078079011091,
              "VelocityAzimuth": 94.9999668053024,
              "VelocityMagnitude": 3074.573026340234
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -0.14202272578709255,
            "Geodetic_Longitude": 31.64952468209812,
            "Geodetic_Altitude": 35788337.488117136,
            "Geocentric_Latitude": -0.14187891449529486,
            "Geocentric_Longitude": 31.649524682098114
          },
          "DurationSec": 43200,
          "Results": {
            "X": 0.04375969928059931,
            "Y": -200000.03925802436
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Target_GEO1_LVLH_XY",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2022-06-20T04:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 42166000.000000015,
          "Y": 9961.94698091746,
          "Z": 871.5574274765819,
          "Vx": 3.0652141895530294,
          "Vy": 3062.906195933456,
          "Vz": 267.96956958748996
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 42166390.874964274,
          "Eccentricity": 0.0012341372162056631,
          "Inclination": 4.999999999999999,
          "RAAN": 4.7225327901527634E-18,
          "ArgOfPeriapsis": 270.3719360906416,
          "MeanAnomaly": 89.50023396872892,
          "TrueAnomaly": 89.64165205583629,
          "Period": 86170.89943856942
        },
        "Spherical": {
          "RightAscension": 0.01353643948015075,
          "Declination": 0.0011842849878321527,
          "RadiusMagnitude": 42166001.185789496,
          "HorizFPA": 0.07070888916176606,
          "VelocityAzimuth": 85.0000001398969,
          "VelocityMagnitude": 3074.607527937588
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 0.12498316483816448,
        "Geodetic_Longitude": 31.947353747521998,
        "Geodetic_Altitude": 35787864.287272036,
        "Geocentric_Latitude": 0.12485660615883398,
        "Geocentric_Longitude": 31.947353747522
      },
      "FinalState": {
        "Epoch": "2022-06-20T16:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -42166192.990104534,
          "Y": -153454.17310004192,
          "Z": -13425.500516997126,
          "Vx": 15.0262744748661,
          "Vy": -3062.836768498723,
          "Vz": -267.9634954740132
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 42166390.87496471,
          "Eccentricity": 0.0012341372161950364,
          "Inclination": 5.000000000000002,
          "RAAN": 2.0816924538993263E-14,
          "ArgOfPeriapsis": 270.37193609550116,
          "MeanAnomaly": 269.97879576750205,
          "TrueAnomaly": 269.83737429386935,
          "Period": 86170.89943857075
        },
        "Spherical": {
          "RightAscension": 180.2085139071924,
          "Declination": -0.018242562182391474,
          "RadiusMagnitude": 42166474.357077174,
          "HorizFPA": -0.07071078079011091,
          "VelocityAzimuth": 94.9999668053024,
          "VelocityMagnitude": 3074.573026340234
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -0.14202272578709255,
        "Geodetic_Longitude": 31.64952468209812,
        "Geodetic_Altitude": 35788337.488117136,
        "Geocentric_Latitude": -0.14187891449529486,
        "Geocentric_Longitude": 31.649524682098114
      },
      "DurationSec": 43200,
      "Results": {}
    }
  ],
             */

        }

    }
}