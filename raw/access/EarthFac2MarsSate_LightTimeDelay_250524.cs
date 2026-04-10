using System.Reflection;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  地球 地面站 - 火星卫星 Access,  仅火星遮挡，考虑光延迟
     
            地球地面站作为接收器

            函数: AccessComputeV2

            卫星的历元参数采用TwoBody形式                                         

        测试结果:   与STK的结果几乎一致!   <0.1s
              
            TBD:    有3个点的时间误差较大，3s， 待排查!

        20250524    初次编写
        20250528    Access里link的参考坐标系改为transmitter的中心天体惯性系      
     */

    /*
     
        STK 结果  （Access计算时，Access for对象为 transmitter)
                                                                              24 May 2025 20:35:30
Mars_Sate-To-Place1
-------------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1    25 Apr 2025 04:00:39.169    25 Apr 2025 04:07:04.250           385.082
                       2    25 Apr 2025 04:37:21.740    25 Apr 2025 06:00:48.162          5006.422
                       3    25 Apr 2025 06:31:04.688    25 Apr 2025 07:54:32.060          5007.372
                       4    25 Apr 2025 08:24:47.631    25 Apr 2025 09:48:15.952          5008.321
                       5    25 Apr 2025 10:18:30.561    25 Apr 2025 11:41:59.842          5009.281
                       6    25 Apr 2025 12:12:13.496    25 Apr 2025 13:35:43.739          5010.244
                       7    25 Apr 2025 14:05:56.424    25 Apr 2025 15:29:27.646          5011.223
                       8    25 Apr 2025 15:59:39.355    25 Apr 2025 17:23:11.571          5012.217
                       9    25 Apr 2025 17:53:22.292    25 Apr 2025 18:33:47.279          2424.987
                      10    26 Apr 2025 03:58:58.112    26 Apr 2025 04:00:00.000            61.888

Statistics
----------
Min Duration           8    25 Apr 2025 17:53:22.247    25 Apr 2025 18:45:12.971          3110.723
Max Duration           7    25 Apr 2025 15:59:39.310    25 Apr 2025 17:23:11.427          5012.117
Mean Duration                                                                             4771.893
Total Duration                                                                           38175.140
    */

    [TestMethod()]
    public void EarthFac2MarsSate_LightTimeDelay_250524()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");
                   
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "地球地面站-火星卫星 Access(地面站作为接收器), 考虑光延迟",
              "Start": "25 Apr 2025 04:00:00.000",
              "Stop": "26 Apr 2025 04:00:00.000",

              "FromObjectPath": {
                "Name": "Mars_Sate",
                "Position": {
                  "$type": "TwoBody",
                  "CentralBody": "Mars",
                  "GravitationalParameter": 4.282837564100000e+013,
                  "OrbitEpoch": "25 Apr 2025 04:00:00.000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 3696190, 0, 40, 0, 0, 0 ]
                }
              },

              "ToObjectPath": {
                "Name": "fac1",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ 100, 40, 0 ]
                }
              },      
              "UseLightTimeDelay": true
            }
            
            """;
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出结果到控制台
        Console.Write(DateTime.Now.ToString());
        Console.WriteLine(output.ToString());

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);           
        double ebsl = 0.1;
        Assert.AreEqual(385.082, output.Passes[0].Duration, 3.0);
        Assert.AreEqual(5006.422, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(5007.372, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(5008.321, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(5009.281, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(5010.244, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(5011.223, output.Passes[6].Duration, ebsl);
        Assert.AreEqual(5012.217, output.Passes[7].Duration, ebsl);
        Assert.AreEqual(2424.987, output.Passes[8].Duration, 3);
        Assert.AreEqual(61.888, output.Passes[9].Duration, 3);

        /*  
    2025/5/28 22:50:19
Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2025-04-25T04:00:36.789Z    2025-04-25T04:07:04.251Z           387.461
     2    2025-04-25T04:37:21.736Z    2025-04-25T06:00:48.161Z          5006.424
     3    2025-04-25T06:31:04.686Z    2025-04-25T07:54:32.061Z          5007.375
     4    2025-04-25T08:24:47.628Z    2025-04-25T09:48:15.954Z          5008.326
     5    2025-04-25T10:18:30.562Z    2025-04-25T11:41:59.845Z          5009.283
     6    2025-04-25T12:12:13.492Z    2025-04-25T13:35:43.738Z          5010.246
     7    2025-04-25T14:05:56.420Z    2025-04-25T15:29:27.647Z          5011.227
     8    2025-04-25T15:59:39.350Z    2025-04-25T17:23:11.568Z          5012.218
     9    2025-04-25T17:53:22.287Z    2025-04-25T18:33:47.982Z          2425.694
    10    2025-04-26T03:58:55.691Z    2025-04-26T04:00:00.000Z            64.309

         */
    }




    /*  测试  地球地面站-火星卫星 Access,  仅火星遮挡，考虑光延迟

        火星卫星作为 接收器

        函数: AccessComputeV2

        卫星的历元参数采用TwoBody形式                                         

    测试结果:   与STK的结果几乎一致!   <0.1s
                
            TBD:    最后一点的时间误差较大！ 待排查！

    20250524    初次编写

 */

    /*
     
        STK 结果  （Access计算时，Access for对象为 transmitter)

Place1-To-Mars_Sate
-------------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1    25 Apr 2025 04:25:59.270    25 Apr 2025 05:49:25.235          5005.965
                       2    25 Apr 2025 06:19:41.859    25 Apr 2025 07:43:08.774          5006.915
                       3    25 Apr 2025 08:13:24.440    25 Apr 2025 09:36:52.303          5007.863
                       4    25 Apr 2025 10:07:07.012    25 Apr 2025 11:30:35.828          5008.816
                       5    25 Apr 2025 12:00:49.574    25 Apr 2025 13:24:19.355          5009.781
                       6    25 Apr 2025 13:54:32.130    25 Apr 2025 15:18:02.882          5010.752
                       7    25 Apr 2025 15:48:14.686    25 Apr 2025 17:11:46.425          5011.739
                       8    25 Apr 2025 17:41:57.246    25 Apr 2025 18:45:13.543          3796.298
    */

    [TestMethod()]
    public void EarthFac2MarsSate2_LightTimeDelay_250524()
    {
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "地球地面站-火星卫星 Access(卫星作为接收器), 考虑光延迟",
              "Start": "25 Apr 2025 04:00:00.000",
              "Stop": "26 Apr 2025 04:00:00.000",

              "FromObjectPath": {
                "Name": "fac1",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ 100, 40, 0 ]
                }
              },            
            
              "ToObjectPath": {
                "Name": "Mars_Sate",
                "Position": {
                  "$type": "TwoBody",
                  "CentralBody": "Mars",
                  "GravitationalParameter": 4.282837564100000e+013,
                  "OrbitEpoch": "25 Apr 2025 04:00:00.000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 3696190, 0, 40, 0, 0, 0 ]
                }
              },

            
              "UseLightTimeDelay": true
            }
            
            """;
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出结果到控制台
        Console.Write(DateTime.Now.ToString());
        Console.WriteLine(output.ToString());

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        double ebsl = 0.1;
        Assert.AreEqual(5005.965, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(5006.915, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(5007.863, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(5008.816, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(5009.781, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(5010.752, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(5011.739, output.Passes[6].Duration, ebsl);
        Assert.AreEqual(3796.298, output.Passes[7].Duration, 1.0);  
    }

    /*  
    2025/5/26 12:13:53
Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2025-04-25T04:25:59.234Z    2025-04-25T05:49:25.205Z          5005.971
     2    2025-04-25T06:19:41.823Z    2025-04-25T07:43:08.744Z          5006.921
     3    2025-04-25T08:13:24.404Z    2025-04-25T09:36:52.274Z          5007.870
     4    2025-04-25T10:07:06.976Z    2025-04-25T11:30:35.798Z          5008.823
     5    2025-04-25T12:00:49.540Z    2025-04-25T13:24:19.322Z          5009.783
     6    2025-04-25T13:54:32.095Z    2025-04-25T15:18:02.853Z          5010.757
     7    2025-04-25T15:48:14.652Z    2025-04-25T17:11:46.395Z          5011.742
     8    2025-04-25T17:41:57.214Z    2025-04-25T18:45:12.882Z          3795.668
     */

}