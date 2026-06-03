
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class ManeuverImpulsiveTests
    {
        /*
         测试 Astrogator，Target, 脉冲机动，推力方向为：Thrust Vector: VNC, 类型

            GTO -> GEO 多次入轨过程， 考虑相对目标卫星的VNC坐标系

            最后一段Target，进入到目标星后方10m。 VNC的三个分量脉冲已有初值。

            微分修正默认采用多线程，经测试，若CPU为2核，则优化不成功，4核以上可成功，仅2次迭代即可！


            20250625    初次编写
        */
        [TestMethod()]
        public void ImpulsiveMnvr_ThrustVector_GEO_250624()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "ImpulsiveMnvr_ThrustVector_GEO_250624.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);
            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
        }
    }
}