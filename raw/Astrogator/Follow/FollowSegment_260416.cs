using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{

    //  Follow段的测试，验证默认的Joining和Separation设置(Leader的开始和结束)
    //  Leader使用TwoBody积分器
    //  20260416    初次创建
    [TestMethod()]
    public void Follow_DefaultJoiningAndSeparation_260416()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");
        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "FollowSegment_260416.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        /*  Follow段的结束状态(二体轨道,有理论值),和Leader的段末应该一样
            理论RV:
            X: 5596645.793261581 Y: -3201967.425075175 Z: -1738526.4635011759
            Vx: 4215.065285432585 Vy: 5689.999498204895 Vz: 3089.4176584901934
        */
        double ebsl6 = 1e-6;
        Assert.AreEqual(86400.0, output.MainSequenceResults[0].DurationSec, ebsl6);
        var rltSeg0 = output.MainSequenceResults[0];
        Assert.AreEqual(5596.6457930237393157, rltSeg0.FinalState.Cartesian.X * 0.001, ebsl6);
        Assert.AreEqual(-3201.9674254036199272, rltSeg0.FinalState.Cartesian.Y * 0.001, ebsl6);
        Assert.AreEqual(-1738.5264636795045590, rltSeg0.FinalState.Cartesian.Z * 0.001, ebsl6);
        Assert.AreEqual(4.2150652858600894, rltSeg0.FinalState.Cartesian.Vx * 0.001, 1e-9);
        Assert.AreEqual(5.6899994979573716, rltSeg0.FinalState.Cartesian.Vy * 0.001, 1e-9);
        Assert.AreEqual(3.0894176583557909, rltSeg0.FinalState.Cartesian.Vz * 0.001, 1e-9);

        /*  远地点处结果
            STK结果 in Coordinate System: Earth Inertial                                                                                                                                                                   
            UTC Gregorian Date: 2 Dec 2018 00:56:07.483  UTC Julian Date: 2458454.5389755                                                                                                                                                   
            State Vector in Coordinate System: Earth Inertial                                                        
            Parameter Set Type:  Cartesian                                                                            
             X:    -7323.0474341896388069 km              Vx:       -3.4298497903881304 km/sec                
             Y:     4189.6807833724760712 km              Vy:       -4.6300216651657111 km/sec                
             Z:     2274.8110609977093191 km              Vz:       -2.5138966525511690 km/sec             
         */

        var rltSeg2 = output.MainSequenceResults[2];
        Assert.AreEqual(-7323.0474341896388069, rltSeg2.FinalState.Cartesian.X * 0.001, ebsl6);
        Assert.AreEqual(4189.6807833724760712, rltSeg2.FinalState.Cartesian.Y * 0.001, ebsl6);
        Assert.AreEqual(2274.8110609977093191, rltSeg2.FinalState.Cartesian.Z * 0.001, ebsl6);
        Assert.AreEqual(-3.4298497903881304, rltSeg2.FinalState.Cartesian.Vx * 0.001, 1e-9);
        Assert.AreEqual(-4.6300216651657111, rltSeg2.FinalState.Cartesian.Vy * 0.001, 1e-9);
        Assert.AreEqual(-2.5138966525511690, rltSeg2.FinalState.Cartesian.Vz * 0.001, 1e-9);
    }

}
