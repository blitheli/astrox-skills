using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    [TestClass()]
    public class PropagatorTwoBodyTests
    {
        /*
            测试 积分器 GetTwoBody
            
                Mars惯性系下二体轨道模型，积分24小时            

            输入文件: TwoBody_Mars_230408.json
                里面的数值取自STK 12.2计算
                       
            结果与STK结果一致(位置精度差别为1e-6 km,速度精度差别为1e-9 km/s)

            20230408    初次编写
         */
        [TestMethod()]
        public void TwoBody_Mars_230408()
        {
            string inputStr = """
                {
                  "Start": "7 Apr 2023 04:00:00.000000",
                  "Stop": "8 Apr 2023 04:00:00.000000",
                  "CentralBody": "Mars",
                  "GravitationalParameter": 4.282837564100000e+013,
                  "OrbitEpoch": "7 Apr 2023 04:00:00.00000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 3696190, 0, 90, 0, 0, 0 ],
                  "Step": 60
                }
                """;
            var input = JsonSerializer.Deserialize<TwoBodyInput>(inputStr);

            //  webApi
            var output = PropagatorTwoBody.Compute(input);

            //  标准值为STK 的计算结果,  24小时之后的数值            
            int id = output.Position.cartesianVelocity.Length - 7;
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id], 0.1);
            Assert.AreEqual(-1903.0094369272860604, output.Position.cartesianVelocity[id + 1] * 0.001, 1e-6);
            Assert.AreEqual(0, output.Position.cartesianVelocity[id + 2] * 0.001, 1e-6);
            Assert.AreEqual(-3168.6551720052116252, output.Position.cartesianVelocity[id + 3] * 0.001, 1e-6);
            Assert.AreEqual(2.9181616772212955, output.Position.cartesianVelocity[id + 4] * 0.001, 1e-9);
            Assert.AreEqual(0, output.Position.cartesianVelocity[id + 5] * 0.001, 1e-6);
            Assert.AreEqual(-1.7525697523965471, output.Position.cartesianVelocity[id + 6] * 0.001, 1e-9);

        }


        /*
            测试 积分器 GetTwoBody
            

            输入文件: GetTwoBodyInput_20220519.json
                里面的数值取自STK计算
                       
            结果与STK结果一致(位置精度差别为~0.2m,速度精度差别为2e-3 m/s)

            20220519    初次编写
            20230823    输入文件改为json格式
         */
        [TestMethod()]
        public void GetTwoBodyTest_20220519()
        {
            //  读取输入参数(json)
            string inputStr = """
            {
               "Start": "25 Apr 2022 04:00:00.000000",
              "Stop": "25 Apr 2022 08:00:00.000000",
              "CentralBody": "Moon",
              "GravitationalParameter": 4.90280030555540e+012,
              "OrbitEpoch": "25 Apr 2022 04:00:00.000000",
              "CoordSystem": "Inertial",
              "CoordType": "Classical",
              "OrbitalElements": [ 2037400, 0, 45, 0, 90, 0 ],
              "Step": 60
            }
            """;

            var input = JsonSerializer.Deserialize<TwoBodyInput>(inputStr);

            //  webApi
            var output = PropagatorTwoBody.Compute(input);

            //  标准值为STK 的计算结果,  4小时之后的数值            
            int id = output.Position.cartesianVelocity.Length - 7;
            Assert.AreEqual(14400, output.Position.cartesianVelocity[id], 0.1);
            Assert.AreEqual(1439942.573, output.Position.cartesianVelocity[id + 1], 0.2);
            Assert.AreEqual(-64261.457, output.Position.cartesianVelocity[id + 2], 0.2);
            Assert.AreEqual(-1439942.573, output.Position.cartesianVelocity[id + 3], 0.2);
            Assert.AreEqual(34.597, output.Position.cartesianVelocity[id + 4], 2e-3);
            Assert.AreEqual(1550.486, output.Position.cartesianVelocity[id + 5], 2e-3);
            Assert.AreEqual(-34.597, output.Position.cartesianVelocity[id + 6], 2e-3);

        }
    }
}