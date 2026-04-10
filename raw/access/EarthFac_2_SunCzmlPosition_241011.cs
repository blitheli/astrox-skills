using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站- Sun ICRF CzmlPositino类型卫星 Access
            函数: AccessComputeV2
            卫星的历元参数采用 CzmlPosition形式           
            
        STK 场景: Bennu

            facility:       (109.311°, 18.313°, 0)
            satellite:      TwoBody

       Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
        ------    ------------------------    ------------------------    --------------
            1    30 Dec 2018 02:48:21.479    30 Dec 2018 14:42:03.987         42822.507
            2    31 Dec 2018 02:47:22.141    31 Dec 2018 14:42:25.219         42903.077
            3     1 Jan 2019 02:46:23.371     1 Jan 2019 14:42:47.477         42984.106
            4     2 Jan 2019 02:45:25.212     2 Jan 2019 14:43:10.798         43065.586
            5     3 Jan 2019 02:44:27.709     3 Jan 2019 14:43:35.231         43147.522
            6     4 Jan 2019 02:43:30.907     4 Jan 2019 14:44:00.809         43229.902
            7     5 Jan 2019 02:42:34.853     5 Jan 2019 14:44:27.568         43312.715
                            
        与STK对比:   <0.1s

        20241011    初次创建
        20250528    增加对光延迟
     */

    [TestMethod()]
    public void EarthFac_2_SunCzmlPosition_241011()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "EarthFac_2_SunCzmlPosition_241011.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        Console.WriteLine(output.ToString());

        double ebsl = 0.1;
        Assert.AreEqual(42822.507, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(42903.077, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(42984.106, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(43065.586, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(43147.522, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(43229.902, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(43312.715, output.Passes[6].Duration, ebsl);
    }

    /*
    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2018-12-30T02:48:21.445Z    2018-12-30T14:42:03.952Z         42822.506
     2    2018-12-31T02:47:22.107Z    2018-12-31T14:42:25.182Z         42903.075
     3    2019-01-01T02:46:23.333Z    2019-01-01T14:42:47.458Z         42984.125
     4    2019-01-02T02:45:25.170Z    2019-01-02T14:43:10.763Z         43065.594
     5    2019-01-03T02:44:27.666Z    2019-01-03T14:43:35.193Z         43147.527
     6    2019-01-04T02:43:30.865Z    2019-01-04T14:44:00.770Z         43229.905
     7    2019-01-05T02:42:34.812Z    2019-01-05T14:44:27.532Z         43312.720
     8    2019-01-06T02:41:39.555Z    2019-01-06T14:44:55.532Z         43395.978
     9    2019-01-07T02:40:45.140Z    2019-01-07T14:45:24.786Z         43479.646
    10    2019-01-08T02:39:51.616Z    2019-01-08T14:45:55.373Z         43563.757

     */

}