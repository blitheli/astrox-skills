using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.OrbitCore;
using ASTROX.Coordinates;
using ASTROX.Extended;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator
                StoppingCondition:  Duration

                MCS:
                >   Initial_State   地球惯性系Cartesian
                >   Propagate       Earth_point_mass积分器

            初始轨道：[ 6678137, 0, 0, 0, 6789.5303002727, 3686.4141744009 ]
            初始轨道根数：  
                半长径：    6678137 (m)
                偏心率：    0
                轨道倾角：  28.5 (deg)
                近点角距：  0    (deg) 
                升交点经度：0    (deg) 
                平近点角：  0    (deg)

       
            与STK结果对比:   10位有效位数

            使用理论值进行对比，也是差不多的精度

            20240730    初次创建
            20250309    使用理论值进行对比
        */
        [TestMethod()]
        public void EarthPointMass_240730()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_240730.json");
                                    
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  理论值
            double d2r = Math.PI / 180;
            var elm0 = new KeplerianElements(6678137, 0, 28.5 * d2r, 0, 0, 0, 3.986004415e14);
            var elmt = elm0.GetElementsAfterDt(86400);
            var rvt = elmt.ToCartesian();
            Console.WriteLine("理论RV:");
            Console.WriteLine($"X: {rvt.Value.X} Y: {rvt.Value.Y} Z: {rvt.Value.Z}");
            Console.WriteLine($"Vx: {rvt.FirstDerivative.X} Vy: {rvt.FirstDerivative.Y} Vz: {rvt.FirstDerivative.Z}");

            /*
                STK结果
                Parameter Set Type: Cartesian
                X:  5596.6457930237393157 km Vx:        4.2150652858600894 km / sec
                Y: -3201.9674254036199272 km Vy:        5.6899994979573716 km / sec
                Z: -1738.5264636795045590 km Vz:        3.0894176583557909 km / sec
            */

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            //  第多个数值，(与STK中的时间一致)
            int id = output.Position.cartesianVelocity.Length;
            var rv = output.Position.cartesianVelocity;
            //  最终状态
            Console.WriteLine(rv[id - 7]);
            Console.WriteLine(rv[id - 6]);
            Console.WriteLine(rv[id - 5]);
            Console.WriteLine(rv[id - 4]);
            Console.WriteLine(rv[id - 3]);
            Console.WriteLine(rv[id - 2]);
            Console.WriteLine(rv[id - 1]);
            Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(5596.6457930237393157, output.Position.cartesianVelocity[id - 6] * 0.001, 1e-6);
            Assert.AreEqual(-3201.9674254036199272, output.Position.cartesianVelocity[id - 5] * 0.001, 1e-6);
            Assert.AreEqual(-1738.5264636795045590, output.Position.cartesianVelocity[id - 4] * 0.001, 1e-6);
            Assert.AreEqual(4.2150652858600894, output.Position.cartesianVelocity[id - 3] * 0.001, 1e-9);
            Assert.AreEqual(5.6899994979573716, output.Position.cartesianVelocity[id - 2] * 0.001, 1e-9);
            Assert.AreEqual(3.0894176583557909, output.Position.cartesianVelocity[id - 1] * 0.001, 1e-9);
            /*
               理论RV:
                X: 5596645.793261581 Y: -3201967.425075175 Z: -1738526.4635011759
                Vx: 4215.065285432585 Vy: 5689.999498204895 Vz: 3089.4176584901934
                86400
                5596645.793154927
                -3201967.425222474
                -1738526.463581147
                4215.065285624325
                5689.999498093885
                3089.417658429911
            */

        }
    }
}