using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{

    //  Follow段的测试，验证设置XYZ偏移量(各1000m)时的跟随段行为
    //  基于SpecifyCondition_260414算例，增加XOffset=1000, YOffset=1000, ZOffset=1000
    //  20260415    初次创建
    [TestMethod()]
    public void SpecifyConditionWithOffset_260415()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");
        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "SpecifyCondition_260414.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  设置开始的x,y,z offset
        var seg0 = input.MainSequence[0] as AgVAMCSFollow;
        seg0.XOffset = 1000;
        seg0.YOffset = 1000;
        seg0.ZOffset = 1000;

        //  调用RunMCS
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);


        /*  远地点处结果
        
        STK 计算结果,注意，没使用Follow段，使用的是Initial State段，设置为Leader的VVLH坐标系状态
        X=Y=Z=1km, Vx=Vy=Vz=0.001km/s

        UTC Gregorian Date: 1 Dec 2018 12:56:33.882  UTC Julian Date: 2458454.03928105                            
        Julian Ephemeris Date: 2458454.04008179                                                                   
        Time past epoch: -2.24953e+08 sec   (Epoch in UTC Gregorian Date: 17 Jan 2026 04:00:00.000)               
                                                                                                          
        State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                          
        Parameter Set Type:  Cartesian                                                                            
                 X:    -8448.7940687635400536 km              Vx:       -1.5791176948161867 km/sec                
                 Y:     1924.4723103101910056 km              Vy:       -5.3524649841396945 km/sec                
                 Z:     1046.3906293822085445 km              Vz:       -2.9061513702540158 km/sec                     
                                                                                                           
       */
        Console.WriteLine(output.MainSequenceResults[2].FinalState.Epoch);
        var rv2 = output.MainSequenceResults[2].FinalState.Cartesian;
        Assert.AreEqual(-8448.7940687635400536, rv2.X * 0.001, 1e-5);
        Assert.AreEqual(1924.4723103101910056, rv2.Y * 0.001, 1e-5);
        Assert.AreEqual(1046.3906293822085445, rv2.Z * 0.001, 1e-5);
        Assert.AreEqual(-1.5791176948161867, rv2.Vx * 0.001, 1e-8);
        Assert.AreEqual(-5.3524649841396945, rv2.Vy * 0.001, 1e-8);
        Assert.AreEqual(-2.9061513702540158, rv2.Vz * 0.001, 1e-8);
    }
}
