
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

            @InitialState参数：
                段名称: Initial_State
                坐标系: 地球惯性系
                中心天体引力常数: 3.986004415E14,
                轨道历元: 2017-06-28T04:00:00.000Z
                坐标元素: 
                    "$type": "Cartesian",
                    "X": -4348504.5285496607903,
                    "Y": -4721813.3054333820837,
                    "Z": -1835916.3669064525948,
                    "Vx": 6043.9238667905633,
                    "Vy": -7499.6612192427978,
                    "Vz": 4972.9769780850104
                结构质量: 500 (kg)
                燃料质量: 500 (kg)
                拖拽系数: 2.2
                拖拽面积: 20 (m^2)
                SRP系数: 1.0
                SRP面积: 20 (m^2)
 
            @Propagate参数：
                段名称: Propagate1
                积分器名称: Two_Body_Earth
                停止条件: Duration    86400s

            @ManeuverImpulsive参数：
                段名称: ImpulsiveManeuve
                机动类型: Impulsive
                姿态参数: 
                     姿态控制类型: Anti Velocity Vector
                     DeltaV模: 5.0 (m/s)
           
            @Propagate参数
                段名称: Propagate2
                积分器名称: Two_Body_Earth
                停止条件: Duration    43200
       
            20220810    初次创建，测试结果与STK结果对比，位置精度~3e-5 (m), 速度精度~3e-8 (m/s)
                        Mulin LIU 
            20230526    重新创建, 测试结果与STK结果对比，位置精度~3e-5 (m), 速度精度~3e-8 (m/s)
                        Mulin LIU
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        */
        [TestMethod()]
        public void Impulsive_antiVelocityVector_230526()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "antiVelocityVector_230526.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 129600 s的数值            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(129600, output.Position.cartesianVelocity[id - 7], 1e-6);
            Assert.AreEqual(4802982.8720989435169, output.Position.cartesianVelocity[id - 6], 3e-5);
            Assert.AreEqual(-4077443.1515469073020, output.Position.cartesianVelocity[id - 5], 3e-5);
            Assert.AreEqual(-2213870.9990841184663, output.Position.cartesianVelocity[id - 4], 3e-5);
            Assert.AreEqual(5365.6852526495671, output.Position.cartesianVelocity[id - 3], 3e-8);
            Assert.AreEqual(4879.0081586286478, output.Position.cartesianVelocity[id - 2], 3e-8);
            Assert.AreEqual(2649.0852883098586, output.Position.cartesianVelocity[id - 1], 3e-8);
            Assert.IsTrue(output.IsSuccess);
        }
    }
}