
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
                >   ImpulsiveManeuver   脉冲机动 Attitude
                >   Propagate2          Two_Body_Earth积分器

            @InitialState参数：
                段名称: Initial_State
                坐标系: 地球惯性系
                中心天体引力常数: 3.986004415E14,
                轨道历元: 2017-06-28T04:00:00.000Z
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
 
            @Propagate参数：
                段名称: Propagate1
                积分器名称: Two_Body_Earth
                停止条件: Duration    86400s

            @ManeuverImpulsive参数：
                段名称: ImpulsiveManeuver
                机动类型: Impulsive
                姿态参数: Attitude
                    "DeltaVMagnitude": 5.0,
                    "RefAxesName": "VNC(Earth)",
                    "CoordType": "EulerAngles",
                    "A": 45,
                    "B": 45,
                    "C": 45,
                    "Sequence": "313"
                   
            @Propagate参数
                段名称: Propagate2
                积分器名称: Two_Body_Earth
                停止条件: Duration    43200
       
            20220810    初次创建，测试结果与STK结果对比，位置精度~5e-5 (m), 速度精度~4e-8 (m/s)
                        Mulin LIU 
            20230529    重新创建, liyunfei
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        */
        [TestMethod()]
        public void Impulsive_Attitude_EulerAngles_230529()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "Attitude_EulerAngles_230529.json");
            
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
            Assert.AreEqual(4254527.5713638893649, output.Position.cartesianVelocity[id - 6], 5e-5);
            Assert.AreEqual(-4522495.7732328002749, output.Position.cartesianVelocity[id - 5], 3e-5);
            Assert.AreEqual(-2456766.8824325864989, output.Position.cartesianVelocity[id - 4], 3e-5);
            Assert.AreEqual(5956.9511039161362,     output.Position.cartesianVelocity[id - 3], 4e-8);
            Assert.AreEqual(4323.4007455701917,     output.Position.cartesianVelocity[id - 2], 4e-8);
            Assert.AreEqual(2352.0502622770034, output.Position.cartesianVelocity[id - 1], 4e-8);
            Assert.IsTrue(output.IsSuccess);
        }
    }
}