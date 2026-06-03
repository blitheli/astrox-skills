
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
                >   Initial_State       月球惯性系Cartisian
                >   Propagate1          Two_Body_Moon积分器
                >   ImpulsiveManeuver   脉冲机动
                >   Propagate2          Two_Body_Moon积分器

            @Initial State参数：
                段名称: Initial_State
                坐标系: 月球惯性系
                中心天体引力常数: 4.90280030555539997294545e+012,
                轨道历元: 2022-06-20T04:00:00.000Z
                坐标类型: Cartesian
                坐标元素: 367579208.7370632798411 (m)
                         -68393849.6181882364908 (m)
                         -63404153.8937911973335 (m)
                               376.7005425488177 (m/s)
                              2104.1167780364676 (m/s)
                              1417.9183931038208 (m/s)
                结构质量: 500 (kg)
                燃料质量: 500 (kg)
                拖拽系数: 2.2
                拖拽面积: 20 (m^2)
                SRP系数: 1.0
                SRP面积: 20 (m^2)
 
            @Propagate1参数：
                段名称: Propagate1
                积分器名称: Two_Body_Moon
                停止条件: Duration    86400 s

            @ImpulsiveManeuver参数：
                段名称: Maneuver
                机动类型: Impulsive
                姿态参数: 
                    "ThrustAxesName": "LVLH",
                    "CoordType": "Cartesian",
                    "X": 3.0,
                    "Y": 4.0,
                    "Z": 5.0
           
            @Propagate2参数
                段名称: Propagate2
                积分器名称: Two_Body_Moon
                停止条件: Duration    43200 s

            与STK进行比较，位置精度～2e-4 (m), 速度精度～2e-7 (m/s)
            20230610 初次创建                                           Mulin LIU
            20230614 修改了第一段积分段时间，使该算例与其他算例的积分时间一致。 Mulin LIU
            20230616 对比机动矢量（Moon Inertial）
                     计算的值：4.2633318819484884    -3.8173629199091295    4.153521590417448
                    STK参考值：4.263331882123068     -3.817362919565822     4.153521590553842
                         差别：4e-10
                                                                        Mulin LIU
            20230617 修改了月球引力常数值与cb文件中一致。                    Mulin LIU
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器                     
        */
        [TestMethod()]
        public void Impulsive_ThrustVectorLVLH_Moon_230610()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/ManeuverImpulsive");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ThrustVectorLVLH(Moon)_230610.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 129600s 的数值            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(129600, output.Position.cartesianVelocity[id - 7], 1e-6);
            Assert.AreEqual(1584057.9025611643829, output.Position.cartesianVelocity[id - 6], 2e-4);
            Assert.AreEqual(1207997.2586341677925, output.Position.cartesianVelocity[id - 5], 2e-4);
            Assert.AreEqual(329269.2969130218330, output.Position.cartesianVelocity[id - 4], 2e-4);
            Assert.AreEqual(-960.1454999824605, output.Position.cartesianVelocity[id - 3], 2e-7);
            Assert.AreEqual(1181.9782468267106, output.Position.cartesianVelocity[id - 2], 2e-7);
            Assert.AreEqual(313.9353817639366, output.Position.cartesianVelocity[id - 1], 2e-7);
            Assert.IsTrue(output.IsSuccess);
        }
    }
}