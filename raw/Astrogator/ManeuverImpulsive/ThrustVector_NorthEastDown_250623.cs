
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class ManeuverImpulsiveTests
    {
        /*
         测试 Astrogator， 脉冲机动，推力方向为： NorthEastDown
                GTO->GEO 变轨第1次机动

            @Initial State参数：
                段名称: Initial_State
                坐标系: 地球惯性系
               
            @Propagate参数
                段名称: Propagate1
                积分器名称: Earth_Hpop_default_v10
                停止条件: Epoch
       
            @ImpulsiveManeuver参数：
                段名称: Maneuver
                机动类型: Impulsive
                姿态参数: 
                    参考轴:    NorthEastDown
                    姿态控制类型: Thrust Vector
                    Spherical类型
           
            @Propagate参数
                段名称: Propagate2
                积分器名称: Earth_Hpop_default_v10
                停止条件: Epoch
       
            与STK对比：     位置 < 10m    误差较大，待排查！

            20250623    初次编写
        */
        [TestMethod()]
        public void ThrustVector_NorthEastDown_250623()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ThrustVector_NorthEastDown_250623.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);
            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            /*  STK 结果
UTC Gregorian Date: 3 Mar 2029 00:36:40.500  UTC Julian Date: 2462198.52546875                 
Julian Ephemeris Date: 2462198.52626949                                                        
Time past epoch: 144400 sec   (Epoch in UTC Gregorian Date: 1 Mar 2029 08:30:00.000)           
                                                                                               
State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
Parameter Set Type:  Cartesian                                                                 
         X:     5350.9549339211152983 km              Vx:        1.9606986570846889 km/sec     
         Y:   -41855.0086998090191628 km              Vy:        0.2369868521483738 km/sec     
         Z:        5.0666462196166391 km              Vz:        0.4137703682243241 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:    26893.8528951190091902 km            RAAN:         277.2526229510406 deg        
       ecc:        0.5689978405670399                  w:         180.3264609137392 deg        
       inc:         11.83296645686554 deg             TA:         179.7070893572593 deg   
            */
            //  最后一段
            var seg3 = output.MainSequenceResults[3];
            Assert.AreEqual(5350954.9339211152983, seg3.FinalState.Cartesian.X, 12);
            Assert.AreEqual(-41855008.6998090191628, seg3.FinalState.Cartesian.Y, 5);
            Assert.AreEqual(5066.6462196166391, seg3.FinalState.Cartesian.Z, 5);

            Assert.AreEqual(4158.54, seg3.FinalState.FuelMass, 0.01);
        }
    }
}