using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  moonSate-earth Access,  无约束(默认月遮挡)
            函数: AccessComputeV2
                    
            moonSate采用二体轨道初始参数输入(TwoOrbitInput)
            earth输入采用CentralBodyPath类型
    
        STK 场景: LunarTerrain                
            satellite:      moonSate
            centralbody:    earth
            
            计算Access

            moonSate-To-Earth
            -----------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 04:00:00.000    25 Apr 2022 05:09:13.366          4153.366
                   2    25 Apr 2022 05:49:12.865    25 Apr 2022 07:27:24.099          5891.234
                   3    25 Apr 2022 08:07:11.684    25 Apr 2022 09:45:35.138          5903.454
                   4    25 Apr 2022 10:25:10.858    25 Apr 2022 12:03:46.465          5915.607
                   5    25 Apr 2022 12:43:10.400    25 Apr 2022 14:00:00.000          4609.600

            详细结果参见: moonSate_earth_Stk结果.txt
                    

        测试结果:   与STK的结果相差< 0.02s !   

        20220518    初次创建
        20230824    修改为AccessController2
     */

    [TestMethod()]
    public void moonSate_earth_20220518()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "moonSate-earth Access, 无约束(默认月遮挡)",
              "Start": "2022-04-25T04:00:00Z",
              "Stop": "2022-04-25T14:00:00Z",

              "FromObjectPath": {
                "Name": "moonSate",
                "Position": {
                  "$type":"TwoBody",
                  "CentralBody": "Moon",
                  "GravitationalParameter": 4.90280030555540e+012,
                  "OrbitEpoch": "25 Apr 2022 04:00:00.000000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 2037400, 0, 45, 0, 90, 0 ]
                }
              },
              "ToObjectPath": {
                "Position": {
                  "$type":"CentralBody",
                  "Name": "Earth"
                }
              }
            }                
            """;

        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "Out_moonSate_earth_20220518.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(5, output.Passes.Length);
        var ebsl = 0.01;
        Assert.AreEqual(4153.374, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(5891.245, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(5903.463, output.Passes[2].Duration, ebsl);
        
        Console.WriteLine(output.ToString());                     
    }


    /*
    持续时间: 253 毫秒

      标准输出: 
    计算完成，结果写入到文件！

    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2022-04-25T04:00:00.000Z    2022-04-25T05:09:13.376Z          4153.377
         2    2022-04-25T05:49:12.864Z    2022-04-25T07:27:24.107Z          5891.243
         3    2022-04-25T08:07:11.683Z    2022-04-25T09:45:35.149Z          5903.465
         4    2022-04-25T10:25:10.859Z    2022-04-25T12:03:46.473Z          5915.614
         5    2022-04-25T12:43:10.399Z    2022-04-25T14:00:00.000Z          4609.600

     */

}