using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-卫星 Access,  地球遮挡+传感器约束+距离约束
            函数: AccessComputeV2
            卫星的历元参数采用J2形式           
            传感器采用锥形传感器模型+VVLH姿态+传感器指向参数AzEl
        
        STK 场景: AccessTest
            facility:       Fac2
            satellite:      SateJ2/conicSensor2,VVLH姿态
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
        20260103    姿态使用VVLH+传感器指向参数AzEl(原来为四元数插值)
     */

    [TestMethod()]
    public void Fac2_J2conicSensor2_20260103()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Fac2_J2conicSensor2_20260103.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);

        //  与STK对比
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(5, output.Passes.Length);
        double ebsl = 0.01;
        Assert.AreEqual(201.355, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(95.549, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(71.787, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(155.018, output.Passes[3].Duration, ebsl); 
        Assert.AreEqual(148.472, output.Passes[4].Duration, ebsl);

        Console.WriteLine(output.ToString());
    }

    /*
   持续时间: 326 毫秒

  标准输出: 

Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2022-04-25T17:44:08.905Z    2022-04-25T17:47:30.258Z           201.353
     2    2022-04-25T19:20:27.512Z    2022-04-25T19:22:03.062Z            95.551
     3    2022-04-25T20:56:12.457Z    2022-04-25T20:57:24.240Z            71.782
     4    2022-04-25T22:31:00.248Z    2022-04-25T22:33:35.274Z           155.025
     5    2022-04-26T00:06:15.108Z    2022-04-26T00:08:43.581Z           148.473

     */

}