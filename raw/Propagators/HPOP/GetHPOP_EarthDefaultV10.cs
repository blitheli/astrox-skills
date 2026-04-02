using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    [TestClass()]
    public partial class ProgagatorHpopTests
    {
        /*
            测试 HPOP积分器 GetHPOP
        
        STK 12.2 Earth HPOP Default V10

        初始轨道根数：
            半长径：    6678137 (m)
            偏心率：    0
            轨道倾角：  28.5 (deg)
            近点角距：  0    (deg) 
            升交点经度：0    (deg) 
            平近点角：  0    (deg)

        力学模型：
            中心天体：     地球
            非球形引力位：（开启）
                degree：   21
                order      21
                引力文件： EGM2008.grv
                EOP文件：  EOP-v1.1.txt
                固体潮：  （开启）
                           Permanant Tides

            第三体引力：  （开启）
                第三体列表：
                           月球
                           太阳
                星表：     JplDE430

            光压摄动：    （开启）
                模型：     Spherical
                反射系数： 1.0
                面质比：   0.02
                阴影模型： Dual Cone
                太阳位置:  Apparent
                掩食列表： 地球
                           月球
            拖曳力：      （开启）
                拖曳系数： 2.2
                面质比：   0.02

            输入文件: HPOP_defaultInput.json
                里面的数值取自STK计算
                      
            与STK结果对比:   位置精度为～0.03 m，速度精度～1e-4 m/s

            20220503 Mulin Liu
            20220725 重构了代码，添加了测试样例，测试已通过。
            20230414 由HPOPPropagator的最新参数，重新改名，光压中SunPosition改为Apparent
            20231129 输入json中接口使用$type字段
            20231212 Propagator中的引力模型更改为统一的接口，使用属性GravityModel，json输入文件中使用$type字段
            20240226 重新使用STK12生成进行对比，引力场中使用 Use Secular Variations
            20251210 重新测试，精度提高
         */
        [TestMethod()]
        public void GetHPOP_EarthDefaultV10()
        {
            //  输入json文件的路径
            //string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Propagators/HPOP");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "GetHPOP_EarthDefaultV10.json");
 
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<HpopInput>(inputStr);

            //  webApi
            var output = PropagatorHPOP.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  STK结果
            //  Parameter Set Type:  Cartesian
            //X:     6431.6013546813346693 km Vx:        1.9952026578048985 km / sec
            //Y: -1708.0035721591823403 km Vy:        6.5176125915100283 km / sec
            //Z: -461.5714288793832338 km Vz:        3.6478363106252436 km / sec

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            //  第多个数值，(与STK中的时间一致)
            int id = output.Position.cartesianVelocity.Length;
            var rv = output.Position.cartesianVelocity;
            Console.WriteLine($"{rv[id-6]},  {rv[id-5]}, {rv[id-4]}");
            Console.WriteLine($"{rv[id-3]},  {rv[id-2]}, {rv[id-1]}");
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(6431601.3546813346693, output.Position.cartesianVelocity[id - 6], 0.03);
            Assert.AreEqual(-1708003.572159182340, output.Position.cartesianVelocity[id - 5], 0.03);
            Assert.AreEqual(-461571.4288793832338, output.Position.cartesianVelocity[id - 4], 0.03);
            Assert.AreEqual(1995.2026578048985, output.Position.cartesianVelocity[id - 3], 1e-4);
            Assert.AreEqual(6517.6125915100283, output.Position.cartesianVelocity[id - 2], 1e-4);
            Assert.AreEqual(3647.8363106252436, output.Position.cartesianVelocity[id - 1], 1e-4);
        }
    }
}