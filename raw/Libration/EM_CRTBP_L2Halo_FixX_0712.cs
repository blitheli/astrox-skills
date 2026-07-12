using System.Text.Encodings.Web;
using System.Text.Json;
using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{


    //  地月L2点 Northern Halo轨道簇的生成,保存为EM-L2-Halo.json供算法加载！
    //  Az振幅0.02-0.2然后再略小到NRHO轨道,使用Ax从大到小计算
    //  Ax定义为距离月球的无量纲: 0.192-0.04
    [TestMethod()]
    public void EM_CRTBP_L2Halo_FixX_0712()
    {
        const double U = 0.01215058560962404;
        const int LI = 2;
        const double AZ = 0.014;

        // 源数据目录: ASTROX.AeroSpace/Data/Libration（相对测试输出目录回退到仓库根）
        string exportDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "ASTROX.AeroSpace", "Data", "Libration"));
        string exportPath = Path.Combine(exportDir, "EM-L2-Halo.json");

        //  高阶解析解给出的Halo轨道离散点（首个点在xz平面上)
        LibrationBase.HALOAnalyX0(U, LI, AZ, out double[] x0, out double TTh);
        x0[0] += U;

        x0[0] = Math.Round(x0[0], 4);   //  x初始值保留4位小数

        var orbitNodes = new List<EmHaloOrbitNodeExport>();

        //  小步长延拓更稳
        double dx = 0.0002;

        Console.WriteLine("x_moon    Az    Period    Vy0");

        double azPrev = x0[2];
        while (x0[0] > 1.026 - 1e-10)
        {
            HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixX(U, x0, TTh);

            if (!rlt.IsSuccess)
            {
                Console.WriteLine(rlt.Message);
                break;
            }
            //  周期轨道 初始点和末点位置速度差 < 1e-11 (理论上应该重合)
            double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
            Assert.AreEqual(0, dltX0Xt.Norm(), 1e-10);

            x0 = rlt.X0;
            TTh = rlt.Period;
            Console.WriteLine($"{(x0[0]-1):F6}   {x0[2]:F6}   {TTh:F5}   {rlt.X0[4]}");

            //  应保持在Halo族(Az不为0), 且相邻点Az连续变化
            Assert.IsTrue(Math.Abs(x0[2]) > 1e-3, $"塌到平面Lyapunov, Az={x0[2]}");
            Assert.AreEqual(azPrev, x0[2], 0.01, $"Az跳变过大");

            azPrev = x0[2];
            orbitNodes.Add(CreateOrbitNode(rlt));
            x0[0] -= dx;
        }

        //  输出时Ax从小到达排序
        orbitNodes.Sort((a, b) => a.Ax.CompareTo(b.Ax));

        var data = new EmHaloDataExport
        {
            Name = "EM-L2-North-Halo",
            Text = "地月L2点Halo轨道簇初值(会合坐标系,无量纲,原点在主天体).本数据为North HALO,若想获取South Halo,在Orbits中X0[2]设为负值即可。" +
            "Ax为距月球距离,0.192在x轴最右侧,X0为穿越XZ平面,Halo轨道右上侧",
            Text2 = "从L2 右侧上部开始, Ax逐渐变小(0.04-0.192), Az先增大再变小。",
            U = U,
            LibrationPoint = LI,
            CoordinateSystem = "CRTBP rotating, nondimensional, origin at primary body",
            AxDefinition = "x0 - 1",
            OrbitsOrderBy = "Ax ascending",
            Orbits = orbitNodes
        };

        Directory.CreateDirectory(exportDir);
        File.WriteAllText(exportPath,
            JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        Console.WriteLine($"已导出 {exportPath}: {data.Orbits.Count} 个节点");

        EmHaloDataExport? loaded = JsonSerializer.Deserialize<EmHaloDataExport>(File.ReadAllText(exportPath));
        Assert.IsNotNull(loaded);
        AssertEmHaloDataContract(loaded);
        Console.WriteLine($"已校验源数据 {exportPath}: {loaded.Orbits.Count} 个节点, Ax=[{loaded.Orbits[0].Ax:F6}, {loaded.Orbits[^1].Ax:F6}]");
    }

    private static EmHaloOrbitNodeExport CreateOrbitNode(HaloOrbitResults rlt)
    {
        double[] x0 = rlt.X0;
        return new EmHaloOrbitNodeExport
        {
            // L2: Ax = x0 - 1; L1: Ax = 1 - x0。统一为距月球无量纲距离
            Ax = Math.Abs(x0[0] - 1.0),
            Az = x0[2],
            Period = rlt.Period,
            X0 = (double[])x0.Clone()
        };
    }

    private static void AssertEmHaloDataContract(EmHaloDataExport data)
    {
        Assert.AreEqual(0.01215058560962404, data.U, 1e-15);

        for (int i = 0; i < data.Orbits.Count; i++)
        {
            EmHaloOrbitNodeExport node = data.Orbits[i];
            Assert.IsTrue(double.IsFinite(node.Ax), $"节点 {i} 的 Ax 不是有限值");
            Assert.IsTrue(double.IsFinite(node.Az), $"节点 {i} 的 Az 不是有限值");
            Assert.IsTrue(double.IsFinite(node.Period) && node.Period > 0, $"节点 {i} 的 Period 无效");
            Assert.IsNotNull(node.X0, $"节点 {i} 的 X0 为空");
            Assert.AreEqual(6, node.X0.Length, $"节点 {i} 的 X0 长度无效");
            Assert.IsTrue(node.X0.All(double.IsFinite), $"节点 {i} 的 X0 包含非有限值");
            Assert.IsTrue(Math.Abs(node.Az) > 1e-3, $"节点 {i} 塌到平面 Lyapunov");
            Assert.AreEqual(Math.Abs(node.X0[0] - 1.0), node.Ax, 1e-12, $"节点 {i} 的 Ax 与 X0[0] 不一致");
            Assert.AreEqual(node.X0[2], node.Az, 1e-12, $"节点 {i} 的 Az 与 X0[2] 不一致");

            if (i > 0)
            {
                // L1 按 Az 延拓；L2 按 Ax 延拓
                if (data.LibrationPoint == 1)
                    Assert.IsTrue(data.Orbits[i - 1].Az < node.Az, $"节点 {i - 1} 和 {i} 的 Az 未严格递增");
                else
                    Assert.IsTrue(data.Orbits[i - 1].Ax < node.Ax, $"节点 {i - 1} 和 {i} 的 Ax 未严格递增");
            }
        }
    }

    private sealed class EmHaloDataExport
    {
        public string Name { get; set; } = "";
        public string Text { get; set; } = "";
        public string Text2 { get; set; } = "";

        public double U { get; set; }
        public int LibrationPoint { get; set; }
        public string CoordinateSystem { get; set; } = "";
        public string AxDefinition { get; set; } = "";
        public string AzDefinition { get; set; } = "";
        public string OrbitsOrderBy { get; set; } = "";
        public List<EmHaloOrbitNodeExport> Orbits { get; set; } = new();
    }

    private sealed class EmHaloOrbitNodeExport
    {
        public double Ax { get; set; }
        public double Az { get; set; }
        public double Period { get; set; }
        public double[] X0 { get; set; } = Array.Empty<double>();
    }
}
