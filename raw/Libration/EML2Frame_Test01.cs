using Microsoft.VisualStudio.TestTools.UnitTesting;
using AeroSpace.Libration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTROX.Celestial;
using ASTROX.Coordinates;
using ASTROX.Geometry;
using ASTROX;
using ASTROX.Time;
using AeroSpace.Celestial;

namespace AeroSpace.Libration.Tests
{
    
    public partial class LibrationBaseTests
    {
        //  地月系L2点坐标系测试
        //  输入某历元下的航天器的位置速度，并转换至L2点坐标系下
        //  标准值由stk转换提供: 20280801 04:00:00.000  L2点坐标系下的位置速度为
        //  (9000000,0,0,0,-170,0)
        //  国际单位制
        //  20221019 金杨
        [TestMethod()]     
        public void EML2Frame_Test01()
        {
            //调用JplDE430精密历表
            //PlanetsEphemeris.UseJplDe430File();

            //设置地球中心天体
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            
            //调用地月系L2点坐标系
            ReferenceFrame EML2 = LibrationBase.GetEML2Frame();

            //设置转换时刻epoch
            JulianDate epoch = new JulianDate(new GregorianDate(2028, 8, 1, 4, 0, 0));

            Console.WriteLine("JulianDate:{0}",epoch.TotalDays);
            
            //  获取地心惯性系到地月L2坐标系的转换
            ReferenceFrameEvaluator EvaluatorEarth2EML2 = GeometryTransformer.GetReferenceFrameTransformation(earth.InertialFrame, EML2);

            //  获取转换时刻的坐标系转换矩阵
            KinematicTransformation e2L2sys = EvaluatorEarth2EML2.Evaluate(epoch, 1);

            //  输入测试参数(地心惯性系下的位置速度)
            //  由stk转换提供
            Cartesian rp = new Cartesian(-69624441.0859389172401, -411232565.5885090236552, -202553616.1044777545612);
            Cartesian vp = new Cartesian(1008.3375566852790, -223.1288320750045, -11.4342984213928);

            //  将测试参数转换至地月L2点坐标系
            Motion<Cartesian> rv_emL2 = e2L2sys.Transform(new Motion<Cartesian>(rp, vp));

            //   输出地月L2点坐标系下的位置速度
            Console.WriteLine("Pos in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL2.Value[0], rv_emL2.Value[1], rv_emL2.Value[2]);
            Console.WriteLine("Vec in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL2.FirstDerivative[0], rv_emL2.FirstDerivative[1], rv_emL2.FirstDerivative[2]);

            //   与预设的标准值比对
            Assert.AreEqual(9000000.0, rv_emL2.Value[0], 5);
            Assert.AreEqual(0.0, rv_emL2.Value[1], 5);
            Assert.AreEqual(0.0, rv_emL2.Value[2], 5);

            Assert.AreEqual(0.0, rv_emL2.FirstDerivative[0], 0.001);
            Assert.AreEqual(-170.0, rv_emL2.FirstDerivative[1], 0.001);
            Assert.AreEqual(0.0, rv_emL2.FirstDerivative[2], 0.001);
            
                       
            Console.WriteLine("测试完毕！");
        }

    

        
    }
}