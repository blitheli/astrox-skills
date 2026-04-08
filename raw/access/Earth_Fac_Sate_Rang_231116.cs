using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace AeroSpace.Access.Tests;

[TestClass()]
public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-卫星 Access,  地球遮挡+卫星距离约束
            函数: AccessComputeV2
            地面站高度为35786km， 模拟GEO卫星
            卫星的历元参数采用J2形式,最大距离2000km约束           
        
        STK 场景: 
            facility:       (176°, -3°, 35786km)
            satellite:      J2积分器,
                            2000km最大距离约束

            Point2-To-Sate2
            ---------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    18 Nov 2023 13:46:04.735    18 Nov 2023 19:07:33.514         19288.779

        与STK对比:   <0.1s

        20231116    初次创建
        20250528    增加对光延迟
     */

    [TestMethod()]
    public void Earth_Fac_Sate_Rang_231116()
    {
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "earth-fac-Sate Access",
              "Start": "2023-11-15T00:00:00Z",
              "Stop": "2023-11-20T00:00:00Z",

              "FromObjectPath": {                
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Earth",
                  "cartographicDegrees": [ 176, -3, 35786000 ],
                  "clampToGround": false
                  }
                },
              "ToObjectPath": {
                "Position": {
                    "$type": "J2",
                    "CentralBody": "Earth",
                    "GravitationalParameter": 398600441800000,
                    "OrbitEpoch": "2023-11-15T00:00:00.000Z",
                    "CoordSystem": "Inertial",
                    "CoordType": "Classical",
                    "OrbitalElements": [ 41164000, 0, 2, 0, 180, 0 ]
                },
                "Constraints": [
                  {
                    "$type": "Range",
                    "MinimumValue": 0,
                    "MaximumValue": 2000,
                    "IsMaximumEnabled": true
                  }
                ]
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
        Assert.AreEqual(19288.779, output.Passes[0].Duration, ebsl);
    }

    /*
      Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2023-11-18T13:46:04.733Z    2023-11-18T19:07:33.515Z         19288.781

     */

}