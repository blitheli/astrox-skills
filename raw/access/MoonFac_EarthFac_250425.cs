using System.Reflection;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  月球 地面站-地球 地面站
            函数: AccessComputeV2

            月球 地面站 博得月溪
                    -   光照约束: DirectSun
            地球 地面站 佳木斯
                    -   仰角约束

            虽然有日食（地球遮挡），但是日食段正好不Access，所以最终结果和日食无关!

        与STK对比：     < 0.1s

        20250425    初次创建 
        20250528    增加对光延迟        
     */

    /*
       STK 结果: 
        bode-To-Jiamusi
        ---------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1     1 Jun 2029 15:46:07.651     1 Jun 2029 23:51:32.854         29125.204
                       2     2 Jun 2029 16:05:23.030     3 Jun 2029 01:02:03.951         32200.921
                       3     3 Jun 2029 16:23:26.133     4 Jun 2029 02:08:44.193         35118.060
                       4     4 Jun 2029 16:41:31.127     4 Jun 2029 19:37:09.234         10538.107
                       5    19 Jun 2029 13:19:18.344    19 Jun 2029 13:51:36.530          1938.186
                       6    20 Jun 2029 05:36:01.152    20 Jun 2029 14:13:17.832         31036.680
                       7    21 Jun 2029 06:59:05.804    21 Jun 2029 14:36:51.448         27465.645
                       8    22 Jun 2029 08:24:55.133    22 Jun 2029 15:04:52.312         23997.179
                       9    23 Jun 2029 09:49:34.677    23 Jun 2029 15:41:12.695         21098.018
                      10    24 Jun 2029 11:05:50.372    24 Jun 2029 16:30:37.729         19487.357
                      11    25 Jun 2029 12:06:42.175    25 Jun 2029 17:35:26.393         19724.218
                      12    26 Jun 2029 12:51:06.184    26 Jun 2029 18:51:37.977         21631.794
                      13    27 Jun 2029 13:23:17.782    27 Jun 2029 20:11:36.013         24498.231
                      14    28 Jun 2029 13:48:05.360    28 Jun 2029 21:29:40.284         27694.924
                      15    29 Jun 2029 14:08:50.056    29 Jun 2029 22:43:31.217         30881.161
                      16    30 Jun 2029 14:27:38.759    30 Jun 2029 23:53:00.205         33921.446
    */

    [TestMethod()]
    public void MoonFac_EarthFac_250425()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");
                   
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "月球地面站-地球地面站,月球地面站光照约束，地球地面站仰角约束，有日食",
              "Start": "2029-06-01T04:00:00.000Z",
              "Stop": "2029-07-01T04:00:00.000Z",

              "FromObjectPath": {
                "Name": "月球博得月溪",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Moon",
                  "cartographicDegrees": [ -4.8, 11, 0 ]
                },
                "Lighting":"DirectSun"
              },

              "ToObjectPath": {
                "Name": "地球-佳木斯",
                 "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ 130.78, 46.5, 0 ]
                },
                "Constraints": [
                  {
                    "$type": "ElevationAngle",
                    "MinimumValue": 10.0
                  }
                ]
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
        Assert.AreEqual(29125.204, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(32200.921, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(35118.060, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(10538.107, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(1938.186, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(31036.680, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(27465.645, output.Passes[6].Duration, ebsl);
        Assert.AreEqual(23997.179, output.Passes[7].Duration, ebsl);
    }

    /*

  标准输出: 
2025/5/28 22:21:50
Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2029-06-01T15:46:07.669Z    2029-06-01T23:51:32.925Z         29125.256
     2    2029-06-02T16:05:23.048Z    2029-06-03T01:02:04.017Z         32200.969
     3    2029-06-03T16:23:26.152Z    2029-06-04T02:08:44.261Z         35118.109
     4    2029-06-04T16:41:31.149Z    2029-06-04T19:37:09.297Z         10538.148
     5    2029-06-19T13:19:18.286Z    2029-06-19T13:51:36.549Z          1938.262
     6    2029-06-20T05:36:01.227Z    2029-06-20T14:13:17.856Z         31036.629
     7    2029-06-21T06:59:05.881Z    2029-06-21T14:36:51.476Z         27465.595
     8    2029-06-22T08:24:55.210Z    2029-06-22T15:04:52.339Z         23997.129
     9    2029-06-23T09:49:34.751Z    2029-06-23T15:41:12.731Z         21097.980
    10    2029-06-24T11:05:50.435Z    2029-06-24T16:30:37.775Z         19487.340
    11    2029-06-25T12:06:42.221Z    2029-06-25T17:35:26.456Z         19724.235
    12    2029-06-26T12:51:06.221Z    2029-06-26T18:51:38.049Z         21631.828
    13    2029-06-27T13:23:17.813Z    2029-06-27T20:11:36.084Z         24498.272
    14    2029-06-28T13:48:05.384Z    2029-06-28T21:29:40.353Z         27694.969
    15    2029-06-29T14:08:50.078Z    2029-06-29T22:43:31.282Z         30881.204
    16    2029-06-30T14:27:38.778Z    2029-06-30T23:53:00.267Z         33921.489
     */


    /*
     
            月球 地面站 博得月溪
                    -   光照约束: DirectSun
            地球 地面站 喀什
                    -   仰角约束

            1月1日-4日，博得月溪一直有光照

        与STK对比：     < 0.1s

        20250530    初次创建

    STK 结果
     bode-To-kashi
-------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1     1 Jan 2029 00:00:00.000     1 Jan 2029 01:31:22.375          5482.375
                       2     1 Jan 2029 13:33:56.926     2 Jan 2029 02:14:54.540         45657.615
                       3     2 Jan 2029 14:43:03.036     3 Jan 2029 02:52:48.631         43785.595
                       4     3 Jan 2029 15:52:57.529     4 Jan 2029 03:26:06.616         41589.087

     */
    [TestMethod()]
    public void MoonBode_EarthKashi_250530()
    {
       
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "月球地面站-地球地面站,月球地面站光照约束，地球地面站仰角约束，有日食",
              "Start": "2029-01-01T00:00:00.000Z",
              "Stop": "2029-01-04T00:00:00.000Z",

              "FromObjectPath": {
                "Name": "月球博得月溪",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Moon",
                  "cartographicDegrees": [ -4.8, 11, 0 ]
                },
                "Lighting":"DirectSun"
              },

              "ToObjectPath": {
                 "Name": "喀什",
                  "Position": {
                    "$type": "SitePosition",
                    "CentralBody": "Earth",
                    "cartographicDegrees": [ 76.71, 38.42, 0 ]
                  },
                "Constraints": [
                  {
                    "$type": "ElevationAngle",
                    "MinimumValue": 10.0
                  }
                ]
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
        Assert.AreEqual(5482.375, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(45657.615, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(43785.595, output.Passes[2].Duration, ebsl);

    }
    /*
         2025/5/30 11:07:57
    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2029-01-01T00:00:00.000Z    2029-01-01T01:31:22.416Z          5482.417
         2    2029-01-01T13:33:56.990Z    2029-01-02T02:14:54.576Z         45657.586
         3    2029-01-02T14:43:03.094Z    2029-01-03T02:52:48.665Z         43785.572
         4    2029-01-03T15:52:57.590Z    2029-01-04T00:00:00.000Z         29222.409
     */
}