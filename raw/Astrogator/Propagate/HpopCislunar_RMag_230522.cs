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
                R Magnitude   320000km
        
            STK 12.8 结果
            Parameter Set Type:  Cartesian                                                                    
             X:   230467.4548942931578495 km              Vx:        0.4061457047445826 km/sec        
             Y:   195041.4146291227370966 km              Vy:        0.6009834764557634 km/sec        
             Z:   106035.8374042364303023 km              Vz:        0.1338892061679367 km/sec  

            与STK结果对比精度: 时间～0.0001s  位置精度～1e-2 m, 速度精度～1e-7 m/s
       
            20230522    初次创建
            20230607    修改了RKF78积分器的设置，提高了精度
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
                        精度有待提升
            20251210    由于EOP-V1.1.txt更新了，所以重新使用STK12.8计算了标准值，精度不变
        */
        [TestMethod()]
        public void HpopCislunar_RMag_230522()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "HpopCislunar_RMag_230522.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果,
            int id = output.Position.cartesianVelocity.Length;
            double tolR = 1e-2;
            double tolV = 1e-7;
            var rv = output.Position.cartesianVelocity;
            Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
            Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
            Assert.AreEqual(206061.570, output.Position.cartesianVelocity[id - 7], 0.001);
            Assert.AreEqual(230467454.8942931578495, rv[id - 6], tolR);
            Assert.AreEqual(195041414.6291227370966, rv[id - 5], tolR);
            Assert.AreEqual(106035837.4042364303023, rv[id - 4], tolR);
            Assert.AreEqual(406.1457047445826, rv[id - 3], tolV);
            Assert.AreEqual(600.9834764557634, rv[id - 2], tolV);
            Assert.AreEqual(133.8892061679367, rv[id - 1], tolV);
        }
    }
}