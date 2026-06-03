
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
 
            @Propagate1参数：
                段名称: Propagate1
                积分器名称: Two_Body_Earth
                停止条件: Duration    86400s

            @ImpulsiveManeuver参数：
                段名称: Maneuver
                机动类型: Impulsive
                姿态参数: 
                     姿态控制类型: Along Velocity Vector
                     DeltaV模: 5.0 (m/s)
           
            @Propagate2参数
                段名称: Propagate2
                积分器名称: Two_Body_Earth
                停止条件: Duration    43200
       
            20220810    初次创建，测试结果与STK结果对比，位置精度~3e-5 (m), 速度精度~3e-8 (m/s)
                        Mulin LIU 
            20230523    重新创建,liyunfei
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        */
        [TestMethod()]
        public void Impulsive_alongVelocityVector_230523()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "alongVelocityVector_230523.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);
           

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值  
            var segLast = output.MainSequenceResults[3];          
            Assert.AreEqual(3810466.9441048199587, segLast.FinalState.Cartesian.X, 3e-5);
            Assert.AreEqual(-4820403.3578018088519, segLast.FinalState.Cartesian.Y, 3e-5);
            Assert.AreEqual(-2617265.4776747640426, segLast.FinalState.Cartesian.Z, 3e-5);
            Assert.AreEqual(6346.3169284862619, segLast.FinalState.Cartesian.Vx, 3e-8);
            Assert.AreEqual(3878.5142611935668, segLast.FinalState.Cartesian.Vy, 3e-8);
            Assert.AreEqual(2105.8614242440052, segLast.FinalState.Cartesian.Vz, 3e-8);

        }
    }
}