using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
         测试 Astrogator 
            积分终止条件: 相对运动停止条件
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **Initial_State（初始状态）**
            初始位置，速度：相对“目标”星的VVLH系

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：自定义变量: "目标"星的VVLH系的Z轴分量为0
            停止次数:2
            停止类型:ThresholdDecreasing

        结果:
            停止次数 1和2时都一样
            主要是因为 一开始就处于穿越

        */
        [TestMethod()]
        public void StoppingConditionsVVLH_Z_250725()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_VVLH_Z_250725.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            var seg1 = output.MainSequenceResults[1];
            Console.WriteLine("递推段结束时刻: {0}", seg1.FinalState.Epoch);

            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output, 
                new JsonSerializerOptions { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));


         
            //var seg0 = seg.SegmentResults[0];
            //  初始段
            //Assert.AreEqual(9686497.505536763, seg0.InitialState.Keplerian.SemiMajorAxis, 1e-6);
            //Assert.AreEqual(0.2600177958875259, seg0.InitialState.Keplerian.Eccentricity, 1e-9);
            //Assert.AreEqual(0, seg0.InitialState.Keplerian.Inclination, 1e-9);
            //Assert.AreEqual(0, seg0.InitialState.Keplerian.RAAN, 1e-9);
            //Assert.AreEqual(299.730955671197, seg0.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);
            //Assert.AreEqual(60.26904432880302, seg0.InitialState.Keplerian.TrueAnomaly, 1e-9);

        /*

          STK 结果

          UTC Gregorian Date: 2 Jan 2000 11:46:04.314  UTC Julian Date: 2451545.99032771    
            
          Parameter Set Type:  Keplerian                                                                            
        sma:     9686.4975055543509370 km            RAAN:                         0 deg                   
        ecc:        0.2600177958895182                  w:         299.7309556709145 deg                   
        inc:                         0 deg             TA:          72.5444862406426 deg   
        */
            //  最后一段
            //var seg3 = seg.SegmentResults[1];
            //Assert.AreEqual("2000-01-02T11:46:04", seg3.FinalState.Epoch.Split('.')[0]);
            //Assert.AreEqual(9686497.50554038, seg3.FinalState.Keplerian.SemiMajorAxis, 1e-4);
            //Assert.AreEqual(0.2600177958876763, seg3.FinalState.Keplerian.Eccentricity, 1e-9);
            //Assert.AreEqual(299.73095567093924, seg3.FinalState.Keplerian.ArgOfPeriapsis, 1e-9);
            //Assert.AreEqual(72.54448622795992, seg3.FinalState.Keplerian.TrueAnomaly, 1e-6);
        }

    }
}