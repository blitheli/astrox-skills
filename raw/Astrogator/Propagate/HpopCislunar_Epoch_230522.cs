using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator CirLunar
                MCS:
                >   InitialState   地球惯性系 Cartesian轨道参数
                >   Propagate       CisLunar

            地月转移轨道，从近地到近月200km高度, 充分验证月球的摄动效果

            "OrbitEpoch": "2022-06-20T04:00:00.000Z",
            "CoordType": "Cartesian",
       
            力学模型：
            中心天体：     地球
            非球形引力位：
                degree：   8
                order      8
                引力文件： WGS84.grv
                EOP文件：  EOP-v1.1.txt 
                固体潮：   None

            第三体引力：
                第三体列表：
                           月球
                           太阳
                星表：     JplDE430
                采用输入的引力常数(取自Stk 12.2, .cb文件)
                "Mu": [ 4.90280030555540e+012, 1.3271244004193938E20 ],
        
            终止条件:   
                Epoch:  2022-06-24T20:00:00.000Z
        
            STK 12.8 结果
            Parameter Set Type:  Cartesian                                                                    
         X:   278356.5758917277562432 km              Vx:        0.8104979795464430 km/sec        
         Y:   265386.3022551920730621 km              Vy:       -1.0785458856431074 km/sec        
         Z:   109487.3524093937157886 km              Vz:        1.1971572280172103 km/sec  

            与STK结果对比精度: 位置精度～3e-3 m, 速度精度～3e-6 m/s， 10^-11的精度
       
            20230522    初次创建
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
                        精度有待提升
            20240730    输入json中，终止条件增加了Duration类型，但是Active为false，用于测试!
            20251210    由于EOP-V1.1.txt更新了，所以重新使用STK12.8计算了标准值，精度不变
        */
        [TestMethod()]
        public void HpopCislunar_Epoch_230522()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "HpopCislunar_Epoch_230522.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi

            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 403200s
            int id = output.Position.cartesianVelocity.Length;
            var rv = output.Position.cartesianVelocity;
            Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
            Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
            Assert.AreEqual(278356575.8917277562432, rv[id - 6], 3e-3);
            Assert.AreEqual(265386302.2551920730621, rv[id - 5], 3e-3);
            Assert.AreEqual(109487352.4093937157886, rv[id - 4], 3e-3);
            Assert.AreEqual(810.4979795464430, rv[id - 3], 2e-6);
            Assert.AreEqual(-1078.5458856431074, rv[id - 2], 2e-6);
            Assert.AreEqual(1197.1572280172103, rv[id - 1], 2e-6);         
        }
        /*
            278356575.88996965  265386302.2566354  109487352.40791528
            810.4979806381554  -1078.5458849010936  1197.157226893749
        */
    }
}