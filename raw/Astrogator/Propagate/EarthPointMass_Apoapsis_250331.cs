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

            StoppingCondition: Apoapsis
        
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
          - 停止条件：Apoapsis

          Resuts:            
            - Duration
            - TrueAnomaly
               
        结果:
            由于是二体轨道积分器，所以最终的轨道参数与初始段一致!
            由于约束条件为1e-6°，最终的TA的误差为1e-6°

        */
        [TestMethod()]
        public void EarthPointMass_Apoapsis_250331()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_Apoapsis_250331.json");

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

            */
            //  最后一段
            var seg3 = output.MainSequenceResults[1];
            Assert.AreEqual("2000-01-01T13:01:47", seg3.FinalState.Epoch.Split('.')[0]);
            Assert.AreEqual(9686497.505536962, seg3.FinalState.Keplerian.SemiMajorAxis, 1e-6);
            Assert.AreEqual(0.2600177958875108, seg3.FinalState.Keplerian.Eccentricity, 1e-12);
            Assert.AreEqual(299.730955671183, seg3.FinalState.Keplerian.ArgOfPeriapsis, 1e-10);
            Assert.AreEqual(180, seg3.FinalState.Keplerian.TrueAnomaly, 1e-6);
        }

        /*
{
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "$type": "SegmentResult",
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
        "Keplerian": null,
        "Spherical": null,
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
      "$type": "SegmentResult",
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
        "Epoch": "2000-01-01T13:01:47.211469629750354Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -6052878.923934307,
          "Y": 10598517.289120777,
          "Z": 0,
          "Vx": -4268.835933402726,
          "Vy": -2437.9586541969284,
          "Vz": 0
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 9686497.505537456,
          "Eccentricity": 0.2600177958876149,
          "Inclination": 0,
          "RAAN": 0,
          "ArgOfPeriapsis": 299.73095567120623,
          "MeanAnomaly": 179.99999953369078,
          "TrueAnomaly": 179.99999971639127,
          "Period": 9487.704220855718
        },
        "Spherical": {
          "RightAscension": 119.7309553875975,
          "Declination": 0,
          "RadiusMagnitude": 12205159.23679819,
          "HorizFPA": 9.965553105673537E-08,
          "VelocityAzimuth": 90,
          "VelocityMagnitude": 4915.953887688942
        },
        "DryMass": 100,
        "FuelMass": 900,
        "Cd": 2.2,
        "Cr": 2,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 3771.395469629744,
      "Results": {
        "Vel": 4915.953887688942,
        "Duration1": 3771.395469629744,
        "TrueAnomaly": 179.99999971639127
      }
    }
         */
    }
}