using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    [TestClass()]
    public class PropagatorSimpleAscentTests
    {
        /*
            测试 火箭主动上升段积分器 GetSimpleAscent

            输入文件: SimpleAscentInput_220612.json

            根据json文件中的参数创建STK场景，
                创建LaunchVechicle对象

            结果与STK对比(位置精度差别为<1m,速度精度差别为0.001m/s)

            20220612  初次编写
            20220622  仅输出位置
            20220627  输出速度
            20230409  修正文件名称
        */
        [TestMethod()]
        public void SimpleAscent_Earth_220612()
        {
            string inputStr = """
                {
                  "Start": "2022-05-23T04:00:00.000Z",
                  "Stop": "2022-05-23T04:10:00.000Z",
                  "Step": 5.0,
                  "LaunchLatitude": 28.6084,
                  "LaunchLongitude": -80.6042,
                  "LaunchAltitude": 0.0,
                  "BurnoutVelocity": 7725.84,
                  "BurnoutLatitude": 25.051,
                  "BurnoutLongitude": -51.326,
                  "BurnoutAltitude": 300000
                }
                """;
            var input = JsonSerializer.Deserialize<SimpleAscentInput>(inputStr);


            //  webApi
            var output = PropagatorSimpleAscent.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "SimpleAscent_Earth_220612_out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果, 600s 的数值            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(600, output.Position.cartesianVelocity[id - 7], 0.2);
            Assert.AreEqual(3.7827019905658336e+06, output.Position.cartesianVelocity[id - 6], 1);
            Assert.AreEqual(-4.7259773029793743e+06, output.Position.cartesianVelocity[id - 5], 1);
            Assert.AreEqual(2.8112209725318677e+06, output.Position.cartesianVelocity[id - 4], 1);
            Assert.AreEqual(6.3464007584058454e+03, output.Position.cartesianVelocity[id - 3], 0.001);
            Assert.AreEqual(4.0555638270797631e+03, output.Position.cartesianVelocity[id - 2], 0.001);
            Assert.AreEqual(-1.7216861397443588e+03, output.Position.cartesianVelocity[id - 1], 0.001);
        }

        /*
          测试 火箭主动上升段积分器 GetSimpleAscent

            火星

          根据json文件中的参数创建STK场景，
            创建LaunchVechicle对象

          结果与STK对比(位置精度差别为<1m,速度精度差别为0.001m/s)

          20230409  初次编写
        */
        [TestMethod()]
        public void SimpleAscent_Mars_230409()
        {
            string inputStr = """
                {
                  "Start": "2022-05-23T04:00:00.000Z",
                  "Stop": "2022-05-23T04:10:00.000Z",
                  "Step": 5.0,
                  "CentralBody": "Mars",
                  "LaunchLatitude": 28.6084,
                  "LaunchLongitude": -80.6042,
                  "LaunchAltitude": 0.0,
                  "BurnoutVelocity": 7725.84,
                  "BurnoutLatitude": 25.051,
                  "BurnoutLongitude": -51.326,
                  "BurnoutAltitude": 300000
                }
                """;
            var input = JsonSerializer.Deserialize<SimpleAscentInput>(inputStr);

            //  webApi
            var output = PropagatorSimpleAscent.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "SimpleAscent_Mars_230409_out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果, 600s 的数值            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(600, output.Position.cartesianVelocity[id - 7], 0.2);
            //Assert.AreEqual(3.7827019905658336e+06, output.Position.cartesianVelocity[id - 6], 1);
            //Assert.AreEqual(-4.7259773029793743e+06, output.Position.cartesianVelocity[id - 5], 1);
            //Assert.AreEqual(2.8112209725318677e+06, output.Position.cartesianVelocity[id - 4], 1);
            //Assert.AreEqual(6.3464007584058454e+03, output.Position.cartesianVelocity[id - 3], 0.001);
            //Assert.AreEqual(4.0555638270797631e+03, output.Position.cartesianVelocity[id - 2], 0.001);
            //Assert.AreEqual(-1.7216861397443588e+03, output.Position.cartesianVelocity[id - 1], 0.001);
        }
    }
}