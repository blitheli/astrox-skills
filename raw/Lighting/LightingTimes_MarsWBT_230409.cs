using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Lighting.Tests
{

    public partial class LightingTimesTests
    {
        /*  测试   LightingTimesSite 函数，无遮罩
                      
            火星乌托邦平原

            测试结果:   与STK对比的时间误差 <0.2s
          
            20221002    初次创建      
            20220224    LightComputeV1修改后，重新输出，最小步长改为1s
            20230309    重命名为LightingTimesSite
            20250529    由LightingTimesSc改为LightingTimes
         */

        /*
            STK计算结果:

                Sunlight Times
        --------------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          7 Apr 2023 04:00:00.000    7 Apr 2023 17:15:22.187         47722.187
                          8 Apr 2023 02:27:30.003    8 Apr 2023 04:00:00.000          5549.997

        Penumbra Times
        --------------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          7 Apr 2023 17:15:22.187    7 Apr 2023 17:17:43.086           140.898
                          8 Apr 2023 02:25:09.152    8 Apr 2023 02:27:30.003           140.851
       
        Umbra Times
        -----------
                             Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                          -----------------------    -----------------------    --------------
                          7 Apr 2023 17:17:43.086    8 Apr 2023 02:25:09.152         32846.066
           
         */

        [TestMethod()]
        public void LightingTimes_MarsWBT_230409()
        {           
            //  读取json文件，并序列化为类对象
            string inputStr = """
                {
                  "Description": "Mars 乌托邦平原一点的光照",
                  "Start": "7 Apr 2023 04:00:00.00000",
                  "Stop": "8 Apr 2023 04:00:00.00000",

                  "Position": {
                    "$type": "SitePosition",
                    "CentralBody": "Mars",
                    "cartographicDegrees": [ 118, 49.7, 0 ]
                  },

                  "TimeStepSec": 3600,
                  "MinStepSec": 1
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
            //  与STK对比
            double ebsl = 0.2;
            Assert.AreEqual(47722.187, output.SunLight.Intervals[0].Duration, ebsl);
            Assert.AreEqual(5549.997, output.SunLight.Intervals[1].Duration, ebsl);
            Assert.AreEqual(140.898, output.Penumbra.Intervals[0].Duration, ebsl);
            Assert.AreEqual(140.851, output.Penumbra.Intervals[1].Duration, ebsl);
            Assert.AreEqual(32846.066, output.Umbra.Intervals[0].Duration, ebsl);   
        }

        /*
    2025/5/29 9:09:58

sunLight intervals: 
=================================
2023-04-07T04:00:00.000Z  2023-04-07T17:15:22.188Z       47722.2
2023-04-08T02:27:29.999Z  2023-04-08T04:00:00.000Z        5550.0

Penumbra intervals: 
=================================
2023-04-07T17:15:22.188Z  2023-04-07T17:17:43.088Z         140.9
2023-04-08T02:25:09.153Z  2023-04-08T02:27:29.999Z         140.8

Umbra intervals: 
=================================
2023-04-07T17:17:43.088Z  2023-04-08T02:25:09.153Z       32846.1


          */

    }
}