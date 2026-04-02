using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Propagators.Tests
{

    public partial class ProgagatorHpopTests
    {
        /*
            测试 HPOP积分器 GetHPOP
                mars hpop

            力学模型：
            中心天体：     火星
            非球形引力位：
                degree：   70
                order      70
                引力文件：  MRO110C.grv
                EOP文件：  （无）,坐标系转换已在MarsCentralBody中定义
                固体潮：   无                        
                       
            与STK结果对比精度: 在仅考虑非球形引力70阶的条件下，位置精度～1e-4 m，速度精度～5e-8 m/s
            

            20220808    liyunfei
            20230308    jinyang
            20230417    liyunfei,更新的Propagator
            20231212 Propagator中的引力模型更改为统一的接口，使用属性GravityModel，json输入文件中使用$type字段
         */
        [TestMethod()]
        public void GetHPOP_Mars_OnlyGrv()
        {
            //  输入json
            string inputStr = """
                {
              "Description": "Mars仅非球形引力摄动",
              "Start": "2023-03-07T04:00:00.000Z",
              "Stop": "2023-03-08T04:00:00.000Z",
              "OrbitEpoch": "2023-03-07T04:00:00.000Z",
              "CoordEpoch": "2000-01-01T11:58:55.816Z",
              "CoordSystem": "Inertial",
              "CoordType": "Cartesian",
              "OrbitalElements": [ 3596190.000, 0.0, 0.0, 0.0, 2990.567199157414, 1725.628089264959 ],
              "GravitationalParameter": 0.4282837564100000E14,
              "Mass": 1000,
              "CoefficientOfDrag": 2.2,
              "AreaMassRatio_Drag": 0.02,
              "CoefficientOfSRP": 1.0,
              "AreaMassRatio_SRP": 0.02,

              "HpopPropagator": {
                "Name": "Mars_Hpop_default_v10",
                "CentralBodyName": "Mars",

                "GravityModel": {
                  "$type": "GravityField",
                  "GravityFileName": "MRO110C.grv",
                  "Degree": 70,
                  "Order": 70,
                  "SolidTideType": "None"
                }
              }
            }
            """;           
            var input = JsonSerializer.Deserialize<HpopInput>(inputStr);

            //  webApi
            var output = PropagatorHPOP.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            int id = output.Position.cartesianVelocity.Length;
            Console.WriteLine(output.Position.cartesianVelocity[id - 7]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 6]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 5]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 4]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 3]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 2]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 1]);

            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(451320.1326237355602, output.Position.cartesianVelocity[id - 6], 0.0001);
            Assert.AreEqual(3081724.9139948544325, output.Position.cartesianVelocity[id - 5], 0.0001);
            Assert.AreEqual(1791429.4695418820993, output.Position.cartesianVelocity[id - 4], 0.0001);
            Assert.AreEqual(-3412.0495600215928, output.Position.cartesianVelocity[id - 3], 5e-8);
            Assert.AreEqual(532.6444385983724, output.Position.cartesianVelocity[id - 2], 5e-8);
            Assert.AreEqual(-70.5408917930615, output.Position.cartesianVelocity[id - 1], 5e-8);
        }

        //  轨道历元和场景的开始时间不一致，内部会分成两段积分，导致最终精度有点差别，位置：0.01，速度: 1e-6
        [TestMethod()]
        public void GetHPOP_Mars_OnlyGrv2_240924()
        {

            string inputStr = """
                {
                   "Description": "Mars仅非球形引力摄动",
                  "Start": "2023-03-07T16:00:00.000Z",
                  "Stop": "2023-03-08T04:00:00.000Z",
                  "OrbitEpoch": "2023-03-07T04:00:00.000Z",
                  "CoordEpoch": "2000-01-01T11:58:55.816Z",
                  "CoordSystem": "Inertial",
                  "CoordType": "Cartesian",
                  "OrbitalElements": [3596190.000, 0.0, 0.0, 0.0, 2990.567199157414, 1725.628089264959],
                  "GravitationalParameter": 0.4282837564100000E14,
                  "Mass": 1000,
                  "CoefficientOfDrag": 2.2,
                  "AreaMassRatio_Drag": 0.02,
                  "CoefficientOfSRP": 1.0,
                  "AreaMassRatio_SRP": 0.02,

                  "HpopPropagator": {
                    "Name": "Mars_Hpop_default_v10",
                    "CentralBodyName": "Mars",
                    "GravityModel": {
                      "$type": "GravityField",
                      "GravityFileName": "MRO110C.grv",
                      "Degree": 70,
                      "Order": 70,
                      "SolidTideType": "None"
                    }
                  }
                }
            """;
            var input = JsonSerializer.Deserialize<HpopInput>(inputStr);

            //  webApi
            var output = PropagatorHPOP.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            int id = output.Position.cartesianVelocity.Length;

            Assert.AreEqual(43200, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(451320.1326237355602, output.Position.cartesianVelocity[id - 6], 0.01);
            Assert.AreEqual(3081724.9139948544325, output.Position.cartesianVelocity[id - 5], 0.01);
            Assert.AreEqual(1791429.4695418820993, output.Position.cartesianVelocity[id - 4], 0.01);
            Assert.AreEqual(-3412.0495600215928, output.Position.cartesianVelocity[id - 3], 5e-6);
            Assert.AreEqual(532.6444385983724, output.Position.cartesianVelocity[id - 2], 5e-6);
            Assert.AreEqual(-70.5408917930615, output.Position.cartesianVelocity[id - 1], 5e-6);
        }
    }
}