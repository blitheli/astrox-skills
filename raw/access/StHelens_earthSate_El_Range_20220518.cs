using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;

public partial class AccessComputeV2Tests
{
    /*  测试  StHelens地面站-卫星的Access,  考虑地面仰角:5°,最大距离1000km 限制
            函数: AccessComputeV2
            卫星的历元参数采用CzmlPosition形式

        STK 场景: LunarTerrain
            facility:       StHelens( -122.189, 46.1956, 1912 )
            satellite:      earthSate.e
            
            考虑地面仰角5°/最大距离1000km 约束，计算Access

              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 04:18:04.756    25 Apr 2022 04:22:11.728           246.972
                   2    25 Apr 2022 05:52:47.957    25 Apr 2022 05:56:48.275           240.318
                   3    25 Apr 2022 07:27:06.099    25 Apr 2022 07:31:26.610           260.510
                    
            详细结果参见: StHelens_earthSate_Stk结果(5°仰角,1000km).txt

        输入文件:  StHelens_earthSate_El_Range_Input_20220518.json        
            来源于: STK生成的场景，导出的czml文件,earthSate的position获取

        测试结果:   与STK的结果几乎一致!   

        20220518    初次创建
        20230824    更改为ComputeV2计算模型
     */

    [TestMethod()]
    public void StHelens_earthSate_El_Range_20220518()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "StHelens_earthSate_El_Range_20220518.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "Out_StHelens_earthSate_El_Range_20220518.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        double ebsl = 0.01;
        Assert.AreEqual(246.973, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(240.322, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(260.513, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(41.458, output.Passes[0].MaxElevationData.Elevation, ebsl);

        Console.WriteLine(output.ToString());
    }

    /*
    持续时间: 927 毫秒

      标准输出: 
    计算完成，结果写入到文件！

    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2022-04-25T04:18:04.766Z    2022-04-25T04:22:11.739Z           246.973
         2    2022-04-25T05:52:47.967Z    2022-04-25T05:56:48.289Z           240.323
         3    2022-04-25T07:27:06.109Z    2022-04-25T07:31:26.622Z           260.513

                            Time (UTCG)          Azimuth (deg)    Elevation (deg)     Range (km)
                     ------------------------    -------------    ---------------    -----------
                     2022-04-25T04:18:04.766Z          -81.363             13.808       1000.000
                     2022-04-25T04:19:04.766Z          -65.587             26.160        645.848
                     2022-04-25T04:20:04.766Z          -15.714             41.370        456.123
                     2022-04-25T04:21:04.766Z           39.601             28.188        611.270
                     2022-04-25T04:22:04.766Z           57.500             14.959        955.584
                     2022-04-25T04:22:11.739Z           58.647             13.879       1000.000
    Global Statistics
    -----------------
    Min Elevation    2022-04-25T04:22:11.739Z           58.647             13.879       1000.000
    Max Elevation    2022-04-25T04:20:08.304Z          -11.319             41.458        455.431
    Min Range        2022-04-25T04:20:08.244Z          -11.394             41.458        455.431
    Max Range        2022-04-25T04:22:11.739Z           58.647             13.879       1000.000

     */

}