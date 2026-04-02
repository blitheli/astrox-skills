using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests;

[TestClass()]
public class ProgagatorJ2Tests
{
    /*
        测试 J2积分器 GetJ2 地球
        
            里面的数值取自STK计算
                   
        结果与STK结果一致(位置精度差别为~0.2m,速度精度差别为2e-4 m/s)

        20220402    初次编写,liumulin        
        20250603    输入参数，明确输入: GravitationalParameter,J2NormalizedValue,RefDistance
     */
    [TestMethod()]
    public void GetJ2Test()
    {
        string inputStr = """
                {
                  "Start": "2022-04-18T04:00:00.000Z",
                  "Stop": "2022-04-18T16:00:00.000Z",
                  "CetralBody": "Earth",
                  "OrbitEpoch": "2022-04-18T04:00:00.000Z",
                  "GravitationalParameter": 3.986004415E14,
                  "J2NormalizedValue":  0.000484165143790815,
                  "RefDistance": 6378136.3,                
                  "CoordType": "Classical",
                  "OrbitalElements": [ 6678140, 0, 28.5, 0, 0, 0 ]
                }
                """;
        var input = JsonSerializer.Deserialize<J2Input>(inputStr);

        //  webApi
        var output = PropagatorJ2.Compute(input);
        //  输出最后一个点RV
        int id = output.Position.cartesianVelocity.Length - 7;
        double[] r = [output.Position.cartesianVelocity[id+1],
                      output.Position.cartesianVelocity[id+2],
                      output.Position.cartesianVelocity[id+3]];
        double[] v = [output.Position.cartesianVelocity[id+4],
                        output.Position.cartesianVelocity[id+5],
                        output.Position.cartesianVelocity[id+6]];
        Console.WriteLine($"最后时刻: {output.Position.cartesianVelocity[id]}  s");
        Console.WriteLine($"r=({r[0]:F3}, {r[1]:F3}, {r[2]:F3}) m");
        Console.WriteLine($"v=({v[0]:F6}, {v[1]:F6}, {v[2]:F6}) m/s");

        //  标准值为STK 的计算结果
        Assert.AreEqual(43200, output.Position.cartesianVelocity[id], 0.1);
        Assert.AreEqual(6553469.097, output.Position.cartesianVelocity[id+1], 0.2);
        Assert.AreEqual(-1211978.388, output.Position.cartesianVelocity[id+2], 0.2);
        Assert.AreEqual(-425094.155, output.Position.cartesianVelocity[id + 3], 0.2);
        Assert.AreEqual(1466.353362, output.Position.cartesianVelocity[id + 4], 2e-4);
        Assert.AreEqual(6647.507518, output.Position.cartesianVelocity[id + 5], 2e-4);
        Assert.AreEqual(3653.463551, output.Position.cartesianVelocity[id + 6], 2e-4);

        //  输出到json文件
        string outputStr = JsonSerializer.Serialize(output);
        Console.WriteLine(outputStr);

    }


    /*
        测试 J2积分器 火星 默认引力场（MRO110C)

        与STK结果一致: (位置精度差别为~0.001m,速度精度差别为1e-6 m/s)

        20250603    初次编写,liumulin            
    */
    [TestMethod()]
    public void GetJ2_Mars_250503()
    {
        string inputStr = """
                {
                  "Start": "2025-06-03T00:00:00.000Z",
                  "Stop": "2025-06-04T00:00:00.000Z",
                  "CentralBody": "Mars",
                  "OrbitEpoch": "2025-06-03T00:00:00.000Z",
                  "CoordType": "Classical",
                  "OrbitalElements": [3696190, 0, 45.0, 0, 0, 0 ]
                }
                """;
        var input = JsonSerializer.Deserialize<J2Input>(inputStr);

        //  webApi
        var output = PropagatorJ2.Compute(input);

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        //  标准值为STK 的计算结果
        int id = output.Position.cartesianVelocity.Length - 7;
        //  输出最后一个点RV
        double[]  r = [output.Position.cartesianVelocity[id+1],
                      output.Position.cartesianVelocity[id+2],
                      output.Position.cartesianVelocity[id+3]];
        double[] v = [output.Position.cartesianVelocity[id+4],
                        output.Position.cartesianVelocity[id+5],
                        output.Position.cartesianVelocity[id+6]];
        Console.WriteLine($"最后时刻: {output.Position.cartesianVelocity[id]}  s");
        Console.WriteLine($"r=({r[0]:F3}, {r[1]:F3}, {r[2]:F3}) m");
        Console.WriteLine($"v=({v[0]:F6}, {v[1]:F6}, {v[2]:F6}) m/s");
        //-1575.24709717 -2263.79723898 - 2460.82083049     3.0611428476 - 1.2486122459 - 0.8108885479
        Assert.AreEqual(86400, output.Position.cartesianVelocity[id], 0.1);
        double ebslR = 0.001;
        double ebslV = 1e-6;
        Assert.AreEqual(-1575247.09717, output.Position.cartesianVelocity[id + 1], ebslR);
        Assert.AreEqual(-2263797.23898, output.Position.cartesianVelocity[id + 2], ebslR);
        Assert.AreEqual(-2460820.83049, output.Position.cartesianVelocity[id + 3], ebslR);
        Assert.AreEqual(3061.1428476, output.Position.cartesianVelocity[id + 4], ebslV);
        Assert.AreEqual(-1248.6122459, output.Position.cartesianVelocity[id + 5], ebslV);
        Assert.AreEqual(-810.8885479, output.Position.cartesianVelocity[id + 6], ebslV);
    }

}