using Microsoft.VisualStudio.TestTools.UnitTesting;
using AeroSpace.Libration;
using AeroSpace.Celestial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTROX.Celestial;
using ASTROX.Time;
using AeroSpace.Propagator;
using ASTROX.Coordinates;
using AeroSpace.IO;
using AeroSpace.MathLib;
using System.IO;
using ASTROX.Geometry;
using ASTROX;

namespace AeroSpace.Libration.Tests
{

    public partial class LibrationBaseTests
    {
        /*

            地月L2点附近平动点 halo 轨道
            
            轨道构建取不同历元时刻为T0，将所得quasi-Halo轨道转换至地月L2点坐标系的结果
            所取轨道位置均为自T0时刻外推10天（epday=10.0）
            

            20221019   金杨

        */

        [TestMethod()]
        public void EML2_Halo_ECI2L2Frame_test()
        {
            
                

                //  调用JPLDE430精密历表
                PlanetsEphemeris.UseJplDe430File();
                
                //  创建四个不同日期的起始时间
                JulianDate t0 = new JulianDate(new GregorianDate(2028, 8, 1, 4, 0, 0));

                JulianDate t1 = new JulianDate(new GregorianDate(2028, 8, 14, 4, 0, 0));

                JulianDate t2 = new JulianDate(new GregorianDate(2028, 8, 2, 4, 0, 0));

                JulianDate t3 = new JulianDate(new GregorianDate(2028, 2, 1, 4, 0, 0));

                int LI = 2;
                double AZ = 10000/384000;
                double DT = 0.5;
                int nPoints = 20;
            
           
                EarthMoonLibration em0 = new EarthMoonLibration(t0);
                EMHaloOrbitResult halorlt0 = em0.QuasiHaloOrbit(LI, AZ, DT, nPoints);

                EarthMoonLibration em1 = new EarthMoonLibration(t1);
                EMHaloOrbitResult halorlt1 = em1.QuasiHaloOrbit(LI, AZ, DT, nPoints);

                EarthMoonLibration em2 = new EarthMoonLibration(t2);
                EMHaloOrbitResult halorlt2 = em2.QuasiHaloOrbit(LI, AZ, DT, nPoints);

                EarthMoonLibration em3 = new EarthMoonLibration(t3);
                EMHaloOrbitResult halorlt3 = em3.QuasiHaloOrbit(LI, AZ, DT, nPoints);

            //设置地球中心天体
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;

            //调用地月系L2点坐标系
            ReferenceFrame EML2 = LibrationBase.GetEML2Frame();

            //  获取地心惯性系到地月L2坐标系的转换
            ReferenceFrameEvaluator EvaluatorEarth2EML2 = GeometryTransformer.GetReferenceFrameTransformation(earth.InertialFrame, EML2);

            //  获取t0+epday转换时刻的坐标系转换矩阵及Halo轨道位置速度
            KinematicTransformation e2L2sys = EvaluatorEarth2EML2.Evaluate(t0.AddDays(10.0), 1);
            double[] PosVec = LibrationBase.GetHaloResultAtEpoch(halorlt0, 10.0);

            //  输入轨道(地心惯性系下的位置速度)
            Cartesian rp = new Cartesian(PosVec[0], PosVec[1], PosVec[2]);
            Cartesian vp = new Cartesian(PosVec[3], PosVec[4], PosVec[5]);

            //  将测试参数转换至地月L2点坐标系
            Motion<Cartesian> rv_emL2 = e2L2sys.Transform(new Motion<Cartesian>(rp, vp));

            //   输出地月L2点坐标系下的位置速度
            Console.WriteLine("Halo Begin Date: {0}", t0.ToGregorianDate());
            Console.WriteLine("Pos1 in EML2Frame(km): {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL2.Value[0]*0.001, rv_emL2.Value[1]*0.001, rv_emL2.Value[2] * 0.001);
            Console.WriteLine("Vec1 in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL2.FirstDerivative[0], rv_emL2.FirstDerivative[1], rv_emL2.FirstDerivative[2]);

            //  获取t0+epday转换时刻的坐标系转换矩阵及Halo轨道位置速度
            KinematicTransformation e2L2sys1 = EvaluatorEarth2EML2.Evaluate(t1.AddDays(10.0), 1);
            double[] PosVec1 = LibrationBase.GetHaloResultAtEpoch(halorlt1, 10.0);

            //  输入轨道(地心惯性系下的位置速度)
            Cartesian rp1= new Cartesian(PosVec1[0], PosVec1[1], PosVec1[2]);
            Cartesian vp1 = new Cartesian(PosVec1[3], PosVec1[4], PosVec1[5]);

            //  将测试参数转换至地月L2点坐标系
            Motion<Cartesian> rv_emL21 = e2L2sys1.Transform(new Motion<Cartesian>(rp1, vp1));

            //   输出地月L2点坐标系下的位置速度
            Console.WriteLine("Halo Begin Date: {0}", t1.ToGregorianDate());
            Console.WriteLine("Pos2 in EML2Frame(km): {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL21.Value[0]*0.001, rv_emL21.Value[1] * 0.001, rv_emL21.Value[2] * 0.001);
            Console.WriteLine("Vec2 in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL21.FirstDerivative[0], rv_emL21.FirstDerivative[1], rv_emL21.FirstDerivative[2]);

            KinematicTransformation e2L2sys2 = EvaluatorEarth2EML2.Evaluate(t2.AddDays(10.0), 1);
            double[] PosVec2 = LibrationBase.GetHaloResultAtEpoch(halorlt2, 10.0);

            //  输入轨道(地心惯性系下的位置速度)
            Cartesian rp2 = new Cartesian(PosVec2[0], PosVec2[1], PosVec2[2]);
            Cartesian vp2 = new Cartesian(PosVec2[3], PosVec2[4], PosVec2[5]);

            //  将测试参数转换至地月L2点坐标系
            Motion<Cartesian> rv_emL22 = e2L2sys2.Transform(new Motion<Cartesian>(rp2, vp2));

            //   输出地月L2点坐标系下的位置速度
            Console.WriteLine("Halo Begin Date: {0}", t2.ToGregorianDate());
            Console.WriteLine("Pos2 in EML2Frame(km): {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL22.Value[0] * 0.001, rv_emL22.Value[1] * 0.001, rv_emL22.Value[2] * 0.001);
            Console.WriteLine("Vec2 in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL22.FirstDerivative[0], rv_emL22.FirstDerivative[1], rv_emL22.FirstDerivative[2]);

            KinematicTransformation e2L2sys3 = EvaluatorEarth2EML2.Evaluate(t3.AddDays(10.0), 1);
            double[] PosVec3 = LibrationBase.GetHaloResultAtEpoch(halorlt3, 10.0);

            //  输入轨道(地心惯性系下的位置速度)
            Cartesian rp3 = new Cartesian(PosVec3[0], PosVec3[1], PosVec3[2]);
            Cartesian vp3 = new Cartesian(PosVec3[3], PosVec3[4], PosVec3[5]);

            //  将测试参数转换至地月L2点坐标系
            Motion<Cartesian> rv_emL23 = e2L2sys3.Transform(new Motion<Cartesian>(rp3, vp3));

            //   输出地月L2点坐标系下的位置速度
            Console.WriteLine("Halo Begin Date: {0}", t3.ToGregorianDate());
            Console.WriteLine("Pos2 in EML2Frame(km): {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL23.Value[0] * 0.001, rv_emL23.Value[1] * 0.001, rv_emL23.Value[2] * 0.001);
            Console.WriteLine("Vec2 in EML2Frame: {0,15:F8},{1,15:F8},{2,15:F8}", rv_emL23.FirstDerivative[0], rv_emL23.FirstDerivative[1], rv_emL23.FirstDerivative[2]);

            Assert.IsTrue(true);

            }
            
        }

    }
