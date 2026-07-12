using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{

    //  地月L2点 CRTBP Halo轨道, Az振幅0.05，然后固定x值进行求解迭代
    [TestMethod()]
    public void HaloOrbitX0_FixX()
    {

        double U = 1.215058560962404E-002;

        int LI = 2;
        double AZ = 0.05;
        //  高阶解析解给出的Halo轨道离散点（首个点在xz平面上)
        List<double[]> X0List = LibrationBase.HALOAnalyX0(U, LI, AZ, 1.0, 4);
        LibrationBase.HALOAnalyX0(U, LI, AZ, out double[] x0, out double TTh);
        x0 = X0List[0];

        Console.WriteLine("初值X0: \n" + x0.ArrayToString());
        Console.WriteLine($"x初值:   {x0[0]}   周期初值: {TTh}");

        HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixX(U, x0,  TTh);

        Console.WriteLine("优化X0: \n" + rlt.X0.ArrayToString());
        Console.WriteLine($"优化后周期:   {rlt.Period}");

        Assert.IsTrue(rlt.IsSuccess, rlt.Message);
        //  周期轨道 初始点和末点位置速度差 < 1e-11 (理论上应该重合)
        double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
        Assert.AreEqual(0, dltX0Xt.Norm(), 1e-11);
        /*
         初值X0:
                1.189017399646985                        0      0.06060549836662331                        0       -0.174038934697076                        0
        x初值:   1.189017399646985   周期初值: 3.384918207056504
        优化X0:
                1.189017399646985                        0      0.06060558718057466                        0     -0.17403902743307584                        0
        优化后周期:   3.384919254474086
         */
    }

    /// <summary>
    /// 典型地月平面 DRO（A≈100000 km，中等振幅段）：
    /// 会合系 +X 轴上放置初值 (x,0,0,0,vy,0)，vy&lt;0 为绕月逆行；
    /// 用 HaloOrbitX0_FixX 固定 x、修正周期与速度，检验一个周期后闭合。
    /// 坐标系：主天体(地球)原点会合系无量纲；参考质心系 x_b=1.247995095973 → x_m1=x_b+μ。
    /// </summary>
    [TestMethod]
    public void HaloOrbitX0_FixX_DRO()
    {
        const double mu = 0.012150585609;
        // 质心会合系参考初值（DRO 研究/Python 算例，A=100000 km，C≈2.892，T≈4.317 TU）
        const double xBary = 1.247995095973;
        const double vy = -0.5716113326981;
        const double period0 = 4.3174583227;

        // HaloOrbitX0_FixX 使用 yhc_m1（原点在主天体）
        double[] x0 =
        {
            xBary + mu,
            0.0,
            0.0,
            0.0,
            vy,
            0.0
        };

        // 故意给 Vy 一点扰动，验证微分修正能拉回周期 DRO
        x0[4] = vy * 1.001;

        Console.WriteLine("DRO 初值(m1): \n" + x0.ArrayToString());
        Console.WriteLine($"周期初值: {period0}");

        HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixX(mu, x0, period0);

        Console.WriteLine(rlt.Message);
        Console.WriteLine("优化X0: \n" + rlt.X0!.ArrayToString());
        Console.WriteLine($"优化后周期: {rlt.Period}");

        Assert.IsTrue(rlt.IsSuccess, rlt.Message);
        Assert.AreEqual(x0[0], rlt.X0[0], 1e-15); // 固定 x
        Assert.AreEqual(0.0, rlt.X0[2], 1e-8);     // 平面 DRO：z≈0
        Assert.AreEqual(0.0, rlt.X0[5], 1e-8);     // vz≈0
        Assert.IsTrue(rlt.X0[4] < 0);               // 逆行 Vy<0

        double[] dlt = rlt.X0.Sub(rlt.ListX!.Last());
        Assert.AreEqual(0.0, dlt.Norm(), 1e-11);
        Assert.AreEqual(period0, rlt.Period, 1e-3);
    }

}
