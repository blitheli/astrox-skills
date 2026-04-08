using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试  StHelens地面站-天和空间站 Access,  仅考虑地面仰角:5°限制
            函数: AccessComputeV1
            天和空间站轨道采用TLE输入
        
        STK 场景: LunarTerrain
            facility:       StHelens( -122.189, 46.1956, 1912 )
            satellite:      TIANHE_48274.e
            
            仅考虑地面仰角5°约束，计算Access

            stHelen0-To-TIANHE_48274
            ------------------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 08:50:59.865    25 Apr 2022 08:55:03.294           243.429
                   2    25 Apr 2022 10:25:20.145    25 Apr 2022 10:32:21.501           421.356
                   3    25 Apr 2022 12:01:15.584    25 Apr 2022 12:08:38.597           443.013
                   4    25 Apr 2022 13:37:42.103    25 Apr 2022 13:44:17.994           395.890

            详细结果参见: StHelens_Tianhe_Stk结果(5°仰角).txt
                    

        输入文件:  StHelens_Tianhe_El_Input_20220518.json        
     

        测试结果:   与STK的结果几乎一致!   

        20220518    初次创建
        20230824    更改为AccessComputeV2
     */

    [TestMethod()]
    public void StHelens_Tianhe_El_20220518()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");


        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "Description": "StHelens-Tianhe Access,TLE参数,仅考虑地面仰角:5°限制",
              "Start": "2022-04-25T08:00:00Z",
              "Stop": "2022-04-25T14:00:00Z",

              "ComputeAER": true,

              "FromObjectPath": {
                "Name": " St.Helens Facility",
                "Position": {
                  "$type": "SitePosition",
                  "cartographicDegrees": [ -122.189, 46.1956, 1912 ]
                },
                "Constraints": [
                  {
                    "$type": "ElevationAngle",
                    "MinimumValue": 5.0
                  }
                ]
              },

              "ToObjectPath": {
                "Name": "TIANHE-48274SS",
                "Position": {
                  "$type": "SGP4",
                  "SatelliteNumber": "48274",
                  "TLEs": [
                    "1 48274U 21035A   22090.17413503  .00052719  00000-0  55387-3 0  9998",
                    "2 48274  41.4719 333.9464 0008030 272.6485 224.0827 15.63436205 52579"
                  ]
                }
              }
            }
            
            """;
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "Out_StHelens_Tianhe_El_20220518.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);

        Assert.AreEqual(4, output.Passes.Length);
        double ebsl = 0.02;
        Assert.AreEqual(243.451, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(421.359, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(443.009, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(395.886, output.Passes[3].Duration, ebsl);

        Console.WriteLine(output.ToString());
    }

    /*        
   持续时间: 211 毫秒
      计算完成，结果写入到文件！

    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2022-04-25T08:50:59.868Z    2022-04-25T08:55:03.320Z           243.451
         2    2022-04-25T10:25:20.156Z    2022-04-25T10:32:21.515Z           421.359
         3    2022-04-25T12:01:15.601Z    2022-04-25T12:08:38.612Z           443.011
         4    2022-04-25T13:37:42.119Z    2022-04-25T13:44:18.006Z           395.887

                            Time (UTCG)          Azimuth (deg)    Elevation (deg)     Range (km)
                     ------------------------    -------------    ---------------    -----------
                     2022-04-25T08:50:59.868Z         -178.539              5.000       1700.870
                     2022-04-25T08:51:59.868Z          167.117              7.199       1530.012
                     2022-04-25T08:52:59.868Z          150.288              8.108       1466.690
                     2022-04-25T08:53:59.868Z          133.394              7.292       1524.708
                     2022-04-25T08:54:59.868Z          118.910              5.148       1691.471
                     2022-04-25T08:55:03.320Z          118.177              5.000       1703.796
    Global Statistics
    -----------------
    Min Elevation    2022-04-25T08:55:03.320Z          118.177              5.000       1703.796
    Max Elevation    2022-04-25T08:53:01.527Z          149.808              8.109       1466.661
    Min Range        2022-04-25T08:53:01.221Z          149.896              8.109       1466.659
    Max Range        2022-04-25T08:55:03.320Z          118.177              5.000       1703.796
     */

}