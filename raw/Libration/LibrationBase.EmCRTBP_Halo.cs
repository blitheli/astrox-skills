using AeroSpace.MathLib;
using ScottPlot;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{
  [TestMethod]
  public void EmCRTBP_L2Halo_WithinDataRange_ReturnsPeriodicNonPlanarHalo()
  {
    HaloOrbitResults rlt = LibrationBase.EmCRTBP_L2Halo(0.191494);

    Assert.IsTrue(rlt.IsSuccess, rlt.Message);
    Assert.IsNotNull(rlt.X0);
    Assert.IsNotNull(rlt.ListX);
    Assert.AreEqual(6, rlt.X0.Length);
    Assert.IsTrue(rlt.ListX.Count > 0);

    double[] finalState = rlt.ListX.Last();
    Assert.AreEqual(6, finalState.Length);
    Assert.IsTrue(rlt.Period > 0);
    Assert.IsTrue(rlt.X0[2] > 1e-3);
    Assert.IsTrue(rlt.X0.Sub(finalState).Norm() <= 1e-11);
  }

  [TestMethod]
  public void EmCRTBP_L2Halo_IsSouth_ReturnsNegativeAz()
  {
    HaloOrbitResults rlt = LibrationBase.EmCRTBP_L2Halo(0.191494, isSouth: true);

    Assert.IsTrue(rlt.IsSuccess, rlt.Message);
    Assert.IsTrue(rlt.X0[2] < -1e-3);
    Assert.IsTrue(rlt.X0.Sub(rlt.ListX.Last()).Norm() <= 1e-11);
  }

  [TestMethod]
  public void EmCRTBP_L2Halo_OutsideDataRange_ReturnsFailure()
  {
    HaloOrbitResults rlt = LibrationBase.EmCRTBP_L2Halo(0.01);

    Assert.IsFalse(rlt.IsSuccess);
    StringAssert.Contains(rlt.Message, "Ax");
  }

  [TestMethod]
  public void EmCRTBP_L1Halo_WithinDataRange_ReturnsPeriodicNonPlanarHalo()
  {
    HaloOrbitResults rlt = LibrationBase.EmCRTBP_L1Halo(0.10);

    Assert.IsTrue(rlt.IsSuccess, rlt.Message);
    Assert.IsNotNull(rlt.X0);
    Assert.AreEqual(0.10, rlt.X0[2], 1e-12);
    Assert.IsTrue(rlt.Period > 0);
    Assert.IsTrue(rlt.X0.Sub(rlt.ListX.Last()).Norm() <= 1e-11);
  }

  [TestMethod]
  public void EmCRTBP_L1Halo_IsSouth_ReturnsNegativeAz()
  {
    HaloOrbitResults rlt = LibrationBase.EmCRTBP_L1Halo(0.10, isSouth: true);

    Assert.IsTrue(rlt.IsSuccess, rlt.Message);
    Assert.AreEqual(-0.10, rlt.X0[2], 1e-12);
    Assert.IsTrue(rlt.X0.Sub(rlt.ListX.Last()).Norm() <= 1e-11);
  }

  /// <summary>
  /// 按 L1 Az、L2 Ax 数据范围均分各取 50 条轨道，绘制会合系 x-z 图。
  /// </summary>
  [TestMethod]
  public void plot_EmCRTBP_L1L2Halo_XZPlane()
  {
    string dataDir = Path.GetFullPath(Path.Combine(
      AppContext.BaseDirectory, "..", "..", "..", "..", "ASTROX.AeroSpace", "Data", "Libration"));
    string pngPath = Path.Combine(dataDir, "EM-CRTBP-L1L2-Halo-xz.png");

    const double emMeanMeters = 3.84747981e8;
    const double toThousandKm = emMeanMeters / 1.0e6;
    const int sampleCount = 50;

    EmHaloDataExport l1All = LoadEmHaloDataExport(Path.Combine(dataDir, "EM-L1-Halo.json"));
    EmHaloDataExport l2All = LoadEmHaloDataExport(Path.Combine(dataDir, "EM-L2-Halo.json"));

    double azMin = l1All.Orbits[0].Az;
    double azMax = l1All.Orbits[^1].Az;
    double axMin = l2All.Orbits[0].Ax;
    double axMax = l2All.Orbits[^1].Ax;
    Console.WriteLine($"L1 Az=[{azMin:F6}, {azMax:F6}], L2 Ax=[{axMin:F6}, {axMax:F6}], n={sampleCount}");

    var l1Results = new List<HaloOrbitResults>(sampleCount);
    var l2Results = new List<HaloOrbitResults>(sampleCount);
    var sw = System.Diagnostics.Stopwatch.StartNew();
    for (int i = 0; i < sampleCount; i++)
    {
      double ratio = sampleCount == 1 ? 0.0 : (double)i / (sampleCount - 1);
      double az = azMin + (azMax - azMin) * ratio;
      double ax = axMin + (axMax - axMin) * ratio;

      HaloOrbitResults l1 = LibrationBase.EmCRTBP_L1Halo(az);
      Assert.IsTrue(l1.IsSuccess, $"L1 Az={az}: {l1.Message}");
      Assert.IsNotNull(l1.X0);
      Assert.IsNotNull(l1.ListX);
      l1Results.Add(l1);

      HaloOrbitResults l2 = LibrationBase.EmCRTBP_L2Halo(ax);
      Assert.IsTrue(l2.IsSuccess, $"L2 Ax={ax}: {l2.Message}");
      Assert.IsNotNull(l2.X0);
      Assert.IsNotNull(l2.ListX);
      l2Results.Add(l2);
    }
    sw.Stop();
    Console.WriteLine(
      $"计算耗时: {sw.Elapsed.TotalMilliseconds:F1} ms " +
      $"({sw.Elapsed.TotalMilliseconds / sampleCount:F2} ms/组, 共{sampleCount}组 L1+L2)");

    var plot = new Plot();
    ApplyChineseFont(plot);

    AddEmCrtbpHaloTrajectories(plot, l1Results, toThousandKm, new Color(30, 90, 180), "L1");
    AddEmCrtbpHaloTrajectories(plot, l2Results, toThousandKm, new Color(20, 140, 70), "L2");

    var x0Marks = new List<Coordinates>(sampleCount * 2);
    foreach (HaloOrbitResults rlt in l1Results.Concat(l2Results))
      x0Marks.Add(new Coordinates((rlt.X0![0] - 1.0) * toThousandKm, rlt.X0[2] * toThousandKm));

    var markers = plot.Add.Markers(x0Marks);
    markers.Color = Colors.Red;
    markers.MarkerSize = 8;
    markers.MarkerShape = MarkerShape.FilledCircle;
    markers.LegendText = "X0";

    plot.Title($"EM-CRTBP L1/L2 North Halo x-z (各{sampleCount}条均分)");
    plot.XLabel("x-1 (1000 km)");
    plot.YLabel("z (1000 km)");
    plot.Axes.SquareUnits();
    plot.ShowLegend();
    plot.SavePng(pngPath, 1400, 1000);

    Assert.IsTrue(File.Exists(pngPath), $"未生成 {pngPath}");
    Assert.IsTrue(new FileInfo(pngPath).Length > 0);
    Assert.AreEqual(sampleCount, l1Results.Count);
    Assert.AreEqual(sampleCount, l2Results.Count);
    Console.WriteLine($"已保存: {pngPath}");
  }

  private static void AddEmCrtbpHaloTrajectories(
    Plot plot,
    List<HaloOrbitResults> orbits,
    double toThousandKm,
    Color trajColor,
    string familyLabel)
  {
    bool legendAdded = false;
    foreach (HaloOrbitResults rlt in orbits)
    {
      Assert.IsNotNull(rlt.ListX);
      double[] xs = new double[rlt.ListX.Count];
      double[] zs = new double[rlt.ListX.Count];
      for (int i = 0; i < rlt.ListX.Count; i++)
      {
        xs[i] = (rlt.ListX[i][0] - 1.0) * toThousandKm;
        zs[i] = rlt.ListX[i][2] * toThousandKm;
      }

      var scatter = plot.Add.ScatterLine(xs, zs);
      scatter.Color = trajColor;
      scatter.LineWidth = 1.2f;
      scatter.LegendText = legendAdded ? string.Empty : familyLabel;
      legendAdded = true;
    }
  }
}
