using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{

    //  Follow段的测试，验证Joining和Separation设置都为指定条件
    //  20260414    初次创建
    [TestMethod()]
    public void Follow_SpecifyJoiningAndSeparationDuration_260414()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");
        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "SpecifyCondition_260414.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        //  Follow段的持续时间为30秒
        Assert.AreEqual(30.0, output.MainSequenceResults[0].DurationSec, 1e-6);

        /*  远地点处结果
        STK 结果
        UTC Gregorian Date: 1 Dec 2018 12:56:37.484  UTC Julian Date: 2458454.03932273                            
        State Vector in Coordinate System: Earth Inertial                                                         
        Parameter Set Type:  Cartesian                                                                            
                 X:    -8458.1401787065442477 km              Vx:       -1.5786809681834553 km/sec                
                 Y:     1928.4136973644499449 km              Vy:       -5.3476879164093409 km/sec                
                 Z:     1047.0432082448498932 km              Vz:       -2.9035576341020479 km/sec            
         */

        var rltSeg2 = output.MainSequenceResults[2];
        Assert.AreEqual(-8458.1401787065442477, rltSeg2.FinalState.Cartesian.X * 0.001, 1e-5);
        Assert.AreEqual(1928.4136973644499449, rltSeg2.FinalState.Cartesian.Y * 0.001, 1e-5);
        Assert.AreEqual(1047.0432082448498932, rltSeg2.FinalState.Cartesian.Z * 0.001, 1e-5);
        Assert.AreEqual(-1.5786809681834553, rltSeg2.FinalState.Cartesian.Vx * 0.001, 1e-8);
        Assert.AreEqual(-5.3476879164093409, rltSeg2.FinalState.Cartesian.Vy * 0.001, 1e-8);
        Assert.AreEqual(-2.9035576341020479, rltSeg2.FinalState.Cartesian.Vz * 0.001, 1e-8);
    }
}
