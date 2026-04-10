using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  月球南极 点-地球站点 Access,  无约束(默认月遮挡)， 1年
            函数: AccessComputeV1                        
           
        南极点:    -75, -89.5, 0
        地球点:    -75.5966, 40.03861, 0
    
        STK 结果
        MoonSp1-To-EarthFac1
        --------------------
                  Access       Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
                  ------    -----------------------    -----------------------    --------------
                       1    2 Jan 2023 00:01:12.705    2 Jan 2023 08:27:16.363         30363.658
                       2    2 Jan 2023 18:40:20.259    3 Jan 2023 04:00:00.000         33579.741

        输入文件:  MoonSpSite2EarthSite_V1_20221229.json                 

        测试结果:   与STK的结果相差 约 4s !           
                    主要是因为在南极点附近，处于临界情况！

        20221229    初次创建
        20230824    更改为ComputeV2计算模型
        20250528    增加对光延迟
     */

    [TestMethod()]
    public void MoonSpSite2EarthSite_V1_20221229()
    {
                
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "月球南极附近 点-地球地面站 Access, 无约束(默认月遮挡)",
              "Start": "2023-01-01T04:00:00Z",
              "Stop": "2023-01-03T04:00:00Z",
              "ComputeAER": false,
            
              "ToObjectPath": {
                "Name": "Earth_Site",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ -75.5966, 40.03861, 0 ]
                }
              },  
              
              "FromObjectPath": {
                "Name": "moonSouthPoleSite",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Moon",
                  "cartographicDegrees": [ -75, -89.5, 0 ]
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
        double ebsl = 4;
        Assert.AreEqual(30363.658, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(33579.741, output.Passes[1].Duration, ebsl);               
    }

    /*        
    持续时间: 929 毫秒
    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2023-01-02T00:01:11.029Z    2023-01-02T08:27:17.532Z         30366.503
     2    2023-01-02T18:40:20.913Z    2023-01-03T04:00:00.000Z         33579.086
     */

}