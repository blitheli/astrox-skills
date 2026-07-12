using AeroSpace.MathLib;
using ScottPlot;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{
  
    /// <summary>
    /// EmCRTBP_DRO：按 Ax 插值初值后微分修正，检验典型振幅（100000 km）周期闭合。
    /// </summary>
    [TestMethod]
    public void EmCRTBP_DRO_100000km()
    {
        const double ax = 100000.0 / 384400.0; // ≈0.260146
        HaloOrbitResults rlt = LibrationBase.EmCRTBP_DRO(ax);

        Console.WriteLine(rlt.Message);
        Console.WriteLine("X0: \n" + rlt.X0!.ArrayToString());
        Console.WriteLine($"Period: {rlt.Period}");

        Assert.IsTrue(rlt.IsSuccess, rlt.Message);
        Assert.AreEqual(1.0 + ax, rlt.X0[0], 1e-12);
        Assert.AreEqual(0.0, rlt.X0[2], 1e-8);
        Assert.IsTrue(rlt.X0[4] < 0);

        double[] dlt = rlt.X0.Sub(rlt.ListX!.Last());
        Assert.AreEqual(0.0, dlt.Norm(), 1e-11);
    }

    /// <summary>
    /// EmCRTBP_DRO：表端点与中间振幅（3e4–2e5 km，步长 5000 km）均可修正并周期闭合。
    /// </summary>
    [TestMethod]
    public void EmCRTBP_DRO_TableAmplitudes()
    {
        const double unitL = 384400.0;
        const double aKmMin = 30000;
        const double aKmMax = 200000;
        const double aKmStep = 3000;
        int n = (int)((aKmMax - aKmMin) / aKmStep) + 1;
        double[] aKm = Enumerable.Range(0, n).Select(i => aKmMin + i * aKmStep).ToArray();

        foreach (double akm in aKm)
        {
            double ax = akm / unitL;
            HaloOrbitResults rlt = LibrationBase.EmCRTBP_DRO(ax);
            Console.WriteLine($"A={akm} km  {rlt.Message}  T={rlt.Period:F6}  Vy={rlt.X0?[4]:G6}");
            Assert.IsTrue(rlt.IsSuccess, $"A={akm} km: {rlt.Message}");
            Assert.AreEqual(1.0 + ax, rlt.X0![0], 1e-12);
            Assert.AreEqual(0, rlt.X0![1], 1e-13);
            Assert.AreEqual(0, rlt.X0![2], 1e-13);
            Assert.AreEqual(0, rlt.X0![3], 1e-13);
            Assert.AreEqual(0, rlt.X0![5], 1e-13);

            Assert.IsTrue(rlt.X0[4] < 0);
            double[] dlt = rlt.X0.Sub(rlt.ListX!.Last());
            Assert.AreEqual(0.0, dlt.Norm(), 1e-11, $"A={akm} km closure");
        }
    }
}
