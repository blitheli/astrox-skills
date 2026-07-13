using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{

    //  L2 Halo轨道生成， 通过高阶解析解给出初值，使用牛顿迭代法优化，得到周期轨道
    //  固定Az振幅
    //  和侯锡云结果对比，结果一致
    [TestMethod()]
    public void L2_HaloOrbit_FixZ()
    {

        double U = 1.215058560962404E-002;

        double[] posL = LibrationBase.LibrationPointPosition(U);
        Console.WriteLine("L2点位置:  " + posL[4]);

        //  高阶解析解给出的Halo轨道离散点（首个点在xz平面上)
        double[] X0;
        double TT;
        LibrationBase.HALOAnalyX0(U, 2, 0.01, out X0, out TT);

        //Az振幅由解析给出,不等于输入值
        HaloOrbitResults rlt0 = LibrationBase.HaloOrbit_FixZ(U, X0, TT);        
        //  侯锡云结果
        double[] X0h = new double[] { 1.18076305054483, 0.0, 1.176973182629384E-002, 0.0, -0.156655153705165, 0.0 };
        double TTh = 3.41439840617371;
        Assert.AreEqual(0, rlt0.X0.Sub(X0h).Norm(), 1e-12);

        //====================================================================
        //  Az振幅精确满足
        HaloOrbitResults rlt = LibrationBase.HaloOrbit_FixZ(U, 2, 0.01);
        Console.WriteLine("初值X0: ");
        Console.WriteLine(rlt.InitialX0.ArrayToString());
        Console.WriteLine("优化X0: ");
        Console.WriteLine(rlt.X0.ArrayToString());
        double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
        //  周期轨道 初始点和末点位置速度差 < 1e-11 (理论上应该重合)
        Assert.AreEqual(0, dltX0Xt.Norm(), 1e-11);

        /*
           L2点位置:  1.1556821654448841
            初值X0: 
                   1.1808069942156938                        0                     0.01                        0      -0.1563980086119829                        0
            优化X0: 
                   1.1808008948025093                        0                     0.01                        0     -0.15643404542413525                        0


         */

    }

    //  L1 Halo轨道生成， 通过高阶解析解给出初值，使用牛顿迭代法优化，得到周期轨道
    //  固定Az振幅
    [TestMethod()]
    public void L1_HaloOrbit_FixZ()
    {

        double U = 1.215058560962404E-002;

        double[] posL = LibrationBase.LibrationPointPosition(U);
        Console.WriteLine("L1点位置:  " + posL[3]);

        //  Az振幅精确满足
        HaloOrbitResults rlt = LibrationBase.HaloOrbit_FixZ(U, 1, 0.03);
        Console.WriteLine("初值X0: ");
        Console.WriteLine(rlt.InitialX0.ArrayToString());
        Console.WriteLine("优化X0: ");
        Console.WriteLine(rlt.X0.ArrayToString());
        double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
        //  周期轨道 初始点和末点位置速度差 < 1e-11 (理论上应该重合)
        Assert.AreEqual(0, dltX0Xt.Norm(), 1e-11);

        /*
           L1点位置:  0.8369151257723572
        初值X0: 
            0.8607334384521171                        0                     0.03                        0     -0.15819735436962204                        0
        优化X0: 
            0.8608215969859138                        0                     0.03                        0     -0.15854530355252094                        0
            0     -0.13658295295593512                        0
         */

    }

    //  L2 Halo轨道生成， 通过高阶解析解给出初值，使用牛顿迭代法优化，得到周期轨道
    //  Az振幅较大, 不收敛！！
    [TestMethod()]
    public void L2_HaloOrbit_FixZ_Test()
    {

        double U = 1.215058560962404E-002;

        double[] posL = LibrationBase.LibrationPointPosition(U);
        Console.WriteLine("L2点位置:  " + posL[4]);

        double Az = 0.1;
        //  Az振幅精确满足
        HaloOrbitResults rlt = LibrationBase.HaloOrbit_FixZ(U, 2, Az);

        Assert.IsFalse(rlt.IsSuccess);
    }
}