
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class ManeuverImpulsiveTests
    {
        /*
         测试 Astrogator
                MCS:
                >   Initial_State       地球惯性系Cartisian
                >   Propagate1          Two_Body_Earth积分器
                >   ImpulsiveManeuver   脉冲机动
                >   Propagate2          Two_Body_Earth积分器

            @Initial State参数：
                段名称: Initial_State
                坐标系: 地球惯性系
                中心天体引力常数: 3.986004415E14,
                轨道历元: 2022-06-20T04:00:00.000Z
                坐标类型: Cartesian
                坐标元素: 6678137.0 (m)
                       0.0 (m)
                       0.0 (m)
                       0.0 (m/s)
                       6789.5303002727 (m/s)
                       3686.4141744009 (m/s)
                结构质量: 500 (kg)
                燃料质量: 500 (kg)
                拖拽系数: 2.2
                拖拽面积: 20 (m^2)
                SRP系数: 1.0
                SRP面积: 20 (m^2)
 
            @Propagate1参数：
                段名称: Propagate1
                积分器名称: Two_Body_Earth
                停止条件: Duration    86400 s

            @ImpulsiveManeuver参数：
                段名称: Maneuver
                机动类型: Impulsive
                姿态参数: 
                    "ThrustAxesName": "VVLH(CBF)",
                    "CoordType": "Cartesian",
                    "X": 3.0,
                    "Y": 4.0,
                    "Z": 5.0
           
            @Propagate2参数
                段名称: Propagate2
                积分器名称: Two_Body_Earth
                停止条件: Duration    43200 s

            测试结果与STK中值对比，位置精度：～2e-5 (m), 速度精度 ～2e-8 (m/s)
            20230609 初次创建                                            Mulin LIU
            20230614 修改了第一段积分段时间，使该算例与其他算例的积分时间一致。 Mulin LIU
            20230617 修改了算例及相应对比算例，令AbsError为1e-10，重新比较精度。Nulin LIU
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器            
        */
        [TestMethod()]
        public void Impulsive_ThrustVectorVVLH_CBF_230609()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ThrustVectorVVLH_CBF_230609.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(129600, output.Position.cartesianVelocity[id - 7], 1e-6);
            Assert.AreEqual(4012580.3053821241519, output.Position.cartesianVelocity[id - 6], 2e-4);
            Assert.AreEqual(-4693864.5624291630156, output.Position.cartesianVelocity[id - 5], 2e-5);
            Assert.AreEqual(-2547241.5352822449677, output.Position.cartesianVelocity[id - 4], 2e-5);
            Assert.AreEqual(6173.3164769442741, output.Position.cartesianVelocity[id - 3], 2e-8);
            Assert.AreEqual(4085.4993430703219, output.Position.cartesianVelocity[id - 2], 2e-8);
            Assert.AreEqual(2214.0556341678317, output.Position.cartesianVelocity[id - 1], 2e-8);
            Assert.IsTrue(output.IsSuccess);
        }
    }
}