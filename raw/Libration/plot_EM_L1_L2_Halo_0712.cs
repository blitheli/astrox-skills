using System.Text.Json;
using AeroSpace.Propagator;
using ScottPlot;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{

    
    //  读取 EM-L1/EM-L2 Halo 数据，以各轨道 X0 积分半个周期，画在同一张 x-z 图上并标出起点
    [TestMethod()]
    public void EM_HaloOrbit_PlotHalfPeriod_XZ_260712()
    {
        string dataDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "ASTROX.AeroSpace", "Data", "Libration"));
        string l1Path = Path.Combine(dataDir, "EM-L1-Halo.json");
        string l2Path = Path.Combine(dataDir, "EM-L2-Halo.json");
        string pngPath = Path.Combine(dataDir, "EM-Halo-halfperiod-xz.png");

        // 地月平均距离(m) → 图中单位 1000 km
        const double emMeanMeters = 3.84747981e8;
        const double toThousandKm = emMeanMeters / 1.0e6;
        const int sampleStep = 10;

        EmHaloDataExport l1All = LoadEmHaloDataExport(l1Path);
        EmHaloDataExport l2All = LoadEmHaloDataExport(l2Path);
        Assert.AreEqual(l1All.U, l2All.U, 1e-15, "L1/L2 的 U 不一致");

        // 从 JSON Orbits 中每 sampleStep 个取 1 个
        List<EmHaloOrbitNodeExport> l1Orbits = SampleEvery(l1All.Orbits, sampleStep);
        List<EmHaloOrbitNodeExport> l2Orbits = SampleEvery(l2All.Orbits, sampleStep);
        Assert.IsTrue(l1Orbits.Count > 0 && l2Orbits.Count > 0);
        Console.WriteLine($"JSON总数 L1={l1All.Orbits.Count}, L2={l2All.Orbits.Count}; 取样步长={sampleStep} → L1={l1Orbits.Count}, L2={l2Orbits.Count}");

        var rhs = new Rhf_CRTBP(l1All.U);
        var plot = new Plot();
        ApplyChineseFont(plot);

        AddEmHaloHalfPeriodTrajectories(plot, rhs, l1Orbits, toThousandKm, new Color(30, 90, 180), "L1");
        AddEmHaloHalfPeriodTrajectories(plot, rhs, l2Orbits, toThousandKm, new Color(20, 140, 70), "L2");

        var x0Marks = new List<Coordinates>(l1Orbits.Count + l2Orbits.Count);
        foreach (EmHaloOrbitNodeExport node in l1Orbits.Concat(l2Orbits))
        {
            // 横坐标先相对月球 (x-1)，再换算为 1000 km
            x0Marks.Add(new Coordinates((node.X0[0] - 1.0) * toThousandKm, node.X0[2] * toThousandKm));
        }

        var markers = plot.Add.Markers(x0Marks);
        markers.Color = Colors.Red;
        markers.MarkerSize = 10;
        markers.MarkerShape = MarkerShape.FilledCircle;
        markers.LegendText = "X0";

        plot.Title($"EM-L1/L2 North Halo 半周期 x-z (每{sampleStep}个取1: L1={l1Orbits.Count}, L2={l2Orbits.Count})");
        plot.XLabel("x-1 (1000 km)");
        plot.YLabel("z (1000 km)");
        plot.Axes.SquareUnits();
        plot.ShowLegend();
        plot.SavePng(pngPath, 1400, 1000);

        Assert.IsTrue(File.Exists(pngPath), $"未生成 {pngPath}");
        Assert.IsTrue(new FileInfo(pngPath).Length > 0);
        // 确认确实做了抽稀（远小于原始节点数）
        Assert.IsTrue(l1Orbits.Count < l1All.Orbits.Count / 5, $"L1 未抽稀: {l1Orbits.Count}/{l1All.Orbits.Count}");
        Assert.IsTrue(l2Orbits.Count < l2All.Orbits.Count / 5, $"L2 未抽稀: {l2Orbits.Count}/{l2All.Orbits.Count}");
        Console.WriteLine($"已保存: {pngPath}");
    }

    /// <summary>为 ScottPlot 注册并启用中文字体，避免 Linux 下标题/坐标轴中文缺字。</summary>
    private static void ApplyChineseFont(Plot plot)
    {
        const string fontName = "WenQuanYi Micro Hei";
        string[] candidates =
        {
            "/usr/share/fonts/truetype/wqy/wqy-microhei.ttc",
            "/usr/share/fonts/truetype/droid/DroidSansFallbackFull.ttf",
        };

        foreach (string path in candidates)
        {
            if (!File.Exists(path))
                continue;
            Fonts.AddFontFile(fontName, path);
            Fonts.Default = fontName;
            plot.Font.Set(fontName);
            return;
        }

        // 回退: 让 ScottPlot 按中文样本文本自动挑选系统字体
        string detected = Fonts.Detect("汉字");
        if (!string.IsNullOrWhiteSpace(detected))
        {
            Fonts.Default = detected;
            plot.Font.Set(detected);
        }
    }

    private static EmHaloDataExport LoadEmHaloDataExport(string jsonPath)
    {
        Assert.IsTrue(File.Exists(jsonPath), $"未找到数据文件: {jsonPath}");
        EmHaloDataExport? data = JsonSerializer.Deserialize<EmHaloDataExport>(File.ReadAllText(jsonPath));
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Orbits.Count > 0, $"{jsonPath} 无轨道节点");
        return data;
    }

    /// <summary>从 Orbits 列表按固定步长取样：索引 0, step, 2*step, ...</summary>
    private static List<EmHaloOrbitNodeExport> SampleEvery(List<EmHaloOrbitNodeExport> orbits, int step)
    {
        Assert.IsTrue(step >= 1);
        var sampled = new List<EmHaloOrbitNodeExport>((orbits.Count + step - 1) / step);
        for (int i = 0; i < orbits.Count; i += step)
            sampled.Add(orbits[i]);
        return sampled;
    }

    private static void AddEmHaloHalfPeriodTrajectories(
        Plot plot,
        Rhf_CRTBP rhs,
        List<EmHaloOrbitNodeExport> orbits,
        double toThousandKm,
        Color trajColor,
        string familyLabel)
    {
        bool legendAdded = false;
        foreach (EmHaloOrbitNodeExport node in orbits)
        {
            double[] x0 = node.X0;
            Assert.AreEqual(6, x0.Length);
            OdeSolerResults ode = OdeSolver.solve_ivp(rhs.yhc_m1, 0, 0.5 * node.Period, x0, h_abs: 0.02);

            double[] xs = new double[ode.listX.Count];
            double[] zs = new double[ode.listX.Count];
            for (int i = 0; i < ode.listX.Count; i++)
            {
                // 横坐标: (x-1) 相对月球，再 × 地月距离 → 1000 km
                xs[i] = (ode.listX[i][0] - 1.0) * toThousandKm;
                zs[i] = ode.listX[i][2] * toThousandKm;
            }

            var scatter = plot.Add.ScatterLine(xs, zs);
            scatter.Color = trajColor;
            scatter.LineWidth = 1.2f;
            if (!legendAdded)
            {
                scatter.LegendText = familyLabel;
                legendAdded = true;
            }
            else
            {
                scatter.LegendText = string.Empty;
            }
        }
    }


}
