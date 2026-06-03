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
            自变量:ImpulsiveMnvr.Spherical.Magnitude
            约束: 半长轴
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **Initial_State（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg
          - 结果计算：远地点距离（RadiusOfApoapsis）

        2. **Manuber1(脉冲机动）**
          Resuts:            
            - sma:

        ## 目标求解配置

        **DC: Epoch_Altitude（霍曼转移微分校正器）**
          输出优化过程
          - 控制变量：
            - ImpulsiveMnvr.Spherical.Magnitude
          - 约束条件：
            - SemimajorAxis 

        结果:
            与STK的速度一致，相对偏差约为1e-10 km/s

            20250414    增加最后脉冲结果信息的校对
        */
        [TestMethod()]
        public void ImpulsiveMnvrAlongVelocity_Sma_250323()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ImpulsiveMnvrAlongVelocity_Sma_250323.json");

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
              STK 结果， 脉冲段
            Maneuver Start: 1 Jan 2000 11:58:55.816 UTCG;   2451544.99925713 UTC Julian Date             
  Maneuver Stop:  1 Jan 2000 11:58:55.816 UTCG;   2451544.99925713 UTC Julian Date             
  Duration:  0 sec                                                                             
  Fuel Used:                        0 kg                                                       
  DeltaV Magnitude:        1280.081385147701 m/sec                                             
  Estimated Equivalent Finite Burn Duration:        2075.897062738251 sec                      
  Estimated Fuel Used:  352.804 kg  (Update mass OFF)                                          
  Maneuver Direction Specification:  Along Velocity Vector                                     
                                                                                               
DeltaV vector with respect to VNC(Earth) axes:                                                 
                                                                                               
       X (Velocity):         1280.081385147701 m/sec                                           
         Y (Normal):                         0 m/sec                                           
      Z (Co-Normal):    -8.526512829121202e-14 m/sec                                           
            Azimuth:                         0 deg                                             
          Elevation:                         0 deg                                             
          Magnitude:         1280.081385147701 m/sec                                           
                                                                                               
DeltaV vector with respect to Earth Inertial axes:                                             
                                                                                               
              X:         251.0446139175003 m/sec                                               
              Y:         1255.223069587501 m/sec                                               
              Z:                         0 m/sec                                               
        Azimuth:         78.69006752597979 deg                                                 
      Elevation:                         0 deg                                                 
      Magnitude:         1280.081385147701 m/sec                                               
                                                                                               
DeltaV vector with respect to spacecraft body axes:                                            
                                                                                               
              X:         1280.081385147701 m/sec                                               
              Y:                         0 m/sec                                               
              Z:                         0 m/sec                                               
        Azimuth:                         0 deg                                                 
      Elevation:                         0 deg                                                 
      Magnitude:         1280.081385147701 m/sec                                               
                                                                                               
Attitude with respect to Earth Inertial axes:                                                  
  ---  -----------------------                                                                 
  qx:       0.5468354723173562                                                                 
  qy:        0.448297854350714                                                                 
  qz:        0.448297854350714                                                                 
  qs:       0.5468354723173562                                             

            UTC Gregorian Date: 1 Jan 2000 11:58:55.816  UTC Julian Date: 2451544.99925713                 
            Julian Ephemeris Date: 2451545                                                                 
            Time past epoch: -7.0897e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)     
                                                                                               
            State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
            Parameter Set Type:  Cartesian                                                                 
                     X:     8000.0000000000000000 km              Vx:        1.7510446145964025 km/sec     
                     Y:        0.0000000000000000 km              Vy:        8.7552230729820106 km/sec     
                     Z:        0.0000000000000000 km              Vz:        0.0000000000000000 km/sec     
                                                                                               
            Parameter Set Type:  Keplerian                                                                 
                   sma:    19999.9999999835818016 km            RAAN:                         0 deg        
                   ecc:        0.6201736729457368                  w:         330.2551187030484 deg        
                   inc:                         0 deg             TA:         29.74488129695164 deg        
                                                                                               
            Parameter Set Type:  Spherical                                                                 
             Right Asc:                         0 deg     Horiz. FPA:         11.30993247402021 deg        
                  Decl:                         0 deg        Azimuth:                        90 deg        
                   |R|:     8000.0000000000000000 km             |V|:        8.9286106589986112 km/sec     

            我的结果
              "Vx": 1751.0446146025429,
              "Vy": 8755.223073012714,
              "Vz": -1.1368683772161603E-13
            */
            //  最后一段
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(1.7510446145964025, seg3.FinalState.Cartesian.Vx*0.001, 1e-11);
            Assert.AreEqual(8.7552230729820106, seg3.FinalState.Cartesian.Vy * 0.001, 1e-10);

            var manvInfo = (seg3 as MCSManeuverImpulsiveResults).ManeuverInformation;
            Assert.AreEqual(0, manvInfo.FuelUsed,1e-5);
            Assert.AreEqual(1280.081385147701, manvInfo.DeltaV_Mag, 1e-5);
            Assert.AreEqual(352.804, manvInfo.EstimatedFuelUsed, 1e-3);
            Assert.AreEqual(1280.081385147701, manvInfo.DeltaV_VNC[0], 1e-5);
            Assert.AreEqual(0, manvInfo.DeltaV_VNC[1], 1e-10);
            Assert.AreEqual(0, manvInfo.DeltaV_VNC[2], 1e-10);
            Assert.AreEqual(251.0446139175003, manvInfo.DeltaV_Inertial[0], 1e-5);  
            Assert.AreEqual(1255.223069587501, manvInfo.DeltaV_Inertial[1], 1e-5);
            Assert.AreEqual(0, manvInfo.DeltaV_Inertial[2], 1e-10);
            Assert.AreEqual(78.69006752597979, manvInfo.DeltaV_Inertial[3], 1e-7);  //  Azimuth
            Assert.AreEqual(0, manvInfo.DeltaV_Inertial[4], 1e-7);      //  Elevation

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
          "TotalIterations": 6,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "ImpulsiveMnvr.Spherical.Magnitude",
              "InitialValue": "0",
              "FinalValue": "280.08138864074607",
              "Correction": 280.08138864074607,
              "LastUpdate": -3.189495339483983E-05,
              "Dimension": "",
              "MaxStep": 500,
              "ParentName": "Maneuver1",
              "Perturbation": 1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                0,
                354.6538382178626,
                285.44393492519663,
                280.11386893289165,
                280.08142053569946,
                280.08138864074607
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "Sma",
              "DesiredValue": "20000000.0",
              "ParentName": "Maneuver1",
              "CurrentValue": "20000000.00054466",
              "Unit": null,
              "Difference": 0.0005446597933769226,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                16038289.549259435,
                21438425.066387516,
                20096589.500174526,
                20000582.062820558,
                20000000.572099723,
                20000000.00054466
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: AlongVelocityDv_Sma",
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
          "$type": "ManeuverImpulsiveResult",
          "ManeuverInformation": {
            "Start": "2000-01-01T11:58:55.81600000000617Z",
            "Stop": "2000-01-01T11:58:55.81600000000617Z",
            "UpdateMass": false,
            "Duration": 0,
            "FuelUsed": 0,
            "EstimatedFuelUsed": 352.80431599299436,
            "DeltaV_Mag": 1280.0813886407461,
            "ManeuverAttitudeName": "AgVAAttitudeControlImpulsiveVelocityVector",
            "DeltaV_Inertial": [
              251.04461460254288,
              1255.223073012714,
              -1.1368683772161603E-13,
              78.69006752597979,
              -1.4210854715202004E-14,
              1280.081388640746
            ],
            "DeltaV_VNC": [
              1280.081388640746,
              5.684341886080802E-14,
              1.5938234496740141E-13,
              2.544281967318474E-15,
              0,
              1280.081388640746
            ],
            "DeltaV_Body": null,
            "Quanternion": null
          },
          "TypeName": "ManeuverImpulsive",
          "Name": "Maneuver1",
          "Description": "轨道机动段",
          "UserComment": "轨道机动段",
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
              "Vx": 1751.0446146025429,
              "Vy": 8755.223073012714,
              "Vz": -1.1368683772161603E-13
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 20000000.00054466,
              "Eccentricity": 0.6201736729561758,
              "Inclination": 7.439874385057628E-16,
              "RAAN": 0,
              "ArgOfPeriapsis": 330.2551187033699,
              "MeanAnomaly": 5.664753147188524,
              "TrueAnomaly": 29.74488129663015,
              "Period": 28148.546498007097
            },
            "Spherical": {
              "RightAscension": 0,
              "Declination": 0,
              "RadiusMagnitude": 8000000,
              "HorizFPA": 11.309932474020208,
              "VelocityAzimuth": 90,
              "VelocityMagnitude": 8928.610659029922
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
            "Vel": 8928.610659029922,
            "Sma": 20000000.00054466
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
        "Epoch": "2000-01-01T11:58:55.81600000000617Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8000000,
          "Y": 0,
          "Z": 0,
          "Vx": 1751.0446146025429,
          "Vy": 8755.223073012714,
          "Vz": -1.1368683772161603E-13
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 20000000.00054466,
          "Eccentricity": 0.6201736729561758,
          "Inclination": 7.439874385057628E-16,
          "RAAN": 0,
          "ArgOfPeriapsis": 330.2551187033699,
          "MeanAnomaly": 5.664753147188524,
          "TrueAnomaly": 29.74488129663015,
          "Period": 28148.546498007097
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 8000000,
          "HorizFPA": 11.309932474020208,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 8928.610659029922
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
      "Results": {}
    }
  ],
  "Positions": {
    "CentralBody": "Earth",
    "CzmlPositions": []
  }
}

         */
    }
}