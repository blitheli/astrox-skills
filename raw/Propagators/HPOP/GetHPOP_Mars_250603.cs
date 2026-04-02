using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                degree：   4
                order      4
                引力文件：  MRO110C.grv
                EOP文件：  （无）,坐标系转换已在MarsCentralBody中定义
                固体潮：   无    grv文件里无(STK中有和没有，结果一样)
            
            第三体引力：  （开启）
                第三体列表：                           
                           太阳
                星表：     JplDE430

            光压摄动：    （开启）
                模型：     Spherical
                反射系数： 1.0
                面质比：   0.02
                阴影模型： Dual Cone
                太阳位置:  Apparent (与STK中的Apparent To True Cb一致)
                掩食列表： Mars
        
        
            与STK结果对比精度:  位置精度～3e-3 m，速度精度～5e-6 m/s
            

            20250603    liyunfei            
         */
        [TestMethod()]
        public void GetHPOP_Mars_250603()
        {
            //  输入json
            string inputStr = """
                {
              "Description": "Mars仅非球形引力摄动",
              "Start": "2025-06-03T00:00:00.000Z",
              "Stop": "2025-06-04T00:00:00.000Z",
              "OrbitEpoch": "2025-06-03T00:00:00.000Z",
              "CoordType": "Classical",
              "CoordSystem": "Inertial",
              "OrbitalElements": [3696190, 0, 45.0, 0, 0, 0 ],      

              "GravitationalParameter": 0.4282837564100000E14,
              "Mass": 1000,
              "CoefficientOfDrag": 2.2,
              "AreaMassRatio_Drag": 0.02,
              "CoefficientOfSRP": 1.0,
              "AreaMassRatio_SRP": 0.02,

              "HpopPropagator": {
                "Name": "Mars_Hpop_v12.8",
                "CentralBodyName": "Mars",

                "GravityModel": {
                  "$type": "GravityField",
                  "GravityFileName": "MRO110C.grv",
                  "Degree": 4,
                  "Order": 4,
                  "SolidTideType": "None"
                },
                "ThirdBodyForce": [             
                  {
                    "ThirdBodyName": "Sun"
                  }
                ],            
                "SRPModel": {
                  "$type": "SRPSpherical",
                  "Description": "光压模型:SRPSpherical类型",
                  "ShadowModel": "DualCone",
                  "SunPosition": "Apparent",
                  "EclipsingBodies": [ "Mars"]
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
            Console.WriteLine("最后一点时刻、位置和速度：");
            Console.WriteLine(output.Position.cartesianVelocity[id - 7]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 6]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 5]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 4]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 3]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 2]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 1]);
            //-1167.28580893 -2406.04419483 -2539.08879358     3.2136784253 -0.9988360392 - 0.5398337094
            double ebslR = 0.003;
            double ebslV = 5e-6;
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(-1167285.80893, output.Position.cartesianVelocity[id - 6], ebslR);
            Assert.AreEqual(-2406044.19483, output.Position.cartesianVelocity[id - 5], ebslR);
            Assert.AreEqual(-2539088.79358, output.Position.cartesianVelocity[id - 4], ebslR);
            Assert.AreEqual(3213.6784253, output.Position.cartesianVelocity[id - 3], ebslV);
            Assert.AreEqual(-998.8360392, output.Position.cartesianVelocity[id - 2], ebslV);
            Assert.AreEqual(-539.8337094, output.Position.cartesianVelocity[id - 1], ebslV);

            /*
                持续时间: 670 毫秒

              标准输出: 
            最后一点时刻、位置和速度：
            86400
            -1167285.806041753
            -2406044.1939506433
            -2539088.7906095767
            3213.678429138392
            -998.8360387776099
            -539.8337082891702
             */
        }

    }
}