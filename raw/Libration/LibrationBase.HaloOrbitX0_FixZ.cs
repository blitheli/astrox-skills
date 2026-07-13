using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{

    //  地月L1点 CRTBP Halo轨道, 固定Az用 HaloOrbitX0_FixZ 修正
    [TestMethod()]
    public void HaloOrbitX0_FixZ()
    {
        double U = 1.215058560962404E-002;
        int LI = 1;
        double AZ = 0.03;

        LibrationBase.HALOAnalyX0(U, LI, AZ, out double[] x0, out double TTh);
        x0[0] += U;
        x0[2] = AZ;

        HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixZ(U, x0, TTh);
        Assert.IsTrue(rlt.IsSuccess, rlt.Message);
        Assert.AreEqual(AZ, rlt.X0[2], 1e-12);
        double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
        Assert.AreEqual(0, dltX0Xt.Norm(), 1e-11);
        Console.WriteLine("优化X0: \n" + rlt.X0.ArrayToString());
        Console.WriteLine($"优化后周期:   {rlt.Period}");
    }

}
