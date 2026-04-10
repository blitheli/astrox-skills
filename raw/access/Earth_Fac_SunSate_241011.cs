using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace AeroSpace.Access.Tests;

public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-太阳系 ICRF 卫星 Access
            函数: AccessComputeV2
            卫星的历元参数采用TwoBody           
        
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

        测试结果:   <0.1s

        20241011    初次创建
        20250528    增加对光延迟
     */

    [TestMethod()]
    public void Earth_Fac_SunSate_241011()
    {
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "地球地面站-日心ICRF系TwoBody卫星",
              "Start": "2018-12-30T00:00:00Z",
              "Stop": "2019-12-11T00:00:00Z",

              "FromObjectPath": {
                "Name": "Sanya",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ 109.311, 18.313, 0 ],
                  "clampToGround": false
                  }
                },
              "ToObjectPath": {
                "Position": {
                    "$type": "TwoBody",
                    "CentralBody": "Sun",
                    "GravitationalParameter": 1.3271244004193938E20,
                    "OrbitEpoch": "2024-03-31T00:00:00.000Z",
                    "CoordSystem": "ICRF",
                    "CoordType": "Classical",
                    "OrbitalElements": [ 168442669593.0, 0.203731, 2.9469573e+01, 6.7977521e+01, 4.24608e-01, 1.4704501362454613e+02]
                }              
              },
              "UseLightTimeDelay": true
            }              
            """;

        //  读取json文件，并序列化为类对象
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



}