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
                                   
            与STK结果对比精度: 位置精度为～1e-5 m，速度精度～1e-8 m/s
            

            20220808    liyunfei
            20230416    由HPOPPropagator的最新参数，重新改名
            20231212 Propagator中的引力模型更改为统一的接口，使用属性GravityModel，json输入文件中使用$type字段
         */
        [TestMethod()]
        public void GetHPOP_Moon_3rd()
        {
            
            //  输入json文件的路径
            //string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName;
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Propagators/HPOP");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "GetHPOP_Moon_3rd.json");

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
            double ebslR = 1e-5;
            double ebslV = 1e-8;
            Assert.AreEqual(236271.0603227338311, x, ebslR);
            Assert.AreEqual(-1740980.3540612056167, y, ebslR);
            Assert.AreEqual(-946324.7799342864255, z, ebslR);
            Assert.AreEqual(1558.0176608203351, Vx, ebslV);
            Assert.AreEqual(159.6496561421778, Vy, ebslV);
            Assert.AreEqual(94.7554627538331, Vz, ebslV);
        }
    }
}