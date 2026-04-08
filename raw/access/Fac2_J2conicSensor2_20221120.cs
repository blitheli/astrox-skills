using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-卫星 Access,  地球遮挡+传感器约束+距离约束
            函数: AccessComputeV2
            卫星的历元参数采用J2形式           
        
        STK 场景: AccessTest
            facility:       Fac2
            satellite:      SateJ2/conicSensor2,
                            指向: Az: -90deg, El: 40 deg
                            40°半锥角
                            1500km最大距离约束

        Fac2-To-Sensor2
        ---------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 17:44:08.902    25 Apr 2022 17:47:30.257           201.355
                   2    25 Apr 2022 19:20:27.512    25 Apr 2022 19:22:03.061            95.549
                   3    25 Apr 2022 20:56:12.454    25 Apr 2022 20:57:24.241            71.787
                   4    25 Apr 2022 22:31:00.249    25 Apr 2022 22:33:35.267           155.018
                   5    26 Apr 2022 00:06:15.104    26 Apr 2022 00:08:43.576           148.472
                            
        输入文件:  Fac2_J2conicSensor1_20221120.json                        

        测试结果:   <0.2s

        20221120    初次创建
        20230824    更改为ComputeV2计算模型
     */

    [TestMethod()]
    public void Fac2_J2conicSensor2_20221120()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Fac2_J2conicSensor2_20221120.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        File.WriteAllText(Path.Combine(filePath0, "Fac2_J2conicSensor2_20221120_Out.json"), outputStr, Encoding.UTF8);
        Console.WriteLine("计算完成，结果写入到文件！");

        //  检查部分参数的正确性
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(5, output.Passes.Length);
        double ebsl = 0.01;
        Assert.AreEqual(201.354, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(95.550, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(71.785, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(155.024, output.Passes[3].Duration, ebsl); 
        Assert.AreEqual(148.473, output.Passes[4].Duration, ebsl);

        //  控制台输出Access结果
        Console.WriteLine("Access     Start Time(UTCG)        Stop Time(UTCG)          Duration(s)");
        for (int i = 0; i < output.Passes.Length; i++)
        {
            AccessData accessData = output.Passes[i];
            Console.WriteLine($"{i}     {accessData.AccessStart}     {accessData.AccessStop}      {accessData.Duration}");
        }

        Console.WriteLine("第1个弧段的完整数据");
        Console.WriteLine("               Time(UTCG)                 Azimuth(deg)    Elevation(deg)      Range(km)");
        foreach (var item in output.Passes[0].AllDatas)
        {
            Console.WriteLine("               {0}{1,15:F3}{2,15:F3}{3,15:F3}",
                item.Time, item.Azimuth, item.Elevation, item.Range * 0.001);
        }
        Console.WriteLine("");

        var data = output.Passes[0].MaxElevationData;
        Console.WriteLine("MaxElevation   {0}{1,15:F3}{2,15:F3}{3,15:F3}", data.Time, data.Azimuth, data.Elevation, data.Range * 0.001);
        data = output.Passes[0].MinRangeData;
        Console.WriteLine("MinRange       {0}{1,15:F3}{2,15:F3}{3,15:F3}", data.Time, data.Azimuth, data.Elevation, data.Range * 0.001);
    }

    /*
    计算完成，结果写入到文件！
    Access     Start Time(UTCG)        Stop Time(UTCG)          Duration(s)
    0     2022-04-25T17:44:08.905Z     2022-04-25T17:47:30.260Z      201.35462847791132
    1     2022-04-25T19:20:27.512Z     2022-04-25T19:22:03.062Z      95.55050246848623
    2     2022-04-25T20:56:12.456Z     2022-04-25T20:57:24.241Z      71.78539050667678
    3     2022-04-25T22:31:00.250Z     2022-04-25T22:33:35.275Z      155.02419953968638
    4     2022-04-26T00:06:15.107Z     2022-04-26T00:08:43.581Z      148.47376194763638
    第1个弧段的完整数据
                   Time(UTCG)                 Azimuth(deg)    Elevation(deg)      Range(km)
                   2022-04-25T17:44:08.905Z       -157.424         10.292       1151.068
                   2022-04-25T17:45:08.905Z       -178.617         14.988        934.052
                   2022-04-25T17:46:08.905Z        152.594         16.289        886.845
                   2022-04-25T17:47:08.905Z        127.069         12.698       1033.216
                   2022-04-25T17:47:30.260Z        120.262         10.930       1120.691

    MaxElevation   2022-04-25T17:45:53.243Z        160.327         16.490        879.503
    MinRange       2022-04-25T17:45:52.967Z        160.465         16.490        879.501

     */

}