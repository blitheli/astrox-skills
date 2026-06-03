
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class ManeuverImpulsiveTests
    {
        /*
         测试 Astrogator，Target, 脉冲机动，推力方向为：Thrust Vector: VNC, Spherical类型

            @Initial State参数：
                段名称: Initial_State
                坐标系: 地球惯性系

            @Propagate参数
                段名称: Propagate2
                积分器名称: Earth_Point_Mass
                停止条件: Duration    3000

            @Target                 
                a. **DV1（第一次脉冲机动）**
                - 在VNC坐标系下施加脉冲，Spherical
                    Azimuth:    90
                    Elevation:  0
                    Magnitude:  1000
                - 机动目的：法向机动，改变轨道倾角
            
                - Result
                    Inc:    轨道倾角（Inclination）计算

        ## 目标求解配置
            控制变量：
              - DV1段，脉冲大小(ImpulsiveMnvr.Spherical.Magnitude)
              
            约束条件：
              - DV1，轨道倾角Inc = 30 deg

        结果:
                - 微分修正从初值开始；4收敛
                速度优化后，偏差 <0.001 m/s
       
            20250623    初次编写
        */
        [TestMethod()]
        public void ImpulsiveMnvr_ThrustVector_Spherical_250623()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ImpulsiveMnvr_ThrustVector_Spherical_250623.json");

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
UTC Gregorian Date: 20 Jun 2000 04:50:00.000  UTC Julian Date: 2451715.70138889                
Julian Ephemeris Date: 2451715.70213176                                                        
Time past epoch: -6.94221e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)    
                                                                                               
State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
Parameter Set Type:  Cartesian                                                                 
         X:    -2034.3605445693335696 km              Vx:       -4.8415214805706475 km/sec     
         Y:    12534.3157073022393888 km              Vy:       -0.3328036935663324 km/sec     
         Z:     3342.4841886139311100 km              Vz:        2.6007997591170748 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:    13113.2850972004089272 km            RAAN:          72.0951018321726 deg        
       ecc:        0.1987807051808486                  w:         288.7597743167232 deg        
       inc:         29.99997961181651 deg             TA:         101.8443471852892 deg  
            */
            //  轨道机动DV1段末状态（叠加了脉冲机动）
            var seg3 = output.MainSequenceResults[2] as MCSTargetSequenceResults;
            var segDv = seg3.SegmentResults[0];
            double ebslR = 2e-6;    //0.001m，m/s
            Assert.AreEqual(-2034.3605445693335696, segDv.FinalState.Cartesian.X * 0.001, ebslR);
            Assert.AreEqual(12534.3157073022393888, segDv.FinalState.Cartesian.Y * 0.001, ebslR);
            Assert.AreEqual(3342.4841886139311100, segDv.FinalState.Cartesian.Z * 0.001, ebslR);
            Assert.AreEqual(-4.8415214805706475, segDv.FinalState.Cartesian.Vx * 0.001, ebslR);
            Assert.AreEqual(-0.3328036935663324, segDv.FinalState.Cartesian.Vy * 0.001, ebslR);
            Assert.AreEqual(2.6007997591170748, segDv.FinalState.Cartesian.Vz * 0.001, ebslR);


        }
    }
}