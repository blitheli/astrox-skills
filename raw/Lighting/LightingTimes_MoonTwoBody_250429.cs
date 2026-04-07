using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Lighting.Tests
{

    public partial class LightingTimesTests
    {

        /*  测试   LightingTimes    月球卫星 光照计算    
            
            卫星输入：
                "OrbitEpoch": "5 Sep 2022 04:00:00.000000",
                "CoordSystem": "Inertial",
                "CoordType": "Classical",
                "OrbitalElements": [ 2037400, 0, 45, 0, 90, 0 ]

            测试结果:   与STK的结果几乎一致! <0.1s
          
            20230316    初次创建            
            20250429    由LightingTimesSc改为LightingTimes
            20250518    内部更改为仅考虑光延迟，不考虑光行差
         */

        /*
            STK 计算结果:                                                                          
        Sunlight Times
        --------------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          5 Sep 2022 04:00:00.000    5 Sep 2022 05:17:57.034          4677.034
                          5 Sep 2022 05:52:49.711    5 Sep 2022 07:35:32.534          6162.822

        Penumbra Times
        --------------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          5 Sep 2022 05:17:57.034    5 Sep 2022 05:18:16.665            19.631
                          5 Sep 2022 05:52:30.079    5 Sep 2022 05:52:49.711            19.632
                          5 Sep 2022 07:35:32.534    5 Sep 2022 07:35:52.177            19.643
        Umbra Times
        -----------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          5 Sep 2022 05:18:16.665    5 Sep 2022 05:52:30.079          2053.414
                          5 Sep 2022 07:35:52.177    5 Sep 2022 08:00:00.000          1447.823
         */

        [TestMethod()]
        public void LightingTimes_MoonTwoBody_250429()
        {
            //  读取json文件，并序列化为类对象
            string inputStr = """
                {
                  "Description": "月球卫星的光照计算,默认月球遮挡",
                  "Start": "2022-09-05T04:00:00Z",
                  "Stop": "2022-09-05T08:00:00Z",

                  "Position": {
                    "$type": "TwoBody",
                    "CentralBody": "Moon",
                    "GravitationalParameter": 4.90280030555540e+012,
                    "OrbitEpoch": "5 Sep 2022 04:00:00.000000",
                    "CoordSystem": "Inertial",
                    "CoordType": "Classical",
                    "OrbitalElements": [ 2037400, 0, 45, 0, 90, 0 ]
                  }
                }
                """;
            var input = JsonSerializer.Deserialize<LightingTimesInput>(inputStr);

            //  Lighting计算            
            var output = LightCompute.LightingTimes(input);

            if (!output.IsSuccess)
            {
                Console.WriteLine(output.Message);
                Assert.Fail();
            }

            Console.WriteLine(output.ToString());
            //  与STK结果对比
            double ebsl = 0.1;
            Assert.AreEqual(4677.034, output.SunLight.Intervals[0].Duration, ebsl);
            Assert.AreEqual(6162.822, output.SunLight.Intervals[1].Duration, ebsl);
            Assert.AreEqual(19.631, output.Penumbra.Intervals[0].Duration, ebsl);
            Assert.AreEqual(19.632, output.Penumbra.Intervals[1].Duration, ebsl);
            Assert.AreEqual(19.643, output.Penumbra.Intervals[2].Duration, ebsl);
            Assert.AreEqual(2053.414, output.Umbra.Intervals[0].Duration, ebsl);
            Assert.AreEqual(1447.823, output.Umbra.Intervals[1].Duration, ebsl);

        }

        /*
          标准输出: 
        2025/5/18 23:33:02

        sunLight intervals: 
        =================================
        2022-09-05T04:00:00.000Z  2022-09-05T05:17:57.038Z        4677.0
        2022-09-05T05:52:49.708Z  2022-09-05T07:35:32.534Z        6162.8

        Penumbra intervals: 
        =================================
        2022-09-05T05:17:57.038Z  2022-09-05T05:18:16.668Z          19.6
        2022-09-05T05:52:30.075Z  2022-09-05T05:52:49.708Z          19.6
        2022-09-05T07:35:32.534Z  2022-09-05T07:35:52.176Z          19.6

        Umbra intervals: 
        =================================
        2022-09-05T05:18:16.668Z  2022-09-05T05:52:30.075Z        2053.4
        2022-09-05T07:35:52.176Z  2022-09-05T08:00:00.000Z        1447.8
         */


    }
}