using System.Text.Encodings.Web;
using System.Text.Json;
using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{
    //  地月平面 DRO 轨道族生成,保存为 EM-DRO.json 供 EmCRTBP_DRO 加载
    //  使用 HaloOrbitX0_FixX(isHalo:false)：固定 Ax，按 A_km=3e4~2e5、步长 5000 km 延拓
    [TestMethod()]
    public void EM_CRTBP_Dro_260712()
    {
        const double U = 0.01215058560962404;
        const double unitLKm = 384400.0;
        // 地月 sidereal month ≈27.321661 d → 无量纲时间单位对应天数
        const double unitTDays = 27.321661 / (2.0 * Math.PI);
        const double aKmMin = 30000;
        const double aKmMax = 200000;
        const double aKmStep = 5000;
        const double aKmSeed = 100000;
        const int minNodeCount = 20;

        string exportDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "ASTROX.AeroSpace", "Data", "Libration"));
        string exportPath = Path.Combine(exportDir, "EM-DRO.json");

        //  典型 DRO 种子（A=100000 km）：质心系参考初值转到主天体原点，再对齐 Ax=A_km/UnitL
        double[] x0 =
        {
            1.0 + aKmSeed / unitLKm,
            0.0,
            0.0,
            0.0,
            -0.5716113326981,
            0.0
        };
        double TTh = 4.3174583227;

        HaloOrbitResults seed = LibrationBase.HaloOrbitX0_FixX(U, x0, TTh, isHalo: false);
        Assert.IsTrue(seed.IsSuccess, $"种子修正失败: {seed.Message}");
        Assert.IsNotNull(seed.X0);
        Assert.IsNotNull(seed.ListX);
        Assert.AreEqual(0.0, seed.X0.Sub(seed.ListX.Last()).Norm(), 1e-10);

        x0 = seed.X0;
        TTh = seed.Period;

        var orbitByAKm = new SortedDictionary<double, EmDroOrbitNodeExport>();
        AddDroNode(orbitByAKm, aKmSeed, seed, U, unitTDays);

        Console.WriteLine("A_km    Ax       Period    Vy");
        Console.WriteLine($"{aKmSeed:F0}   {x0[0] - 1:F6}   {TTh:F5}   {x0[4]}");

        //  自种子向两侧按 5000 km 延拓（固定 x，平面 DRO）
        ContinueDroFamily(
            U, unitLKm, unitTDays, aKmSeed, aKmMin, -aKmStep,
            ref x0, ref TTh, orbitByAKm);
        //  恢复种子状态后再向大振幅延拓
        x0 = seed.X0;
        TTh = seed.Period;
        ContinueDroFamily(
            U, unitLKm, unitTDays, aKmSeed, aKmMax, aKmStep,
            ref x0, ref TTh, orbitByAKm);

        Assert.IsTrue(orbitByAKm.Count >= minNodeCount,
            $"延拓节点不足 {minNodeCount} 个: {orbitByAKm.Count}");

        var orbits = orbitByAKm.Values.ToList();
        var data = new EmDroDataExport
        {
            Name = "EM-DRO",
            Text = "地月平面 DRO 族初值(会合坐标系,无量纲,原点在主天体)。Ax 为远离月球一侧振幅: Ax=X0[0]-1。" +
                   "Vy 为 +X 轴穿越时的逆行速度。由 HaloOrbitX0_FixX(isHalo:false) 微分修正延拓生成。",
            U = U,
            UnitL_km = unitLKm,
            CoordinateSystem = "CRTBP rotating, nondimensional, origin at primary body",
            AxDefinition = "X0[0] - 1 (moon-relative amplitude on far side)",
            AxRange = new[] { orbits[0].Ax, orbits[^1].Ax },
            OrbitsOrderBy = "Ax ascending",
            Orbits = orbits
        };

        Directory.CreateDirectory(exportDir);
        File.WriteAllText(exportPath,
            JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        Console.WriteLine($"已导出 {exportPath}: {data.Orbits.Count} 个节点");

        EmDroDataExport? loaded = JsonSerializer.Deserialize<EmDroDataExport>(File.ReadAllText(exportPath));
        Assert.IsNotNull(loaded);
        AssertEmDroDataContract(loaded);
        Console.WriteLine(
            $"已校验源数据 {exportPath}: {loaded.Orbits.Count} 个节点, " +
            $"Ax=[{loaded.Orbits[0].Ax:F6}, {loaded.Orbits[^1].Ax:F6}], " +
            $"A_km=[{loaded.Orbits[0].A_km:F0}, {loaded.Orbits[^1].A_km:F0}]");
    }

    private static void ContinueDroFamily(
        double U,
        double unitLKm,
        double unitTDays,
        double aKmStart,
        double aKmEnd,
        double aKmStep,
        ref double[] x0,
        ref double TTh,
        SortedDictionary<double, EmDroOrbitNodeExport> orbitByAKm)
    {
        double direction = Math.Sign(aKmStep);
        double aKm = aKmStart;
        while ((direction < 0 && aKm > aKmEnd + 1e-9) || (direction > 0 && aKm < aKmEnd - 1e-9))
        {
            aKm += aKmStep;
            if ((direction < 0 && aKm < aKmEnd - 1e-9) || (direction > 0 && aKm > aKmEnd + 1e-9))
                break;

            double[] xTry = (double[])x0.Clone();
            xTry[0] = 1.0 + aKm / unitLKm;
            xTry[1] = 0.0;
            xTry[2] = 0.0;
            xTry[3] = 0.0;
            xTry[5] = 0.0;

            HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixX(U, xTry, TTh, isHalo: false);
            if (!rlt.IsSuccess)
            {
                Console.WriteLine($"A={aKm:F0} km 失败: {rlt.Message}");
                break;
            }

            Assert.IsNotNull(rlt.X0);
            Assert.IsNotNull(rlt.ListX);
            double[] dlt = rlt.X0.Sub(rlt.ListX.Last());
            Assert.AreEqual(0.0, dlt.Norm(), 1e-10, $"A={aKm:F0} km 未周期闭合");
            Assert.AreEqual(1.0 + aKm / unitLKm, rlt.X0[0], 1e-12, "FixX 未能保持 Ax");
            Assert.AreEqual(0.0, rlt.X0[2], 1e-8, "平面 DRO 的 Az 应≈0");
            Assert.IsTrue(rlt.X0[4] < 0, $"A={aKm:F0} km Vy 应为逆行负值");

            x0 = rlt.X0;
            TTh = rlt.Period;
            Console.WriteLine($"{aKm:F0}   {x0[0] - 1:F6}   {TTh:F5}   {x0[4]}");
            AddDroNode(orbitByAKm, aKm, rlt, U, unitTDays);
        }
    }

    private static void AddDroNode(
        SortedDictionary<double, EmDroOrbitNodeExport> orbitByAKm,
        double aKm,
        HaloOrbitResults rlt,
        double mu,
        double unitTDays)
    {
        double[] x0 = rlt.X0!;
        orbitByAKm[aKm] = new EmDroOrbitNodeExport
        {
            A_km = aKm,
            Ax = x0[0] - 1.0,
            Period = rlt.Period,
            Period_d = Math.Round(rlt.Period * unitTDays, 3),
            JacobiC = Math.Round(ComputeJacobiC_M1(mu, x0), 3),
            Vy = x0[4]
        };
    }

    /// <summary>主天体原点会合系状态 → 质心系 Jacobi 常数 C。</summary>
    private static double ComputeJacobiC_M1(double mu, double[] xm1)
    {
        double x = xm1[0] - mu;
        double y = xm1[1];
        double z = xm1[2];
        double vx = xm1[3];
        double vy = xm1[4];
        double vz = xm1[5];
        double r1 = Math.Sqrt((x + mu) * (x + mu) + y * y + z * z);
        double r2 = Math.Sqrt((x - 1.0 + mu) * (x - 1.0 + mu) + y * y + z * z);
        return x * x + y * y + 2.0 * (1.0 - mu) / r1 + 2.0 * mu / r2 - (vx * vx + vy * vy + vz * vz);
    }

    private static void AssertEmDroDataContract(EmDroDataExport data)
    {
        Assert.AreEqual(0.01215058560962404, data.U, 1e-15);
        Assert.IsTrue(data.Orbits.Count > 0);

        for (int i = 0; i < data.Orbits.Count; i++)
        {
            EmDroOrbitNodeExport node = data.Orbits[i];
            Assert.IsTrue(double.IsFinite(node.Ax), $"节点 {i} 的 Ax 不是有限值");
            Assert.IsTrue(double.IsFinite(node.Period) && node.Period > 0, $"节点 {i} 的 Period 无效");
            Assert.IsTrue(double.IsFinite(node.Vy) && node.Vy < 0, $"节点 {i} 的 Vy 无效");
            Assert.AreEqual(node.A_km / data.UnitL_km, node.Ax, 1e-12, $"节点 {i} 的 Ax 与 A_km 不一致");

            if (i > 0)
                Assert.IsTrue(data.Orbits[i - 1].Ax < node.Ax, $"节点 {i - 1} 和 {i} 的 Ax 未严格递增");
        }
    }

    private sealed class EmDroDataExport
    {
        public string Name { get; set; } = "";
        public string Text { get; set; } = "";
        public double U { get; set; }
        public double UnitL_km { get; set; }
        public string CoordinateSystem { get; set; } = "";
        public string AxDefinition { get; set; } = "";
        public double[] AxRange { get; set; } = Array.Empty<double>();
        public string OrbitsOrderBy { get; set; } = "";
        public List<EmDroOrbitNodeExport> Orbits { get; set; } = new();
    }

    private sealed class EmDroOrbitNodeExport
    {
        public double A_km { get; set; }
        public double Ax { get; set; }
        public double Period { get; set; }
        public double Period_d { get; set; }
        public double JacobiC { get; set; }
        public double Vy { get; set; }
    }
}
