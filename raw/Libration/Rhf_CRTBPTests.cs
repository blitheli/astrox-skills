using AeroSpace.MathLib;
using AeroSpace.Propagator;

namespace AeroSpace.Libration.Tests
{
    [TestClass()]
    public partial class Rhf_CRTBPTests
    {
        //  地月L2点halo轨道, 以解析解为初值,数值积分一段时间
        [TestMethod()]
        public void EmL2HaloX0()
        {

            double U = 1.215058560962404E-002;

            int LI = 2;
            double AZ = 0.05;
            double DT = 1.0;

            List<double[]> X0List = LibrationBase.HALOAnalyX0(U, LI, AZ, DT, 5);

            Rhf_CRTBP yhsLib = new Rhf_CRTBP(U);
            double[] xl = OdeSolver.solve_ivp2(yhsLib.yhc_m1, 0, 4.0, X0List[0], h_abs: 0.01);

            // 1.1630053935442655     -0.08794891882255974      0.03731126827351192
            // -0.07111575548197299     -0.08414570832971033     -0.07173697986801834
            Console.WriteLine(xl.ArrayToString());
            //Console.WriteLine(xl.Sub(X0List[4]).ArrayToString());
            Assert.AreEqual(X0List[4][0], xl[0], 3E-5);
            Assert.AreEqual(X0List[4][1], xl[1], 3E-5);
            Assert.AreEqual(X0List[4][2], xl[2], 3E-5);
            Assert.AreEqual(X0List[4][3], xl[3], 3E-5);
            Assert.AreEqual(X0List[4][4], xl[4], 3E-5);
            Assert.AreEqual(X0List[4][5], xl[5], 3E-5);
        }

        /// <summary>
        /// DRO 无量纲状态点（地月质心会合坐标系）验证 yhc_zhixin + solve_ivp(t_eval)
        /// 振幅 A=100000 km；μ=0.012150585609；步长≈1 天（0.2299756183 TU）
        /// 与python计算结果对比，输出固定步长
        /// </summary>
        [TestMethod]
        public void dro_solver_260710()
        {
            const double mu = 0.012150585609;
            

            //  这个是python代码计算DRO轨道的例子
            // t, x, y, z, vx, vy, vz
            double[,] refStates =
            {
                { 0.000000000000e+00, 1.247995095973e+00,  0.000000000000e+00, 0.0,  0.000000000000e+00, -5.716113326981e-01, 0.0 },
                { 2.299756183256e-01, 1.229973501667e+00, -1.284903774426e-01, 0.0, -1.533338599286e-01, -5.335057553836e-01, 0.0 },
                { 4.599512366513e-01, 1.180138197038e+00, -2.405800473092e-01, 0.0, -2.727345236419e-01, -4.329571627128e-01, 0.0 },
                { 6.899268549769e-01, 1.108143979125e+00, -3.246720702646e-01, 0.0, -3.453534463366e-01, -2.935406236778e-01, 0.0 },
                { 9.199024733026e-01, 1.024912741461e+00, -3.739504874100e-01, 0.0, -3.708617549872e-01, -1.324965284954e-01, 0.0 },
                { 1.149878091628e+00, 9.408765913257e-01, -3.848543696921e-01, 0.0, -3.532903420481e-01,  3.854694927520e-02, 0.0 },
                { 1.379853709954e+00, 8.651574267666e-01, -3.561338038857e-01, 0.0, -3.001018808798e-01,  2.107432296930e-01, 0.0 },
                { 1.609829328280e+00, 8.047113874250e-01, -2.884874677679e-01, 0.0, -2.226384149465e-01,  3.754098770205e-01, 0.0 },
                { 1.839804946605e+00, 7.636214193557e-01, -1.851095843115e-01, 0.0, -1.336077357102e-01,  5.179923638272e-01, 0.0 },
                { 2.069780564931e+00, 7.437025133009e-01, -5.437625686049e-02, 0.0, -3.846862174228e-02,  6.056845067815e-01, 0.0 },
            };

            int n = refStates.GetLength(0);
            double[] tEval = new double[n];
            for (int i = 0; i < n; i++)
                tEval[i] = refStates[i, 0];

            double[] rv0 =
            {
                refStates[0, 1], refStates[0, 2], refStates[0, 3],
                refStates[0, 4], refStates[0, 5], refStates[0, 6]
            };

            Rhf_CRTBP rhs = new Rhf_CRTBP(mu);
            OdeSolerResults rlt = OdeSolver.solve_ivp(
                rhs.yhc_zhixin, tEval[0], tEval[n - 1], rv0,
                h_abs: 0.01, t_eval: tEval);

            Assert.AreEqual(n, rlt.listT.Count);
            const double tol = 1e-11;
            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(refStates[i, 0], rlt.listT[i], tol, $"t[{i}]");
                double[] x = rlt.listX[i];
                Assert.AreEqual(refStates[i, 1], x[0], tol, $"x[{i}]");
                Assert.AreEqual(refStates[i, 2], x[1], tol, $"y[{i}]");
                Assert.AreEqual(refStates[i, 3], x[2], tol, $"z[{i}]");
                Assert.AreEqual(refStates[i, 4], x[3], tol, $"vx[{i}]");
                Assert.AreEqual(refStates[i, 5], x[4], tol, $"vy[{i}]");
                Assert.AreEqual(refStates[i, 6], x[5], tol, $"vz[{i}]");
            }
        }

        /// <summary>
        /// 地月 L2 Halo 周期轨道（主天体原点会合系）闭合性：积分一个周期后回到初值
        /// </summary>
        [TestMethod]
        public void EmL2Halo_m1_period_closure()
        {
            const double mu = 1.215058560962404E-002;
            const double period = 3.384919254474086;
            double[] rv0 =
            {
                1.189017399646985,
                0.0,
                0.06060558718057466,
                0.0,
                -0.17403902743307584,
                0.0
            };

            Trajectory_CRTBP_Output output = Rhf_CRTBP.RunTrajectory(new Trajectory_CRTBP_Input
            {
                U = mu,
                IsBarycentric = false,
                RV0 = rv0,
                T0 = 0.0,
                TEnd = period,
                OutStep = 0.0
            });

            Assert.IsTrue(output.IsSuccess, output.Message);
            Assert.IsNotNull(output.Positions);
            Assert.IsFalse(output.IsBarycentric);

            int n = output.Positions.Length / 7;
            double[] rvEnd =
            {
                output.Positions[(n - 1) * 7 + 1],
                output.Positions[(n - 1) * 7 + 2],
                output.Positions[(n - 1) * 7 + 3],
                output.Positions[(n - 1) * 7 + 4],
                output.Positions[(n - 1) * 7 + 5],
                output.Positions[(n - 1) * 7 + 6]
            };

            double[] dlt = rv0.Sub(rvEnd);
            Assert.AreEqual(0.0, dlt.Norm(), 1e-11);
        }
    }
}