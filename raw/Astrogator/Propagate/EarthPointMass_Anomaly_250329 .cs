using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
         测试 Astrogator 
            StoppingConditions: TrueAnomaly
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        1. **Initial_State（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg
          - 结果计算：远地点距离（RadiusOfApoapsis）

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：TrueAnomaly

          Resuts:            
            - Duration
            - TrueAnomaly
               
        结果:
            由于是二体轨道积分器，所以最终的轨道参数与初始段一致!
            由于约束条件为1e-6°，最终的TA的误差为1e-6°

        */
        [TestMethod()]
        public void EarthPointMass_Anomaly_250329()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_Anomaly_250329 .json");

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


            var seg0 = output.MainSequenceResults[0];
            //  初始段
            Assert.AreEqual(9686497.505536763, seg0.InitialState.Keplerian.SemiMajorAxis, 1e-6);
            Assert.AreEqual(0.2600177958875259, seg0.InitialState.Keplerian.Eccentricity, 1e-9);
            Assert.AreEqual(0, seg0.InitialState.Keplerian.Inclination, 1e-9);
            Assert.AreEqual(0, seg0.InitialState.Keplerian.RAAN, 1e-9);
            Assert.AreEqual(299.730955671197, seg0.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);
            Assert.AreEqual(60.26904432880302, seg0.InitialState.Keplerian.TrueAnomaly, 1e-9);

            /*
              STK 结果
                        UTC Gregorian Date: 1 Jan 2000 14:04:43.611  UTC Julian Date: 2451545.08661587                            
Julian Ephemeris Date: 2451545.08735874                                                                   
Time past epoch: -7.08962e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)               
                                                                                                          
State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                          
Parameter Set Type:  Cartesian                                                                            
         X:    -4028.7104105699409047 km              Vx:        7.2376407286169417 km/sec                
         Y:    -6902.8699377401981110 km              Vy:       -2.4919903321737502 km/sec                
         Z:        0.0000000000000000 km              Vz:        0.0000000000000000 km/sec                
                                                                                                          
Parameter Set Type:  Keplerian                                                                            
       sma:     9686.4975055382110440 km            RAAN:                         0 deg                   
       ecc:        0.2600177958876753                  w:         299.7309556712121 deg                   
       inc:                         0 deg             TA:         299.9999981867196 deg                   
                                                                                                          
Parameter Set Type:  Spherical                                                                            
 Right Asc:         239.7309538579318 deg     Horiz. FPA:         -11.2699607126349 deg                   
      Decl:                         0 deg        Azimuth:                        90 deg                   
       |R|:     7992.5040475180230715 km             |V|:        7.6546364467675545 km/sec                                                                                                 

           我的结果
              "SemiMajorAxis": 9686497.505537093,
          "Eccentricity": 0.2600177958875348,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.73095567119816,
          "MeanAnomaly": 323.29112801476936,
          "TrueAnomaly": 300.00000067282815,
          "Period": 9487.704220855187

            */
            //  最后一段
            var seg3 = output.MainSequenceResults[1];
            //Assert.AreEqual("2000-01-01T14:04:43", seg3.FinalState.Epoch.Split('.')[0]);
            //Assert.AreEqual(9686.4975055382110440, seg3.FinalState.Keplerian.SemiMajorAxis * 0.001, 1e-8);
            //Assert.AreEqual(0.2600177958876753, seg3.FinalState.Keplerian.Eccentricity, 1e-12);
            //Assert.AreEqual(299.7309556712133, seg3.FinalState.Keplerian.ArgOfPeriapsis, 1e-11);
            //Assert.AreEqual(299.9999981867196, seg3.FinalState.Keplerian.TrueAnomaly, 1e-6);
        }

        /*{
 {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
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
        "SRPArea": 20
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
        "SRPArea": 20
      },
      "DurationSec": 0,
      "Results": {
        "RadiusOfApoapsis": 12205159.23679645
      }
    },
    {
      "$type": "PropagateResult",
      "StoppedOnMaximumDuration": false,
      "StoppingConditionName": "TrueAnomaly",
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
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2000-01-01T14:04:43.61103768843168Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -4028710.076215853,
          "Y": -6902870.052861371,
          "Z": 0,
          "Vx": 7237.640873916172,
          "Vy": -2491.9900832153166,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505537093,
          "Eccentricity": 0.2600177958875348,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.73095567119816,
          "MeanAnomaly": 323.29112801476936,
          "TrueAnomaly": 300.00000067282815,
          "Period": 9487.704220855187
        },
        "Spherical": {
          "RightAscension": 239.73095634402642,
          "Declination": 0,
          "RadiusMagnitude": 7992503.9784096135,
          "HorizFPA": -11.269960342570782,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 7654.636503102255
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 7547.7950376884255,
      "Results": {
        "Vel": 7654.636503102255,
        "Duration1": 7547.7950376884255,
        "TrueAnomaly": -59.99999932717182
      }
    }
  ],

         */
    }
}