using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试 火箭传感器 - SGP4 Access,  地球遮挡+传感器约束
     *  
            函数: AccessComputeV2

        火箭: 通过方案弹道生成
            Position: 1s 间隔的火箭位置，速度
            Orientation: 1s 间隔的火箭姿态(四元素)

        卫星: SGP4 积分器

        一开始，zpc输出姿态时,最后一个点的时间输出错了，且固定为5s间隔，导致传感器的
        AvailableIntervals不正确，导致Access计算错误。

    
        20250729    初次编写
     */

    [TestMethod()]
    public void Rocket_SGP4_recSensor_250727()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Rocket_SGP4_recSensor_250727.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出结果到控制台
        Console.Write(DateTime.Now.ToString());
        Console.WriteLine(output.ToString());

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        double ebsl = 0.01;
        Assert.AreEqual(677.161, output.Passes[0].Duration, ebsl);
    }

    /*
   持续时间: 917 毫秒

  标准输出: 
2025/7/29 23:21:36
Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2025-07-21T00:01:56.439Z    2025-07-21T00:13:13.599Z           677.161 
     */

}