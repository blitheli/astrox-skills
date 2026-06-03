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

            StoppingCondition: 其它对象的LVLH坐标 XY平面穿越
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

            Initial_State（初始状态）
                - 初始化航天器在地心惯性坐标系中的位置和速度
          
            Propagate
                - 使用地球点质量模型传播
                - 停止条件：XYPlane( 另一个对象 卫星1 LVLH坐标系的XY平面)
                
        与STK对比
            停止时长 < 0.001s   相对XYZ: < 0.001m

            TBD： 停止条件的tolerance设置的为0.001,但是目前Z的数值为0.08m，不符合啊！待排查！

        */
        [TestMethod()]
        public void EarthPointMass_CrossXYPlane_250619()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_CrossXYPlane_250619.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output, 
                new JsonSerializerOptions { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));


            /*  STK 结果  Propagate段末的 相对XYZ以及段时长
             
                X =     -303.0103108177576132 km                                                                                      
                Y =       89.5056873172535603 km                                                                                      
                Z =        0.0000000156862661 km                                                                                      
                Duration =     1686.8798716501678427 sec 
             */

            var seg1 = output.MainSequenceResults[1];
            double ebsl = 0.001;    //1m
            Assert.AreEqual(-303.0103108177576132, (double)seg1.Results["X"] * 0.001, ebsl);
            Assert.AreEqual(89.5056873172535603, (double)seg1.Results["Y"] * 0.001, ebsl);
            Assert.AreEqual(0.0000000156862661, (double)seg1.Results["Z"] * 0.001, ebsl);
            Assert.AreEqual(1686.8798716501678427, seg1.DurationSec, ebsl);
        }

        /*
            我的结果
            "Results": {
                "X": -303010.3982688357,
                "Y": 89505.7107159186,
                "Z": 0.08850027307198616,
                "Duration": 1686.8798235666109
         */
    }
}