using System.Reflection;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  月球 地面站-地球 地面站 考虑光延迟
            函数: AccessComputeV2

            月球 地面站
                    -   光照约束: DirectSun
            地球 地面站(南美-阿根廷）
                    -   仰角约束

            6月26日 有日食，地球遮挡月球地面站的光照

        与STK对比：     < 0.1s （非日食时，日食时 的结果都精确）

        20250425    初次创建    
        20250528    增加对光延迟  
     */

    /*
       STK 结果: 
bode-To-agenting
----------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1     1 Jun 2029 04:00:00.000     1 Jun 2029 14:45:31.508         38731.508
                       2     2 Jun 2029 03:43:57.574     2 Jun 2029 15:15:38.727         41501.153
                       3     3 Jun 2029 04:43:42.602     3 Jun 2029 15:42:15.948         39513.346
                       4     4 Jun 2029 05:42:09.914     4 Jun 2029 16:06:45.389         37475.475
                       5    19 Jun 2029 17:26:59.848    20 Jun 2029 04:43:45.703         40605.856
                       6    20 Jun 2029 17:59:53.701    21 Jun 2029 05:53:36.285         42822.583
                       7    21 Jun 2029 18:37:27.867    22 Jun 2029 07:04:09.577         44801.710
                       8    22 Jun 2029 19:21:12.408    23 Jun 2029 08:14:14.392         46381.984
                       9    23 Jun 2029 20:12:05.806    24 Jun 2029 09:21:29.610         47363.804
                      10    24 Jun 2029 21:09:56.901    25 Jun 2029 10:23:07.592         47590.692
                      11    25 Jun 2029 22:13:03.000    26 Jun 2029 01:00:02.238         10019.238
                      12    26 Jun 2029 05:34:49.272    26 Jun 2029 11:17:08.112         20538.840
                      13    26 Jun 2029 23:18:39.838    27 Jun 2029 12:03:02.592         45862.754
                      14    28 Jun 2029 00:24:11.399    28 Jun 2029 12:41:40.457         44249.058
                      15    29 Jun 2029 01:28:02.795    29 Jun 2029 13:14:28.874         42386.080
                      16    30 Jun 2029 02:29:47.563    30 Jun 2029 13:43:01.326         40393.764
                      17     1 Jul 2029 03:29:47.261     1 Jul 2029 04:00:00.000          1812.739
    */

    [TestMethod()]
    public void MoonFac_EarthFac2_250425()
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
                "Name": "地球-阿根廷",
                 "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ -70.15, -38.19, 0 ]
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
        Assert.AreEqual(38731.508, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(41501.153, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(39513.346, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(37475.475, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(40605.856, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(42822.583, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(44801.710, output.Passes[6].Duration, ebsl);
        Assert.AreEqual(46381.984, output.Passes[7].Duration, ebsl);
        Assert.AreEqual(47363.804, output.Passes[8].Duration, ebsl);
        Assert.AreEqual(47590.692, output.Passes[9].Duration, ebsl);

        //  这两个都是日食引起的偏差
        Assert.AreEqual(10019.238, output.Passes[10].Duration, ebsl);
        Assert.AreEqual(20538.840, output.Passes[11].Duration, 0.11);

        Assert.AreEqual(45862.754, output.Passes[12].Duration, ebsl);
        Assert.AreEqual(44249.058, output.Passes[13].Duration, ebsl);
        Assert.AreEqual(42386.080, output.Passes[14].Duration, ebsl);
        Assert.AreEqual(40393.764, output.Passes[15].Duration, ebsl);
        Assert.AreEqual(1812.739, output.Passes[16].Duration, ebsl);
    }


    /*
    2025/5/28 22:28:09
Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2029-06-01T04:00:00.000Z    2029-06-01T14:45:31.542Z         38731.543
     2    2029-06-02T03:43:57.635Z    2029-06-02T15:15:38.753Z         41501.119
     3    2029-06-03T04:43:42.662Z    2029-06-03T15:42:15.975Z         39513.313
     4    2029-06-04T05:42:09.973Z    2029-06-04T16:06:45.416Z         37475.443
     5    2029-06-19T17:26:59.880Z    2029-06-20T04:43:45.763Z         40605.883
     6    2029-06-20T17:59:53.737Z    2029-06-21T05:53:36.349Z         42822.612
     7    2029-06-21T18:37:27.904Z    2029-06-22T07:04:09.639Z         44801.736
     8    2029-06-22T19:21:12.451Z    2029-06-23T08:14:14.452Z         46382.001
     9    2029-06-23T20:12:05.857Z    2029-06-24T09:21:29.666Z         47363.808
    10    2029-06-24T21:09:56.958Z    2029-06-25T10:23:07.644Z         47590.686
    11    2029-06-25T22:13:03.061Z    2029-06-26T01:00:02.298Z         10019.237
    12    2029-06-26T05:34:49.221Z    2029-06-26T11:17:08.161Z         20538.940
    13    2029-06-26T23:18:39.901Z    2029-06-27T12:03:02.629Z         45862.728
    14    2029-06-28T00:24:11.454Z    2029-06-28T12:41:40.489Z         44249.035
    15    2029-06-29T01:28:02.852Z    2029-06-29T13:14:28.907Z         42386.054
    16    2029-06-30T02:29:47.623Z    2029-06-30T13:43:01.353Z         40393.730
    17    2029-07-01T03:29:47.321Z    2029-07-01T04:00:00.000Z          1812.678

     */

}