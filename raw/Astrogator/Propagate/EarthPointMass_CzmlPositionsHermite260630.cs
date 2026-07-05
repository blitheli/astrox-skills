using System.Text.Json;
using AeroSpace.Models;

namespace ASTROX.Astrogator.Tests;

public partial class PropagateTests
{
    /*
        测试 Astrogator
            MCS:
            >   Initial_State   Earth Inertial系
            >   Propagate       Earth_point_mass积分器
        
        生成的CzmlPositions数据，采用Hermite插值器, 验证插值器的正确性


      为什么 Hermite 7 更好？
      Hermite 7 阶在 cartesianVelocity 数据下，局部只需 4 个时间点（每点含位置+速度），却提供 7 阶多项式精度：

      Hermite 5 → 3 个点，5 阶
      Hermite 7 → 4 个点，7 阶
      Lagrange 7 → 8 个点，7 阶（但不用速度）
      Hermite 7 相当于用一半的时间点 + 速度信息，达到与 Lagrange 7 相同阶数，所以在两组场景下都更优。

      建议
      若星历本身带 cartesianVelocity（Astrogator MCS 输出即有），优先考虑 Hermite 7 阶，而不是 Lagrange 7 或 Hermite 5：

      高偏心率长弧段：位置误差从 ~0.35–0.48 m 降到 ~0.016 m
      LEO 短弧段：已是亚毫米级，Hermite 7 仍最好
      注意： Cesium 端需设 interpolationAlgorithm: "HERMITE"、interpolationDegree: 7；当前 MCS 默认仍输出 LAGRANGE 7，若要全面切换需改 AstrogatorMCS 的输出逻辑。

      测试文件已更新，两个测试都会同时输出 Hermite 5 / Hermite 7 / Lagrange 7 的对比汇总。

        20260630
    */
    [TestMethod()]
    public void EarthPointMass_CzmlPositionsHermite260630()
    {
        EntityPositionTwoBody twoBody = new EntityPositionTwoBody()
        {
            CentralBody = "Earth",
            GravitationalParameter = 3.986004415E14,
            OrbitEpoch = "2026-06-30T00:00:00.000Z",
            CoordSystem = "Inertial",
            CoordType = "Classical",
            OrbitalElements = new double[] { 166953425.0, 0.96, 45.0, 0.0, 0.0, 0.0 },
        };

        string inputStr = """
        {
          "CentralBody": "Earth",

          "MainSequence": [
            {
              "$type": "InitialState",
              "Name": "初始段",
              "InitialState": {
                "Cd": 2.2,
                "CoordSystemName": "Earth Inertial",
                "Cr": 1.0,
                "DragArea": 20,
                "DryMass": 500,
                "Element": {
                  "$type": "Keplerian",
                  "GravitationalParameter": 3.986004415E14,
                  "SemiMajorAxis": 166953425.0,
                  "Eccentricity": 0.96,
                  "Inclination": 45.0,
                  "RAAN": 0.0,
                  "ArgOfPeriapsis": 0.0,
                  "TrueAnomaly": 0.0
                },
                "Epoch": "2026-06-30T00:00:00.000Z",
                "FuelMass": 500,
                "SRPArea": 20
              }
            },
            {
              "$type": "Propagate",
              "Name": "轨道递推段",
              "PropagatorName": "Earth_Point_Mass",
              "MaxPropagationTime": 864000000,
              "StopConditions": [
                {
                    "$type": "Duration",
                    "Name": "Duration",
                    "Active": true,
                    "Description": "积分固定的时长",
                    "Trip": 691200.0
                }
              ]
            }
          ]
        }
        """;
        var output = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr)!.RunMCS();

        CompareInterpolationSummary(twoBody, output, 691200.0, 3600.0,
            ("Hermite 5", "HERMITE", 5),
            ("Hermite 7", "HERMITE", 7),
            ("Lagrange 7", "LAGRANGE", 7));
        /*
             插值对比 vs 二体理论 (步长3600s):
          Hermite 5     最大位置误差 0.349561 m,  最大速度误差 0.000317660 m/s
          Hermite 7     最大位置误差 0.016382 m,  最大速度误差 0.000004867 m/s
          Lagrange 7    最大位置误差 0.478854 m,  最大速度误差 0.000090305 m/s

         */
    }

    /*
        近地轨道短弧段测试
            sma: 7078137 m (近圆)
            时长: 1.5小时 (5400s)
            对比步长: 45s
            Hermite 5阶 vs Lagrange 7阶 vs 二体理论

        20260630
    */
    [TestMethod()]
    public void EarthPointMass_CzmlPositionsHermite_Leo5400s()
    {
        const double sma = 7078137.0;
        const string epoch = "2026-06-30T00:00:00.000Z";
        const double durationSec = 5400.0;
        const double stepSec = 45.0;

        EntityPositionTwoBody twoBody = new EntityPositionTwoBody()
        {
            CentralBody = "Earth",
            GravitationalParameter = 3.986004415E14,
            OrbitEpoch = epoch,
            CoordSystem = "Inertial",
            CoordType = "Classical",
            OrbitalElements = new double[] { sma, 0.0, 45.0, 0.0, 0.0, 0.0 },
        };

        string inputStr = $$"""
        {
          "CentralBody": "Earth",

          "MainSequence": [
            {
              "$type": "InitialState",
              "Name": "初始段",
              "InitialState": {
                "Cd": 2.2,
                "CoordSystemName": "Earth Inertial",
                "Cr": 1.0,
                "DragArea": 20,
                "DryMass": 500,
                "Element": {
                  "$type": "Keplerian",
                  "GravitationalParameter": 3.986004415E14,
                  "SemiMajorAxis": {{sma}},
                  "Eccentricity": 0.0,
                  "Inclination": 45.0,
                  "RAAN": 0.0,
                  "ArgOfPeriapsis": 0.0,
                  "TrueAnomaly": 0.0
                },
                "Epoch": "{{epoch}}",
                "FuelMass": 500,
                "SRPArea": 20
              }
            },
            {
              "$type": "Propagate",
              "Name": "轨道递推段",
              "PropagatorName": "Earth_Point_Mass",
              "MaxPropagationTime": 864000000,
              "StopConditions": [
                {
                    "$type": "Duration",
                    "Name": "Duration",
                    "Active": true,
                    "Description": "积分固定的时长",
                    "Trip": {{durationSec}}
                }
              ]
            }
          ]
        }
        """;
        var output = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr)!.RunMCS();

        CompareInterpolationSummary(twoBody, output, durationSec, stepSec,
            ("Hermite 5", "HERMITE", 5),
            ("Hermite 7", "HERMITE", 7),
            ("Lagrange 7", "LAGRANGE", 7));
    }

    private static void CompareInterpolationSummary(
        EntityPositionTwoBody twoBody,
        MCSOutput output,
        double durationSec,
        double stepSec,
        params (string Name, string Algorithm, int Degree)[] configs)
    {
        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        PrintPositionTimeIntervals(output);

        var pointEvaluator = twoBody.CreatePoint().GetEvaluator();
        var T0 = twoBody.GetEpoch();

        Console.WriteLine($"插值对比 vs 二体理论 (步长{stepSec}s):");
        foreach (var cfg in configs)
        {
            var positions = ClonePositions(output.Positions!);
            ApplyInterpolation(positions, cfg.Algorithm, cfg.Degree);
            var evaluator = positions.CreatePoint().GetEvaluator();

            double maxPos = 0, maxVel = 0;
            for (double t = 0; t <= durationSec; t += stepSec)
            {
                var time = T0.AddSeconds(t);
                var rvInterp = evaluator.Evaluate(time, 1);
                var rvTheory = pointEvaluator.Evaluate(time, 1);
                maxPos = Math.Max(maxPos, (rvInterp.Value - rvTheory.Value).Magnitude);
                maxVel = Math.Max(maxVel, (rvInterp.FirstDerivative - rvTheory.FirstDerivative).Magnitude);
            }
            Console.WriteLine($"  {cfg.Name,-12}  最大位置误差 {maxPos:F6} m,  最大速度误差 {maxVel:F9} m/s");
        }

        /*
             插值对比 vs 二体理论 (步长45s):
      Hermite 5     最大位置误差 0.000326 m,  最大速度误差 0.000016496 m/s
      Hermite 7     最大位置误差 0.000004 m,  最大速度误差 0.000000007 m/s
      Lagrange 7    最大位置误差 0.000058 m,  最大速度误差 0.000000061 m/s
         */
    }

    private static EntityPositionCzmlPositions ClonePositions(EntityPositionCzmlPositions source)
    {
        return JsonSerializer.Deserialize<EntityPositionCzmlPositions>(
            JsonSerializer.Serialize(source))!;
    }

    private static void ApplyInterpolation(EntityPositionCzmlPositions positions, string algorithm, int degree)
    {
        foreach (var segment in positions.CzmlPositions)
        {
            segment.interpolationAlgorithm = algorithm;
            segment.interpolationDegree = degree;
        }
    }

    private static void PrintPositionTimeIntervals(MCSOutput output)
    {
        Console.WriteLine("output.Positions中的数据的时间间隔变化:");
        for (int segIdx = 0; segIdx < output.Positions!.CzmlPositions.Length; segIdx++)
        {
            var segment = output.Positions.CzmlPositions[segIdx];
            var rv = segment.cartesianVelocity!;
            int count = rv.Length / 7;
            Console.WriteLine($"  段{segIdx + 1}: {segment.interval}, 插值={segment.interpolationAlgorithm}, 阶数={segment.interpolationDegree}, 点数={count}");
            for (int i = 1; i < count; i++)
            {
                double dt = rv[i * 7] - rv[(i - 1) * 7];
                Console.WriteLine($"    点{i - 1}->{i}: dt={dt:F3}s");
            }
        }
    }
}
