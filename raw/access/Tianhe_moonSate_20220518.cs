using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  天和空间站-moonSate Access,  无约束(默认地、月遮挡)
            函数: AccessComputeV2

            天和空间站轨道采用TLE输入(TLEs)
            moonSate采用二体轨道初始参数输入(TwoOrbitInput)
                
        STK 场景: LunarTerrain
            satellite:      TIANHE_48274
            satellite:      moonSate
            
            计算Access

            TIANHE_48274-To-moonSate
            ------------------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 04:16:55.924    25 Apr 2022 05:08:58.596          3122.673
                   2    25 Apr 2022 05:49:40.057    25 Apr 2022 06:46:00.425          3380.368
                   3    25 Apr 2022 07:21:02.074    25 Apr 2022 07:27:35.007           392.933
                   4    25 Apr 2022 08:06:41.524    25 Apr 2022 08:18:04.674           683.150
                   5    25 Apr 2022 08:53:09.443    25 Apr 2022 09:45:22.071          3132.628
                   6    25 Apr 2022 10:25:38.224    25 Apr 2022 11:21:51.279          3373.055
                   7    25 Apr 2022 11:57:13.817    25 Apr 2022 12:03:55.751           401.934
                   8    25 Apr 2022 12:42:40.011    25 Apr 2022 12:53:55.224           675.213
                   9    25 Apr 2022 13:29:19.483    25 Apr 2022 13:59:58.758          1839.275

            详细结果参见: Tianhe_moonSate_Stk结果.txt

        测试结果:   与STK的结果相差< 0.1s !   
            最后一个超出了时间边界，与STK处理的方式不一样！

        20220518    初次创建
        20230824    更改为AccessComputeV2
        20250528    增加对光延迟
     */

    [TestMethod()]
    public void Tianhe_moonSate_20220518()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");
                  
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "Tianhe-moonSate Access,无约束(默认地、月遮挡)",
              "Start": "2022-04-25T04:00:00Z",
              "Stop": "2022-04-25T14:00:00Z",

              "FromObjectPath": {
                "Name": "TIANHE-48274SS",
                "Position": {
                  "$type": "SGP4",
                  "SatelliteNumber": "48274",
                  "TLEs": [
                    "1 48274U 21035A   22090.17413503  .00052719  00000-0  55387-3 0  9998",
                    "2 48274  41.4719 333.9464 0008030 272.6485 224.0827 15.63436205 52579"
                  ]
                }
              },

              "ToObjectPath": {
                "Name": "moonSate",
                "Position": {
                  "$type": "TwoBody",
                  "CentralBody": "Moon",
                  "GravitationalParameter": 4.90280030555540e+012,
                  "OrbitEpoch": "25 Apr 2022 04:00:00.000000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 2037400, 0, 45, 0, 90, 0 ]
                }
              },      
              "UseLightTimeDelay": true
            }
            
            """;
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);
                        
        Console.WriteLine(output.ToString());
        
        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);            
        double ebsl = 0.1;
        Assert.AreEqual(3122.673, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(3380.368, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(392.933, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(683.150, output.Passes[3].Duration, ebsl);
        Assert.AreEqual(3132.628, output.Passes[4].Duration, ebsl);
        Assert.AreEqual(3373.055, output.Passes[5].Duration, ebsl);
        Assert.AreEqual(401.934, output.Passes[6].Duration, ebsl);
        Assert.AreEqual(675.213, output.Passes[7].Duration, ebsl);
        Assert.AreEqual(1839.275, output.Passes[8].Duration, 2);
    }

    /*
    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2022-04-25T04:16:55.921Z    2022-04-25T05:08:58.598Z          3122.677
     2    2022-04-25T05:49:40.054Z    2022-04-25T06:46:00.429Z          3380.375
     3    2022-04-25T07:21:02.071Z    2022-04-25T07:27:35.007Z           392.936
     4    2022-04-25T08:06:41.520Z    2022-04-25T08:18:04.671Z           683.151
     5    2022-04-25T08:53:09.441Z    2022-04-25T09:45:22.073Z          3132.633
     6    2022-04-25T10:25:38.226Z    2022-04-25T11:21:51.283Z          3373.057
     7    2022-04-25T11:57:13.815Z    2022-04-25T12:03:55.751Z           401.937
     8    2022-04-25T12:42:40.007Z    2022-04-25T12:53:55.224Z           675.217
     9    2022-04-25T13:29:19.481Z    2022-04-25T14:00:00.000Z          1840.519

     */

}