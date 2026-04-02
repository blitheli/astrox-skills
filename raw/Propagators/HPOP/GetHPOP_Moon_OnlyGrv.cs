using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AeroSpace.Propagators.Tests
{

    public partial class ProgagatorHpopTests
    {
        /*
            测试 HPOP积分器 GetHPOP
            
            STK 12.2 Moon HPOP Default V10 (引力场更改为GL0900D.grv)
            

            力学模型：
            中心天体：     月球
            非球形引力位：
                degree：   48
                order      48
                引力文件：  GL0900D.grv
                EOP文件：  （无），月固系和月球惯性系转换采用DE430的实现
                固体潮：   Permanant Tides
                       
            与STK结果对比精度: 位置精度为～3e-6 m，速度精度～3e-9 m/s
            

            20220808    liyunfei
            20230416    由HPOPPropagator的最新参数，重新改名
            20231212 Propagator中的引力模型更改为统一的接口，使用属性GravityModel，json输入文件中使用$type字段
         */
        [TestMethod()]
        public void GetHPOP_Moon_OnlyGrv()
        {
            
            //  输入json文件的路径
            //string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Propagators/HPOP");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "GetHPOP_Moon_OnlyGrv.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<HpopInput>(inputStr);

            //  webApi
            var output = PropagatorHPOP.Compute(input);

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            int id = output.Position.cartesianVelocity.Length;
            var x = output.Position.cartesianVelocity[id - 6];
            var y = output.Position.cartesianVelocity[id - 5];
            var z = output.Position.cartesianVelocity[id - 4];
            var Vx = output.Position.cartesianVelocity[id - 3];
            var Vy = output.Position.cartesianVelocity[id - 2];
            var Vz = output.Position.cartesianVelocity[id - 1];
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            double ebslR = 3e-6;
            double ebslV = 3e-9;
            Assert.AreEqual(235243.376256253100, x, ebslR);
            Assert.AreEqual(-1741171.8777891578611, y, ebslR);
            Assert.AreEqual(-946142.4730548269508, z, ebslR);
            Assert.AreEqual(1558.1648530408891, Vx, ebslV);
            Assert.AreEqual(159.0009715585189, Vy, ebslV);
            Assert.AreEqual(94.2911557175238, Vz, ebslV);
        }
    }
}