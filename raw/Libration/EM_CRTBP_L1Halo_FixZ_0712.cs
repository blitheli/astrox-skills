using System.Text.Encodings.Web;
using System.Text.Json;
using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{

    //  地月L1点 Northern Halo轨道簇的生成,保存为EM-L1-Halo.json供算法加载！
    //  使用 HaloOrbitX0_FixZ：固定 Az，延拓 Az=0.0162~0.20
    [TestMethod()]
    public void EM_CRTBP_L1Halo_FixZ_0712()
    {
        const double U = 0.01215058560962404;
        const int LI = 1;
        const double azStart = 0.015;
        const double azEnd = 0.20;
        const double daz = 0.0002;
        const int minNodeCount = 20;

        // 源数据目录: ASTROX.AeroSpace/Data/Libration（相对测试输出目录回退到仓库根）
        string exportDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "ASTROX.AeroSpace", "Data", "Libration"));
        string exportPath = Path.Combine(exportDir, "EM-L1-Halo.json");

        //  高阶解析解给出的Halo轨道初始点（L1点右上侧, 质心系 → 主天体原点）
        LibrationBase.HALOAnalyX0(U, LI, azStart, out double[] x0, out double TTh);
        //  积分半个周期到左下侧(原点已经在主天体)
        List<double[]> X0List = LibrationBase.HALOAnalyX0(U, LI, azStart, 0.5 * TTh, 2);

        //  质心在大天体,Northern Halo轨道起点,L1点左上侧,穿越XZ平面        
        x0 = X0List[1];
        x0[2] = -Math.Round(x0[2], 4);  // 取北半球Halo轨道
        x0[1] = 0;  // y=0
        x0[3] = 0;  // Vx=0
        x0[5] = 0;  // Vz=0

        var orbitNodes = new List<EmHaloOrbitNodeExport>();

        Console.WriteLine("Az    x_moon    Period    Vy0");

        double azPrev = x0[2];
        while (x0[2] <= azEnd + 1e-12)
        {
            double azTarget = x0[2];
            HaloOrbitResults rlt = LibrationBase.HaloOrbitX0_FixZ(U, x0, TTh);

            if (!rlt.IsSuccess)
            {
                Console.WriteLine($"Az={azTarget:F3} 失败: {rlt.Message}");
                break;
            }

            double[] dltX0Xt = rlt.X0.Sub(rlt.ListX.Last());
            Assert.AreEqual(0, dltX0Xt.Norm(), 1e-10);
            Assert.AreEqual(azTarget, rlt.X0[2], 1e-12, "FixZ 未能保持 Az");

            x0 = rlt.X0;
            TTh = rlt.Period;
            Console.WriteLine($"{x0[2]:F6}   {(1 - x0[0]):F6}   {TTh:F5}   {rlt.X0[4]}");

            Assert.IsTrue(Math.Abs(x0[2]) > 1e-3, $"塌到平面Lyapunov, Az={x0[2]}");
            Assert.AreEqual(azPrev, x0[2], 0.01, "Az跳变过大");

            azPrev = x0[2];
            orbitNodes.Add(CreateOrbitNode(rlt));
            x0[2] += daz;
        }

        Assert.IsTrue(orbitNodes.Count >= minNodeCount, $"延拓节点不足 {minNodeCount} 个: {orbitNodes.Count}");

        //  输出时按 Az 从小到大排序
        orbitNodes.Sort((a, b) => a.Az.CompareTo(b.Az));

        var data = new EmHaloDataExport
        {
            Name = "EM-L1-North-Halo",
            Text = "地月L1点Halo轨道簇初值(会合坐标系,无量纲,原点在主天体).本数据为North HALO,若想获取South Halo,在Orbits中X0[2]设为负值即可。" +
            "X0为穿越XZ平面、L1点左上侧; Ax=|x0-1|仅作参考",
            Text2 = "从L1 左侧上部开始, Az逐渐变大(0.017-0.20), Ax先缓慢变小再逐渐变小。",
            U = U,
            LibrationPoint = LI,
            CoordinateSystem = "CRTBP rotating, nondimensional, origin at primary body",
            AxDefinition = "abs(x0 - 1)",
            AzDefinition = "X0[2]",
            OrbitsOrderBy = "Az ascending",
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
        Console.WriteLine($"已校验源数据 {exportPath}: {loaded.Orbits.Count} 个节点, Az=[{loaded.Orbits[0].Az:F6}, {loaded.Orbits[^1].Az:F6}]");
    }

}
