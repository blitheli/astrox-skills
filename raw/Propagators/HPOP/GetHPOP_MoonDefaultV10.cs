using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    public partial class ProgagatorHpopTests
    {
        /*
            测试 HPOP积分器 GetHPOP
            
            STK 12.2 Moon HPOP Default V10 (引力场更改为GL0900D.grv，因为里面为DE430,和本程序一致)

            力学模型：
            中心天体：     月球
            非球形引力位：
                degree：   48
                order      48
                引力文件：  GL0900D.grv
                EOP文件：  （无），月固系和月球惯性系转换采用DE430的实现
                固体潮：   Permanant Tides

            第三体引力：
                第三体列表：
                           地球
                           太阳
                星表：     JplDE430
                    
            大气拖曳力：
                 （无）

            光压摄动：    
                ModelType:          Spherical
                ShadowModel:        Dual Cone
                SunPosition:        Apparent Sun To True Cb
                掩食列表:           地球                
                    
            STK 结果：（采用两种模型，无大气时一样，有大气时有差异）
                -   HPOP模型
                236301.902    -1740976.612    -946322.441           1558.016259            159.664996             94.762683    
                -   Astrogator模型
                Parameter Set Type:  Cartesian                                                                 
                 X:      236.3020910295573742 km              Vx:        1.5580162414561893 km/sec     
                 Y:    -1740.9765901189173292 km              Vy:        0.1596651262577385 km/sec     
                 Z:     -946.3224288088867979 km              Vz:        0.0947627540704217 km/sec     

                两者相差 0.2m
                               
            与STK结果对比精度: 位置精度为～0.1 m，速度精度～1e-4 m/s
                无大气时，精度较高！

            20220808    liyunfei
            20230416    由HPOPPropagator的最新参数，重新改名
            20231129    输入json中接口使用$type字段
            20231212    Propagator中的引力模型更改为统一的接口，使用属性GravityModel，json输入文件中使用$type字段
            20250603    重新测试
         */
        [TestMethod()]
        public void GetHPOP_MoonDefaultV10()
        {
            
            //  输入json文件的路径
            //string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Propagators/HPOP");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "GetHPOP_MoonDefaultV10.json");
                     
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<HpopInput>(inputStr);

            //  webApi
            var output = PropagatorHPOP.Compute(input);
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  标准值为STK 的计算结果, 86400s 的数值
            int id = output.Position.cartesianVelocity.Length;

            Console.WriteLine("最后一点时刻、位置和速度：");
            Console.WriteLine(output.Position.cartesianVelocity[id - 7]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 6]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 5]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 4]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 3]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 2]);
            Console.WriteLine(output.Position.cartesianVelocity[id - 1]);


            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(236302.0910295573742, output.Position.cartesianVelocity[id - 6], 0.1);
            Assert.AreEqual(-1740976.5901189173292, output.Position.cartesianVelocity[id - 5], 0.1);
            Assert.AreEqual(-946322.4288088867979, output.Position.cartesianVelocity[id - 4], 0.1);
            Assert.AreEqual(1558.0162414561893, output.Position.cartesianVelocity[id - 3], 1e-4);
            Assert.AreEqual(159.6651262577385, output.Position.cartesianVelocity[id - 2], 1e-4);
            Assert.AreEqual(94.7627540704217, output.Position.cartesianVelocity[id - 1], 1e-4);

            /*
                 最后一点时刻、位置和速度：
            86400
            236302.02033578526
            -1740976.5974619088
            -946322.4330998895
            1558.0162481233235
            159.6650773030818
            94.76272746454543
             */
        }
    }
}