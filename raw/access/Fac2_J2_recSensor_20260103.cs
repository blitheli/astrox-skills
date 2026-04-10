using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Access.Tests;


public partial class AccessComputeV2Tests
{
    /*  测试 地球 地面站-卫星 Access,  地球遮挡+传感器约束
            函数: AccessComputeV2
            卫星的历元参数采用J2形式           
            传感器: 矩形传感器,  指向参数为AzEl
        
        STK 场景: AccessTest
            facility:       Fac2
            satellite:      SateJ2/recSensor,   VVLH姿态
                            默认指向: Az: 0deg, El: 90 deg
                            矩形传感器:  xHalfAngle: 30°,yHalfAngle: 60°
            
            Fac2-To-recSensor
            -----------------
              Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
              ------    ------------------------    ------------------------    --------------
                   1    25 Apr 2022 19:20:49.693    25 Apr 2022 19:21:41.522            51.829
                   2    25 Apr 2022 20:56:22.552    25 Apr 2022 20:57:13.859            51.307
                   3    25 Apr 2022 22:31:49.487    25 Apr 2022 22:32:44.559            55.073

            Fac2-To-recSensor - AER reported in the object's default AER frame
            ------------------------------------------------------------------
                                     Time (UTCG)          Azimuth (deg)    Elevation (deg)    Range (km)
                              ------------------------    -------------    ---------------    ----------
                              25 Apr 2022 19:20:49.693          206.305             41.284    448.587141
                              25 Apr 2022 19:21:15.608          172.103             47.159    407.521219
                              25 Apr 2022 19:21:41.522          137.584             41.541    446.750473

            Global Statistics
            -----------------
            Min Elevation     25 Apr 2022 19:20:49.693          206.305             41.284    448.587141
            Max Elevation     25 Apr 2022 19:21:15.949          171.587             47.160    407.515234
            Mean Elevation                                                          43.328              
            Min Range         25 Apr 2022 19:21:15.918          171.634             47.160    407.515173
            Max Range         25 Apr 2022 19:20:49.693          206.305             41.284    448.587141
            Mean Range         

        测试结果:   < 0.01s

        20221230    初次创建    首个时刻点为最小值时未记录
        20230824    更改为ComputeV2
        20260103    姿态使用VVLH+传感器指向参数AzEl(原来为四元数插值)
     */

    [TestMethod()]
    public void Fac2_J2_recSensor_20260103()
    {
        //  输入json文件的路径            
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Access");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Fac2_J2_recSensor_20260103.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AccessInput2>(inputStr);

        //  Access计算
        var output = AccessComputeV2.Compute(input);
     
        //  与STK对比,<0.01s
        Assert.IsTrue(output.IsSuccess);
        Assert.AreEqual(3, output.Passes.Length);
        double ebsl = 0.01;
        Assert.AreEqual(51.829, output.Passes[0].Duration, ebsl);
        Assert.AreEqual(51.307, output.Passes[1].Duration, ebsl);
        Assert.AreEqual(55.073, output.Passes[2].Duration, ebsl);
        //Assert.AreEqual(47.160, output.Passes[0].MaxElevationData.Elevation, ebsl);
        //Assert.AreEqual(407.515, output.Passes[0].MinRangeData.Range * 0.001, 0.02);

        Console.WriteLine(output.ToString());
    }

    /*
   持续时间: 339 毫秒

  标准输出: 

Access        Start Time (UTCG)           Stop Time (UTCG)        Duration (sec)
------    ------------------------    ------------------------    --------------
     1    2022-04-25T19:20:49.693Z    2022-04-25T19:21:41.521Z            51.828
     2    2022-04-25T20:56:22.552Z    2022-04-25T20:57:13.862Z            51.310
     3    2022-04-25T22:31:49.491Z    2022-04-25T22:32:44.562Z            55.071
  
     */

}