using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.Celestial;
using AeroSpace.IO;
using Newtonsoft.Json.Bson;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{

    //  Follow段的测试，验证默认的Joining和Separation设置(Leader的开始和结束)
    //  Leader使用SimpleAscent积分器
    //  20260420    初次创建
    [TestMethod()]
    public void FollowSimpleAscent_260420()
    {
        string eopFp = Path.Combine(DataPaths.DGLPath, "EOP-v1.1.txt");
        PlanetsEphemeris.UseEarthEOPFile(eopFp);

        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");
        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "FollowSimpleAscent_260420.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        /*  Follow段的结束状态
            STK 结果 
            State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
            Parameter Set Type:  Cartesian                                                                 
             X:    -1899.7081940950049557 km              Vx:        7.2717261432161520 km/sec     
             Y:    -5745.5242232813161536 km              Vy:       -3.2556206076191501 km/sec     
             Z:     2815.4430115145032687 km              Vz:       -1.7372361306495940 km/sec 

        State Vector in Coordinate System: Earth Fixed                                                 
            Parameter Set Type:  Cartesian                                                                 
             X:     3782.7019905658339667 km              Vx:        6.3463998047819707 km/sec     
             Y:    -4725.9773029793741443 km              Vy:        4.0555709413264998 km/sec     
             Z:     2811.2209725318680285 km              Vz:       -1.7216728967290749 km/sec 
        */
        double ebsl6 = 1e-6;
        Assert.AreEqual(600.0, output.MainSequenceResults[0].DurationSec, ebsl6);
        var rltSeg0 = output.MainSequenceResults[0];
        Assert.AreEqual(-1899.7081940950049557, rltSeg0.FinalState.Cartesian.X * 0.001, ebsl6);
        Assert.AreEqual(-5745.5242232813161536, rltSeg0.FinalState.Cartesian.Y * 0.001, ebsl6);
        Assert.AreEqual(2815.4430115145032687, rltSeg0.FinalState.Cartesian.Z * 0.001, ebsl6);
        Assert.AreEqual(7.2717261432161520, rltSeg0.FinalState.Cartesian.Vx * 0.001, 1e-5);
        Assert.AreEqual(-3.2556206076191501, rltSeg0.FinalState.Cartesian.Vy * 0.001, 1e-5);
        Assert.AreEqual(-1.7372361306495940, rltSeg0.FinalState.Cartesian.Vz * 0.001, 3e-5);

        /*  远地点处结果
            STK结果 in Coordinate System: Earth Inertial    
                                                                                                          
        Parameter Set Type:  Cartesian                                                                            
                 X:     3194.3530571184924156 km              Vx:       -4.5897210860175575 km/sec                
                 Y:     9661.0800139726743510 km              Vy:        2.0548615634464902 km/sec                
                 Z:    -4734.1581294214720401 km              Vz:        1.0964974674420571 km/sec            
         */

        var rltSeg2 = output.MainSequenceResults[2];
        Assert.AreEqual(3194.3530571184924156, rltSeg2.FinalState.Cartesian.X * 0.001, 2e-4);
        Assert.AreEqual(9661.0800139726743510, rltSeg2.FinalState.Cartesian.Y * 0.001, 2e-4);
        Assert.AreEqual(-4734.1581294214720401, rltSeg2.FinalState.Cartesian.Z * 0.001, 2e-4);
        Assert.AreEqual(-4.5897210860175575, rltSeg2.FinalState.Cartesian.Vx * 0.001, 1e-7);
        Assert.AreEqual(2.0548615634464902, rltSeg2.FinalState.Cartesian.Vy * 0.001, 1e-7);
        Assert.AreEqual(1.0964974674420571, rltSeg2.FinalState.Cartesian.Vz * 0.001, 3e-7);
    }

}
