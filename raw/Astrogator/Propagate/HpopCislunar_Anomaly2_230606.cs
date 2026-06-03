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

            地月转移轨道，从近地到地心坐标下真近点角178.7 deg

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
                True Anomaly    180 deg
                    Central Body:
                                  Earth
                    CoordSystemName:
                                  Earth Inertial
                    Mu:
                                  3.986004415E14
            STK 12.8 结果
            Parameter Set Type:  Cartesian                                                                                                                                    
             X:   106225.8967296962946421 km              Vx:       -0.2444111945184534 km/sec        
             Y:   116305.5704185548383975 km              Vy:        0.3001625685837035 km/sec        
             Z:    44526.7598027418716811 km              Vz:       -0.2009528517985332 km/sec   

            与STK结果对比精度: 时间～5e-4 s, 位置精度～0.5 m, 速度精度～1e-5 m/s                        
            20230606    李云飞，增加AngularSetting设置           
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器     
            20250415    不再需要AngularSetting，加入Dimension:Angle
            20251210    由于EOP-V1.1.txt更新了，所以重新使用STK12.8计算了标准值，精度不变
        */
        [TestMethod()]
        public void HpopCislunar_Anomaly2_230606()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "HpopCislunar_Anomaly2_230606.json");
            
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            int id = output.Position.cartesianVelocity.Length;
            var rv = output.Position.cartesianVelocity;
            Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
            Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
            Assert.AreEqual(123711.1241, rv[id - 7], 0.001);
            Assert.AreEqual(106225896.7296962946421, rv[id - 6], 0.5);
            Assert.AreEqual(116305570.4185548383975, rv[id - 5], 0.5);
            Assert.AreEqual(44526759.8027418716811, rv[id - 4], 0.5);
            Assert.AreEqual(-244.4111945184534, rv[id - 3], 1e-5);
            Assert.AreEqual(300.1625685837035, rv[id - 2], 1e-5);
            Assert.AreEqual(-200.9528517985332, rv[id - 1], 1e-5);
            Assert.IsTrue(output.IsSuccess);
        }
    }
}