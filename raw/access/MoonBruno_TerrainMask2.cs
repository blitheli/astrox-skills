using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;

public partial class AccessComputeV2Tests
{
    /*  测试  AccessComputeV2函数，
        
        月球地面站考虑地形遮罩约束
     
        地面站:   sitePosition    [  102.91745, 35.911758, -2252.895 ]
                    布鲁诺坑，坑内
        卫星:     TwoBody     [ 2037400, 0, 45, 0, 90, 0 ]

        地面站地形遮罩约束:  MoonV14全月地形,Bruno坑附近半径约15km
        采用的遮罩数据源:  AzElMask_moonBruno_Simple.cs
            {
              "Text": "MoonV14全月地形,Bruno坑附近半径约15km",
              "TerrainMask": true,
              "TerrainMaskPara": {
                "TerrainServerUrl": "http://astrox.cn:8765/TerrainDb/v1/tilesets/moonV14/tiles/",
                "StepSize": 30,
                "MaxSearchRange": 15
              }
            }

        输入文件:  moonBruno_TerrainMask2.json                     

        20220629    初次创建
        20230824    更改为ComputeV2计算模型
     */

    [TestMethod()]
    public void MoonBruno_TerrainMask2()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "MoonBruno_TerrainMask2.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "MoonBruno_TerrainMask2_out.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        if(!output.IsSuccess)
        {
            Console.WriteLine(output.Message);
            Assert.Fail();
        }
        Console.WriteLine(output.ToString());

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(2, output.Passes.Length);
        Assert.AreEqual(output.Passes[0].Duration, 740.60, 0.1);
        Assert.AreEqual(output.Passes[1].Duration, 726.20, 0.1);
    }

    /**
     持续时间: 183 毫秒

    标准输出: 
    计算完成，结果写入到文件！

    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
    ------    ------------------------    ------------------------    --------------
         1    2022-04-25T04:46:06.270Z    2022-04-25T04:58:26.871Z           740.600
         2    2022-04-25T07:04:06.887Z    2022-04-25T07:16:13.090Z           726.203

     */
}