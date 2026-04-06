using System.Text.Json;
namespace AeroSpace.Lighting.Tests
{

    public partial class LightingTimesTests
    {
        /*  测试   LightingTimesSite 函数，地面站无地形
        
            地面站:   sitePosition    [ 83.492794 , -89.484167, 0 ]

           与STK对比， 光照时间最大相差 6s，  
                由于太阳在地平线附近，因此变化缓慢，造成计算结果的差异。正常！


            20220911    初次创建     
            20230227    LightComputeV1修改后，重新输出，最小步长改为1s
            20230309    重命名为LightingTimesSite
            20250529    重新计算，考虑光延迟
        */

        /*        
            STK的计算结果（考虑地球的遮挡）

            Sunlight Times
    --------------
                            Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                        ------------------------    ------------------------    --------------
                        1 Jan 2024 00:00:00.000    25 Mar 2024 05:07:30.873       7276050.873
                        25 Mar 2024 09:12:31.105    26 Mar 2024 00:36:46.456         55455.351
                        4 Oct 2024 12:37:57.828    22 Oct 2024 04:36:15.397       1526297.569
                        26 Oct 2024 15:06:31.282     1 Jan 2025 00:00:00.000       5734408.718

    Penumbra Times
    --------------
                            Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                        ------------------------    ------------------------    --------------
                        25 Mar 2024 05:07:30.873    25 Mar 2024 09:12:31.105         14700.232
                        26 Mar 2024 00:36:46.456    30 Mar 2024 10:06:00.245        379753.789
                        7 Apr 2024 18:27:02.180    22 Apr 2024 08:55:24.096       1261701.916
                        7 Sep 2024 07:40:08.820    18 Sep 2024 04:41:49.091        939700.272
                        18 Sep 2024 02:32:00.931    18 Sep 2024 03:05:37.191          2016.260
                        30 Sep 2024 06:24:41.875     4 Oct 2024 12:37:57.828        367995.953
                        22 Oct 2024 04:36:15.397    26 Oct 2024 15:06:31.282        383415.885

    Umbra Times
    -----------
                            Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                        ------------------------    ------------------------    --------------
                        30 Mar 2024 10:06:00.245     7 Apr 2024 18:27:02.180        721261.935
                        22 Apr 2024 08:55:24.096     7 Sep 2024 07:40:08.820      11918684.724
                        18 Sep 2024 04:41:49.091    30 Sep 2024 06:24:41.875       1042972.784
         */

        [TestMethod()]
        public void LightingTimes_MoonPoleSite_20530()
        {
            //  读取json文件，并序列化为类对象
            string inputStr = """
                {
                  "Description": "南极坑顶部一点的光照,不考虑地形",
                  "Start": "2024-01-01T00:00:00Z",
                  "Stop": "2025-01-01T00:00:00Z",

                  "Position": {
                    "$type": "SitePosition",
                    "CentralBody": "Moon",
                    "cartographicDegrees": [ 83.492794, -89.484167, 0 ]
                  }
                }
                """;
            var input = JsonSerializer.Deserialize<LightingTimesInput>(inputStr);

            //  Access计算            
            var output = LightCompute.LightingTimes(input);

            Console.WriteLine(output.ToString());

            if (!output.IsSuccess)
            {
                Console.WriteLine(output.Message);
                Assert.Fail();
            }

            //  与STK 对比: 
            Assert.AreEqual(7276050.873, output.SunLight.Intervals[0].Duration, 1);
            Assert.AreEqual(55455.351, output.SunLight.Intervals[1].Duration, 6.0);
            Assert.AreEqual(1526297.569, output.SunLight.Intervals[2].Duration, 6.0);
            Assert.AreEqual(5734408.718, output.SunLight.Intervals[3].Duration, 6.0);


        }
        /*
   持续时间: 4.9 秒

          标准输出: 
        2025/5/30 9:31:46

        sunLight intervals: 
        =================================
        2024-01-01T00:00:00.000Z  2024-03-25T05:07:30.877Z     7276050.9
        2024-03-25T09:12:31.065Z  2024-03-26T00:36:47.594Z       55456.5
        2024-10-04T12:37:56.407Z  2024-10-22T04:36:19.170Z     1526302.8
        2024-10-26T15:06:26.689Z  2025-01-01T00:00:00.000Z     5734413.3

        Penumbra intervals: 
        =================================
        2024-03-25T05:07:30.877Z  2024-03-25T09:12:31.065Z       14700.2
        2024-03-26T00:36:47.594Z  2024-03-30T10:05:59.153Z      379751.6
        2024-04-07T18:27:03.814Z  2024-04-22T08:55:23.662Z     1261699.8
        2024-09-07T07:40:09.573Z  2024-09-18T04:41:47.694Z      939698.1
        2024-09-30T06:24:42.360Z  2024-10-04T12:37:56.407Z      367994.0
        2024-10-22T04:36:19.170Z  2024-10-26T15:06:26.689Z      383407.5

        Umbra intervals: 
        =================================
        2024-03-30T10:05:59.153Z  2024-04-07T18:27:03.814Z      721264.7
        2024-04-22T08:55:23.662Z  2024-09-07T07:40:09.573Z    11918685.9
        2024-09-18T04:41:47.694Z  2024-09-30T06:24:42.360Z     1042974.7
        */


    }
}