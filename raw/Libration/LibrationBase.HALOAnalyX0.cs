using ASTROX.Access.Constraints;
using ASTROX.MathLib;

namespace AeroSpace.Libration.Tests
{

    public partial class LibrationBaseTests
    {

        //  测试：使用侯锡云的结果对比，结果准确
        [TestMethod()]
        public void HALOAnalyX0()
        {

            double U = 1.215058560962404E-002;

            int LI = 2;
            double AZ = 0.05;
            //AZ = 0.12;

            double DT = 0.919883267488107;
            int nPoints = 100;

            List<double[]> X0List = LibrationBase.HALOAnalyX0(U, LI, AZ, DT, nPoints);
            double[] X00 = X0List[0];
            /*  侯锡云Fortran程序结果，第3个点             
            X0(1)	1.11978537596381
	        X0(2)	3.076489735911818E-002
	        X0(3)	-3.824613040553713E-002
	        X0(4)	5.293201762049559E-003
	        X0(5)	0.200045082202978
	        X0(6)	3.898691973911027E-002
             */
            double ebsl10 = 1e-10;
            Assert.AreEqual(1.11978537596381, X0List[2][0], ebsl10);
            Assert.AreEqual(3.076489735911818E-002, X0List[2][1], ebsl10);
            Assert.AreEqual(-3.824613040553713E-002, X0List[2][2], ebsl10);
            Assert.AreEqual(5.293201762049559E-003, X0List[2][3], ebsl10);
            Assert.AreEqual(0.200045082202978, X0List[2][4], ebsl10);
            Assert.AreEqual(3.898691973911027E-002, X0List[2][5], ebsl10);

            double[] X0;
            double TT;
            LibrationBase.HALOAnalyX0(U, LI, 0.01, out X0, out TT);

            //  侯锡云结果
            double[] X0h = new double[] { 1.18076303152246, 0.0, 1.176973182629384E-002, 0.0, -0.156655014159731, 0.0 };
            double TTh = 3.41439831042772;
            Assert.AreEqual(X0h[0], X0[0], ebsl10);
            Assert.AreEqual(X0h[1], X0[1], ebsl10);
            Assert.AreEqual(X0h[2], X0[2], ebsl10);
            Assert.AreEqual(X0h[3], X0[3], ebsl10);
            Assert.AreEqual(X0h[4], X0[4], ebsl10);
            Assert.AreEqual(X0h[5], X0[5], ebsl10);
            Assert.AreEqual(TTh, TT, ebsl10);
        }

        //  地月L2点Halo轨道计算, Az振幅0.03-0.1,超过0.1不收敛
        [TestMethod()]
        public void HALOAnalyX0_EML2_Search_260711()
        {
            double U = 1.215058560962404E-002;

            int LI = 2;
          
            double[] X0;
            double TT;
            double Az = 0.03;
            while (Az < 0.101)
            {
                LibrationBase.HALOAnalyX0(U, LI, Az, out X0, out TT);
                Console.WriteLine($"Az={Az:F2}, X0.Az={X0[2]:F2}, TT={TT:F5}");
                Az += 0.01;
            }

            /*
                Az=0.03, X0.Az=0.04, TT=3.40508
                Az=0.04, X0.Az=0.05, TT=3.39653
                Az=0.05, X0.Az=0.06, TT=3.38492
                Az=0.06, X0.Az=0.07, TT=3.36964
                Az=0.07, X0.Az=0.09, TT=3.34972
                Az=0.08, X0.Az=0.10, TT=3.32355
                Az=0.09, X0.Az=0.12, TT=3.28835
                Az=0.10, X0.Az=0.13, TT=3.23924
             */
        }

        //  地月L1点Halo轨道，Az = 0.02 解析解
        [TestMethod()]
        public void HALOAnalyX0_EML1_260711()
        {
            double U = 1.215058560962404E-002;

            int LI = 1;

            double[] X0;
            double TT;
            double Az = 0.02;

            LibrationBase.HALOAnalyX0(U, LI, Az, out X0, out TT);

            List<double[]> X0List = LibrationBase.HALOAnalyX0(U, LI, Az, 0.5 * TT, 2);
            Console.WriteLine("首点X0, L1点右侧上面");
            Console.WriteLine(X0List[0].ArrayToString());
            Console.WriteLine("半周期点X0, L1点左侧下面");
            Console.WriteLine(X0List[1].ArrayToString());

            /*
               首点X0, L1点右侧上面
           0.8692604497088579                        0     0.018634030377973353                        0     -0.14351627570920464                        0
    半周期点X0, L1点左侧下面
           0.8355346946317959   -7.167876321669058E-18       -0.021588121303312   -6.053650548460244E-18      0.13372749096213396    -5.08500775588771E-18
   
             */
        }
    }
}