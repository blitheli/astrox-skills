using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace AeroSpace.Lighting.Tests
{

    public partial class LightingTimesTests
    {


        /*
            测试   LightingTimes 函数，无地形

            地球地面站:   St Helens火山坑里一点    [-122.18936, 46.19557, 0 ]
        
            测试结果:   与STK对比的时间误差 < 0.2s        
        
            20250529    初次创建            
         */

        /*
         STK    结果： 无地形
        
        Facility-stHelens1:  Lighting

Sunlight Times
--------------
                     Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                  -----------------------    -----------------------    --------------
                  5 Sep 2022 13:40:58.412    6 Sep 2022 02:32:56.714         46318.302
                  6 Sep 2022 13:42:13.037    7 Sep 2022 02:31:01.440         46128.403

Penumbra Times
--------------
                     Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                  -----------------------    -----------------------    --------------
                  5 Sep 2022 13:37:52.346    5 Sep 2022 13:40:58.412           186.066
                  6 Sep 2022 02:32:56.714    6 Sep 2022 02:36:02.239           185.525
                  6 Sep 2022 13:39:07.215    6 Sep 2022 13:42:13.037           185.822
                  7 Sep 2022 02:31:01.440    7 Sep 2022 02:34:06.732           185.292


Umbra Times
-----------
                     Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                  -----------------------    -----------------------    --------------
                  5 Sep 2022 04:00:00.000    5 Sep 2022 13:37:52.346         34672.346
                  6 Sep 2022 02:36:02.239    6 Sep 2022 13:39:07.215         39784.977
                  7 Sep 2022 02:34:06.732    7 Sep 2022 04:00:00.000          5153.268
         */

        [TestMethod()]
        public void LightingTimes_StHelens_250529()
        {
            //  读取json文件，并序列化为类对象
            string inputStr = """
                {
                  "Description": "St.Helens火山坑一点的光照,考虑地形遮罩数据，来源于STK地形遮罩数据",
                  "Start": "2022-09-05T04:00:00Z",
                  "Stop": "2022-09-07T04:00:00Z",
                
                  "Position": {
                    "$type": "SitePosition",
                    "CentralBody": "Earth",
                    "cartographicDegrees": [-122.18936, 46.19557, 0],
                    "clampToGround": false
                  }
                }
                """;
            var input = JsonSerializer.Deserialize<LightingTimesInput>(inputStr);

            //  Access计算            
            var output = LightCompute.LightingTimes(input);

            if (!output.IsSuccess)
            {
                Console.WriteLine(output.Message);
                Assert.Fail();
            }
            Console.WriteLine(output.ToString());

            double ebsl = 0.2;
            Assert.AreEqual(46318.302, output.SunLight.Intervals[0].Duration, ebsl);
            Assert.AreEqual(46128.403, output.SunLight.Intervals[1].Duration, ebsl);
            Assert.AreEqual(186.066, output.Penumbra.Intervals[0].Duration, ebsl);
            Assert.AreEqual(185.525, output.Penumbra.Intervals[1].Duration, ebsl);
            Assert.AreEqual(185.822, output.Penumbra.Intervals[2].Duration, ebsl);
            Assert.AreEqual(185.292, output.Penumbra.Intervals[3].Duration, ebsl);
            Assert.AreEqual(34672.346, output.Umbra.Intervals[0].Duration, ebsl);
            Assert.AreEqual(39784.977, output.Umbra.Intervals[1].Duration, ebsl);
            Assert.AreEqual(5153.268, output.Umbra.Intervals[2].Duration, ebsl);

            /*
            
     2025/5/29 10:55:06

sunLight intervals: 
=================================
2022-09-05T13:40:58.370Z  2022-09-06T02:32:56.676Z       46318.3
2022-09-06T13:42:12.994Z  2022-09-07T02:31:01.405Z       46128.4

Penumbra intervals: 
=================================
2022-09-05T13:37:52.308Z  2022-09-05T13:40:58.370Z         186.1
2022-09-06T02:32:56.676Z  2022-09-06T02:36:02.194Z         185.5
2022-09-06T13:39:07.177Z  2022-09-06T13:42:12.994Z         185.8
2022-09-07T02:31:01.405Z  2022-09-07T02:34:06.688Z         185.3

Umbra intervals: 
=================================
2022-09-05T04:00:00.000Z  2022-09-05T13:37:52.308Z       34672.3
2022-09-06T02:36:02.194Z  2022-09-06T13:39:07.177Z       39785.0
2022-09-07T02:34:06.688Z  2022-09-07T04:00:00.000Z        5153.3
          
             */

        }

    }
}