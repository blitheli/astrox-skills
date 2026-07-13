using Microsoft.VisualStudio.TestTools.UnitTesting;
using AeroSpace.Libration;
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

namespace AeroSpace.Libration.Tests
{

    public partial class LibrationBaseTests
    {
        /*

            地月L2点附近平动点 halo 轨道

            T0: 2020-01-01T00:00:00.000
            约3个月

            20220623    初次编写

        */

        [TestMethod()]
        public void EM_L1_Halo_0623()
        {
            try
            {
                double d2r = Math.PI / 180.0;

                //  采用DE405数据，与侯锡云程序一致            
                //JplDE405 jplde = new JplDE405(Path.Combine(DataPaths.DataPath, "JPLEPH"));
                JplDE430 jplde = new JplDE430(Path.Combine(DataPaths.DataPath, "plneph.430"));
                CentralBodiesFacet centralBodies = CentralBodiesFacet.GetFromContext();
                jplde.UseForCentralBodyPositions(centralBodies);

                JulianDate t0 = new JulianDate(new GregorianDate(2020, 1, 1, 0, 0, 0));

                int LI = 1;
                double AZ = 0.05;
                double DT = 0.919883267488107;
                int nPoints = 25;

                EarthMoonLibration em = new EarthMoonLibration(t0);
                EMHaloOrbitResult rlt = em.QuasiHaloOrbit(LI, AZ, DT, nPoints);
                               
                string cd = System.Environment.CurrentDirectory;              
                string fileName = "地月L1-QuasiHalo轨道(地心惯性系)0622.txt";
                using (StreamWriter sw = new StreamWriter(Path.Combine(cd, fileName)))
                {
                    sw.WriteLine("# T0: " + t0.ToGregorianDate().ToString());
                    sw.WriteLine("# 地月QuasiHalo轨道初始轨道(真实力模型修正后，地心惯性系)");
                    sw.WriteLine("");
                    sw.WriteLine("# 初始位置、速度(km,km/s)");
                    //  获取初始状态点（km,km/s)
                    double[] x0 = em.RV2Real(rlt.Q0_Norm_ECI_All)[0].Multiply(0.001);
                    sw.WriteLine(string.Format("{0} {1} {2}", x0[0], x0[1], x0[2]));
                    sw.WriteLine(string.Format("{0} {1} {2}", x0[3], x0[4], x0[5]));
                    sw.WriteLine("# 初始轨道根数");
                    KeplerianElements elm = new KeplerianElements(new Cartesian(x0), new Cartesian(x0, 3), em.MuEarth * 1e-9);
                    sw.WriteLine(string.Format("{0} {1} {2}", elm.SemimajorAxis, elm.Eccentricity, elm.Inclination / d2r));
                    sw.WriteLine(string.Format("{0} {1} {2}", elm.RightAscensionOfAscendingNode / d2r, elm.ArgumentOfPeriapsis / d2r, elm.ComputeMeanAnomaly() / d2r));
                    sw.WriteLine("全程轨道");

                    var allRV = em.RV2Real(rlt.Q0_Norm_ECI_All);

                    //  地心惯性系下所有的为T,R,V(s,km,km/s)
                    for (int i = 0; i < allRV.Count; i++)
                    {
                        sw.WriteLine("{0,15:F3}  {1,15:F3}  {2,15:F3}  {3,15:F3}  {4,15:F6}  {5,15:F6} {6,15:F6}",
                            rlt.T_Norm_All[i] * em.TUNIT, allRV[i][0] * 0.001, allRV[i][1] * 0.001, allRV[i][2] * 0.001,
                            allRV[i][3] * 0.001, allRV[i][4] * 0.001, allRV[i][5] * 0.001);
                    }
                }

                Assert.IsTrue(true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

    }
}