using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.OrbitCore;
using ASTROX.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class PropagateTests
    {
        /*
         测试 Astrogator

            地球 近地大椭圆多次轨道机动到达小行星XF261

            验证 CzmlPositions 各段 interval 与 cartesianVelocity 首/尾历元秒严格对应
        */
        [TestMethod()]
        public void Earth2XF261_250701()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "Earth2XF261_250701.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            Assert.IsNotNull(output.Positions);
            var czml = output.Positions.CzmlPositions;
            Assert.AreEqual(5, czml.Length);

            const string sharedEpoch = "2027-12-31T02:18:42.439Z";
            string[] expectedIntervals =
            [
                "2027-12-31T02:18:42.439Z/2028-01-11T17:00:00.000Z",
                "2028-01-11T17:00:00.000Z/2028-01-22T14:00:00.000Z",
                "2028-01-22T14:00:00.000Z/2028-02-16T06:45:00.000Z",
                "2028-02-16T06:45:00.000Z/2028-07-24T11:33:00.000Z",
                "2028-07-24T11:33:00.000Z/2029-04-09T15:00:00.000Z",
            ];

            for (int i = 0; i < czml.Length; i++)
            {
                var segment = czml[i];
                Assert.AreEqual(sharedEpoch, segment.epoch);
                Assert.AreEqual("HERMITE", segment.interpolationAlgorithm);
                Assert.AreEqual(expectedIntervals[i], segment.interval);

                var rv = segment.cartesianVelocity!;
                int count = rv.Length / 7;
                var epochJd = JulianDateHelper.Parse(segment.epoch);
                string[] intervalParts = segment.interval!.Split('/');

                Assert.AreEqual(
                    OrbitBase.JulianDate2UTCG2(epochJd.AddSeconds(rv[0])),
                    intervalParts[0],
                    $"段{i + 1} interval 起始时刻与 cartesianVelocity 首点不一致");
                Assert.AreEqual(
                    OrbitBase.JulianDate2UTCG2(epochJd.AddSeconds(rv[(count - 1) * 7])),
                    intervalParts[1],
                    $"段{i + 1} interval 结束时刻与 cartesianVelocity 末点不一致");

                if (i > 0)
                {
                    string prevEnd = czml[i - 1].interval!.Split('/')[1];
                    Assert.AreEqual(prevEnd, intervalParts[0], $"段{i}与段{i + 1} interval 衔接不一致");
                }
            }
        }
    }
}
