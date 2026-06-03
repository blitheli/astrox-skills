using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    [TestClass()]
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator
                MCS:
                >   Initial_State   地球惯性系Cartesian
                >   Propagate       Earth_hpop_v10_default积分器

            初始轨道：[ 6678137, 0, 0, 0, 6789.5303002727, 3686.4141744009 ]
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

                大气摄动：    （开启）
                    拖曳系数： 2.2
                    面质比：   0.02

            与STK结果对比:   位置精度为～0.3 m，速度精度～4e-4 m/s

            20230510    初次创建
            20231129    输入json更改为$type属性
            20231212    修改json输入,不再提供Propagators,引用缺省的积分器
            20240226    重新使用STK12生成进行对比，引力场中使用 Use Secular Variations
        */
        [TestMethod()]
        public void EarthDefaultV10_230510()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthDefaultV10_230510.json");
                                    
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
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
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(6431601.3546813346693, output.Position.cartesianVelocity[id - 6], 0.3);
            Assert.AreEqual(-1708003.572159182340, output.Position.cartesianVelocity[id - 5], 0.3);
            Assert.AreEqual(-461571.4288793832338, output.Position.cartesianVelocity[id - 4], 0.3);
            Assert.AreEqual(1995.2026578048985, output.Position.cartesianVelocity[id - 3], 4e-4);
            Assert.AreEqual(6517.6125915100283, output.Position.cartesianVelocity[id - 2], 4e-4);
            Assert.AreEqual(3647.8363106252436, output.Position.cartesianVelocity[id - 1], 4e-4);

        }
    }
}