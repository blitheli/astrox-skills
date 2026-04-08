using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;
public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-卫星 Access,  地球遮挡+传感器约束+距离约束
            函数: AccessComputeV2
            卫星的历元参数采用J2形式,多段姿态(VVLH+LVLH)
            传感器采用锥形传感器模型+传感器指向参数AzEl
        
        STK 场景: AccessTest
            facility:       Fac2
            satellite:      SateJ2/conicSensor2
                            传感器指向: Az: -90deg, El: 40 deg
                            圆锥传感器: 40°半锥角
                            约束: 1500km最大距离
                            姿态多段
                                25 Apr 2022 04:00:00.00-25 Apr 2022 20:00:00.00 VVLH
                                25 Apr 2022 20:00:00.00-25 Apr 2022 20:59:00.00 LVLH    
                                25 Apr 2022 20:59:00.00-26 Apr 2022 04:00:00.00 对日  

        STK 结果
        Facility1-To-Sensor2
        --------------------
                  Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
                  ------    ------------------------    ------------------------    --------------
                       1    25 Apr 2022 17:44:08.892    25 Apr 2022 17:47:30.264           201.372
                       2    25 Apr 2022 19:20:27.503    25 Apr 2022 19:22:03.063            95.559
                       3    25 Apr 2022 20:57:41.929    25 Apr 2022 20:58:59.997            78.069
                       4    25 Apr 2022 22:31:34.049    25 Apr 2022 22:34:03.122           149.073
                       5    26 Apr 2022 00:06:15.104    26 Apr 2022 00:08:43.574           148.470
                            
        测试结果:   <0.1s

        20260105    初次创建
     */

    [TestMethod()]
    public void Fac2_J2conicSensor_Composite_20260105()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Fac2_J2conicSensor_Composite_20260105.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);
        Console.WriteLine(output.ToString());

        //  与STK对比
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(5, output.Passes.Length);
        double ebsl = 0.1;
        Assert.AreEqual(201.372, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(95.559, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(78.069, output.Passes[2].Duration, ebsl);
        Assert.AreEqual(149.073, output.Passes[3].Duration, ebsl); 
        Assert.AreEqual(148.470, output.Passes[4].Duration, ebsl);
    }

    /*

    Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2022-04-25T17:44:08.905Z    2022-04-25T17:47:30.258Z           201.353
     2    2022-04-25T19:20:27.512Z    2022-04-25T19:22:03.062Z            95.551
     3    2022-04-25T20:57:41.916Z    2022-04-25T20:58:59.998Z            78.082
     4    2022-04-25T22:31:34.055Z    2022-04-25T22:34:03.121Z           149.066
     5    2022-04-26T00:06:15.108Z    2022-04-26T00:08:43.581Z           148.473

     */

}