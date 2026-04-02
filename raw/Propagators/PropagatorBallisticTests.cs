using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    [TestClass()]
    public class PropagatorBallisticTests
    {
        /*
          测试 导弹积分器 GetBallistic
                  
          输入文件: Ballistic_ApogeeAlt_Input.json
                BallisticType:  ApogeeAlt
            
          根据json文件中的参数 创建STK场景，
            创建Missile的对象，采用缺省值
                弹道类型选择: Fixed Apogee Alt

          结果与STK对比(位置精度差别为<20m, 速度<0.1m/s)

          20220627  初次编写
       */
        [TestMethod()]
        public void GetBallistic_ApogeeAlt_Test_20220627()
        {
            string inputStr = """
                {
                  "Start": "2022-06-27T04:00:00.000Z",
                  "Step": 5,
                  "LaunchLatitude": 0,
                  "LaunchLongitude": 0,
                  "LaunchAltitude": 0.0,
                  "BallisticType": "ApogeeAlt",
                  "BallisticTypeValue": 500000,
                  "ImpactLatitude": 20,
                  "ImpactLongitude": 20,
                  "ImpactAltitude": 0
                }
                """;
            var input = JsonSerializer.Deserialize<BallisticInput>(inputStr);

            //  webApi
            var output = PropagatorBallistic.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "Ballistic_ApogeeAlt_Out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(8.2062264972953562e+02, output.Position.cartesianVelocity[id - 7], 1.0);
            Assert.AreEqual(5.6342432053876352e+06, output.Position.cartesianVelocity[id - 6], 20);
            Assert.AreEqual(2.0506968193776826e+06, output.Position.cartesianVelocity[id - 5], 20);
            Assert.AreEqual(2.1676967878254363e+06, output.Position.cartesianVelocity[id - 4], 20);
            Assert.AreEqual(-4.1760039723288273e+03, output.Position.cartesianVelocity[id - 3], 0.1);
            Assert.AreEqual(1.8284920095159659e+03, output.Position.cartesianVelocity[id - 2], 0.1);
            Assert.AreEqual(1.7973193646938466e+03, output.Position.cartesianVelocity[id - 1], 0.1);
        }


        /*
          测试 导弹积分器 GetBallistic

          输入文件: Ballistic_DeltaV_MinEcc_Input.json
                BallisticType:  DeltaV_MinEcc

          根据json文件中的参数 创建STK场景，
            创建Missile的对象，采用缺省值
                弹道类型选择: Fixed DeltaV-MinEcc

          结果与STK对比(位置精度差别为<20m, 速度<0.1m/s)

          20220627  初次编写
        */
        [TestMethod()]
        public void GetBallistic_DeltaV_MinEcc_Test_20220627()
        {
            string inputStr = """
                {
                  "Start": "2022-06-27T04:00:00.000Z",
                  "Step": 5,
                  "LaunchLatitude": 0,
                  "LaunchLongitude": 0,
                  "LaunchAltitude": 0.0,
                  "BallisticType": "DeltaV_MinEcc",
                  "BallisticTypeValue": 6901.943,
                  "ImpactLatitude": 20,
                  "ImpactLongitude": 20,
                  "ImpactAltitude": 0
                }
                """;
            var input = JsonSerializer.Deserialize<BallisticInput>(inputStr);

            //  webApi
            var output = PropagatorBallistic.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "Ballistic_DeltaV_MinEcc_Out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(4.5482544154694961e+02, output.Position.cartesianVelocity[id - 7], 1.0);
            Assert.AreEqual(5.6342432053875979e+06, output.Position.cartesianVelocity[id - 6], 20);
            Assert.AreEqual(2.0506968193776794e+06, output.Position.cartesianVelocity[id - 5], 20);
            Assert.AreEqual(2.1676967878254270e+06, output.Position.cartesianVelocity[id - 4], 20);
            Assert.AreEqual(-3.5623530389950683e+03, output.Position.cartesianVelocity[id - 3], 0.1);
            Assert.AreEqual(4.1056355558730893e+03, output.Position.cartesianVelocity[id - 2], 0.1);
            Assert.AreEqual(4.2560242272544610e+03, output.Position.cartesianVelocity[id - 1], 0.1);

        }


        /*
          测试 导弹积分器 GetBallistic

          输入文件: Ballistic_DeltaV_Input.json
                BallisticType:  DeltaV

          根据json文件中的参数 创建STK场景，
            创建Missile的对象，采用缺省值
                弹道类型选择: Fixed DeltaV

          结果与STK对比(位置精度差别为<20m, 速度<0.1m/s)

          20220627  初次编写
        */
        [TestMethod()]
        public void GetBallistic_DeltaV_Test_20220627()
        {
            string inputStr = """
                {
                  "Start": "2022-06-27T04:00:00.000Z",
                  "Step": 5,
                  "LaunchLatitude": 0,
                  "LaunchLongitude": 0,
                  "LaunchAltitude": 0.0,
                  "BallisticType": "DeltaV",
                  "BallisticTypeValue": 6901.943,
                  "ImpactLatitude": 20,
                  "ImpactLongitude": 20,
                  "ImpactAltitude": 0
                }
                """;
            var input = JsonSerializer.Deserialize<BallisticInput>(inputStr);

            //  webApi
            var output = PropagatorBallistic.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "Ballistic_DeltaV_Out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(2.7285131991063545e+03, output.Position.cartesianVelocity[id - 7], 1.0);
            Assert.AreEqual(5.6342432053876352e+06, output.Position.cartesianVelocity[id - 6], 20);
            Assert.AreEqual(2.0506968193776826e+06, output.Position.cartesianVelocity[id - 5], 20);
            Assert.AreEqual(2.1676967878254363e+06, output.Position.cartesianVelocity[id - 4], 20);
            Assert.AreEqual(-6.8336545666908787e+03, output.Position.cartesianVelocity[id - 3], 0.1);
            Assert.AreEqual(-3.4960999491823492e+02, output.Position.cartesianVelocity[id - 2], 0.1);
            Assert.AreEqual(-9.1612516955393914e+02, output.Position.cartesianVelocity[id - 1], 0.1);


        }


        /*
          测试 导弹积分器 GetBallistic

          输入文件: Ballistic_TimeOfFlight_Input.json
                BallisticType:  TimeOfFlight

          根据json文件中的参数 创建STK场景，
            创建Missile的对象，采用缺省值
                弹道类型选择: Fixed TimeOfFlight

          结果与STK对比(位置精度差别为<20m, 速度<0.1m/s)

          20220627  初次编写
        */
        [TestMethod()]
        public void GetBallistic_TimeOfFlight_Test_20220627()
        {
            string inputStr = """
                {
                  "Start": "2022-06-27T04:00:00.000Z",
                  "Step": 5,
                  "LaunchLatitude": 0,
                  "LaunchLongitude": 0,
                  "LaunchAltitude": 0.0,
                  "BallisticType": "TimeOfFlight",
                  "BallisticTypeValue": 3000,
                  "ImpactLatitude": 20,
                  "ImpactLongitude": 20,
                  "ImpactAltitude": 0
                }
                """;
            var input = JsonSerializer.Deserialize<BallisticInput>(inputStr);

            //  webApi
            var output = PropagatorBallistic.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  输出到json文件
            //string outputStr = JsonSerializer.Serialize(output);
            //File.WriteAllText(Path.Combine(filePath0, "Ballistic_TimeOfFlight_Out.json"), outputStr, Encoding.UTF8);
            //Console.WriteLine("计算完成，结果写入到文件！");

            //  标准值为STK 的计算结果            
            int id = output.Position.cartesianVelocity.Length;
            Assert.AreEqual(2.9999999999999854e+03, output.Position.cartesianVelocity[id - 7], 1.0);
            Assert.AreEqual(5.6342432053876352e+06, output.Position.cartesianVelocity[id - 6], 20);
            Assert.AreEqual(2.0506968193776826e+06, output.Position.cartesianVelocity[id - 5], 20);
            Assert.AreEqual(2.1676967878254363e+06, output.Position.cartesianVelocity[id - 4], 20);
            Assert.AreEqual(-7.0359116724227406e+03, output.Position.cartesianVelocity[id - 3], 0.1);
            Assert.AreEqual(-4.1815420766116353e+02, output.Position.cartesianVelocity[id - 2], 0.1);
            Assert.AreEqual(-1.0530871014299598e+03, output.Position.cartesianVelocity[id - 1], 0.1);
        }
    }
}