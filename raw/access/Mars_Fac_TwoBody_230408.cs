using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  火星 地面站-卫星 Access,  仅火星遮挡，且地面站高度<0
            函数: AccessComputeV1

            卫星的历元参数采用TwoBody形式                                         

        测试结果:   与STK的结果几乎一致!   <0.01s

        20230409    初次创建     
        20230824    更改为ComputeV2计算模型

     */

    /*
       STK 结果:                 
        wutuobang-To-TW1
        ----------------
            Access       Start Time (UTCG)           Stop Time (UTCG)       Duration (sec)
            ------    -----------------------    -----------------------    --------------
                1    7 Apr 2023 04:34:00.080    7 Apr 2023 04:47:12.056           791.976
                2    7 Apr 2023 13:41:20.097    7 Apr 2023 13:50:15.611           535.514
                3    7 Apr 2023 15:30:33.436    7 Apr 2023 15:45:26.332           892.896
                4    7 Apr 2023 17:26:35.185    7 Apr 2023 17:38:01.257           686.072
                5    8 Apr 2023 03:18:22.596    8 Apr 2023 03:32:51.189           868.593
    */

    [TestMethod()]
    public void Mars_Fac_TwoBody_230408()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");
                   
        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "Fac1-TwoBody Access,无限制, 默认火星遮挡",
              "Start": "7 Apr 2023 04:00:00.00000",
              "Stop": "8 Apr 2023 04:00:00.00000",

              "FromObjectPath": {
                "Name": "乌托邦平原",
                "Position": {
                  "$type": "SitePosition",
                  "CentralBody": "Mars",
                  "cartographicDegrees": [ 118, 49.7, 0 ]
                }
              },

              "ToObjectPath": {
                "Name": "天问一号",
                "Position": {
                  "$type": "TwoBody",
                  "CentralBody": "Mars",
                  "GravitationalParameter": 4.282837564100000e+013,
                  "OrbitEpoch": "7 Apr 2023 04:00:00.00000",
                  "CoordSystem": "Inertial",
                  "CoordType": "Classical",
                  "OrbitalElements": [ 3696190, 0, 90, 0, 0, 0 ]
                }
              }
            }
            
            """;
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出结果到控制台
        Console.Write(DateTime.Now.ToString());
        Console.WriteLine(output.ToString());

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "Mars_Fac_TwoBody_230408_out.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);           
        double ebsl = 0.01;
        Assert.AreEqual(791.975, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(535.519, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(892.904, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(686.075, output.Passes[3].Duration, ebsl);

    }


    /*
      持续时间: 448 毫秒

          标准输出: 
        2023/4/9 13:00:17
        Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
        ------    ------------------------    ------------------------    --------------
             1    2023-04-07T04:34:00.085Z    2023-04-07T04:47:12.060Z           791.975
             2    2023-04-07T13:41:20.099Z    2023-04-07T13:50:15.618Z           535.519
             3    2023-04-07T15:30:33.436Z    2023-04-07T15:45:26.340Z           892.904
             4    2023-04-07T17:26:35.188Z    2023-04-07T17:38:01.263Z           686.075
             5    2023-04-08T03:18:22.602Z    2023-04-08T03:32:51.194Z           868.591

     */

}