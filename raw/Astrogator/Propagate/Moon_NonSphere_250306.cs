using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
        测试 Astrogator
            MCS:
            >   Initial_State   
            >   Propagate       

        初始轨道根数：  
            半长径：    2000000 (m)
            偏心率：    0
            轨道倾角：  45 (deg)
            升交点经度：50    (deg) 
            近地点幅角：0    (deg)
            真近点角：  0    (deg)

        力学模型：
            中心天体：     月球
            非球形引力位：（开启）
                degree：   48
                order      48
                引力文件： GL0900D.grv
                固体潮：  （开启）
                            Permanant Tides

        与STK 12结果对比:   位置精度为～3e-5 m，速度精度～2e-8 m/s
          注意，使用GL0900D.grv,内部使用De430历表，与本算法内部的De430历表一致!!
          如果使用LP150Q.grv,内部使用De403历表，与本算法内部的De430历表不一致!!

        20250306    初次创建
    */
    [TestMethod()]
    public void Moon_NonSphere_250306()
    {

        //  读取json文件，并序列化为类对象
        string inputStr = """
            {
              "CentralBody": "Moon",

              "MainSequence": [
                {
                  "$type": "InitialState",
                  "Name": "初始段",
                  "InitialState": {
                    "Cd": 2.2,
                    "CoordSystemName": "Moon Inertial",
                    "Cr": 1.0,
                    "DragArea": 20,
                    "DryMass": 500,
                    "Element": {
                      "$type": "Keplerian",
                      "GravitationalParameter": 4902800305555.4003,
                      "SemiMajorAxis": 2000000,
                      "Eccentricity": 0,
                      "Inclination": 45,
                      "RAAN": 50,
                      "ArgOfPeriapsis": 0,
                      "TrueAnomaly": 0
                    },
                    "Epoch": "2018-12-01T00:00:00.000Z",
                    "FuelMass": 500,
                    "SRPArea": 20
                  }
                },
                {
                  "$type": "Propagate",
                  "Name": "轨道递推段",
                  "PropagatorName": "Moon_NonSphere",
                  "StopConditions": [
                    {
                      "$type": "Duration",
                      "Name": "Duration",
                      "Active": true,
                      "Description": "积分固定的时长",
                      "Trip": 86400
                    }
                  ]
                }
              ],
             "Propagators": [
              {
                "Name": "Moon_NonSphere",
                "CentralBodyName": "Moon",
                "GravityModel": {
                  "$type": "GravityField",
                  "GravityFileName": "GL0900D.grv",
                  "Degree": 48,
                  "Order": 48,
                  "SolidTideType": "Permanent tide only"
                }
              }
            ]
            }
            """;
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        
        var output = input.RunMCS();
                   
        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        /*  STK结果               
            //  初始值
            X:     1285.5752193730781983 km              Vx:       -0.8480980665642368 km/sec     
            Y:     1532.0888862379549664 km              Vy:        0.7116387748562155 km/sec     
            Z:        0.0000000000000000 km              Vz:        1.1071133981615668 km/sec 
            //  最终状态
            Parameter Set Type:  Cartesian                                                                 
            X:     1204.0631616230450618 km              Vx:        0.9213928915395979 km/sec     
            Y:     -743.0636855141354999 km              Vy:        1.2616690886031445 km/sec     
            Z:    -1411.0969734605064332 km              Vz:        0.1208381537708133 km/sec  
        */

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        int id = output.Position.cartesianVelocity.Length;
        //  初始值
        Console.WriteLine(output.Position.cartesianVelocity[0]);
        Console.WriteLine(output.Position.cartesianVelocity[1]);
        Console.WriteLine(output.Position.cartesianVelocity[2]);
        Console.WriteLine(output.Position.cartesianVelocity[3]);
        Console.WriteLine(output.Position.cartesianVelocity[4]);
        Console.WriteLine(output.Position.cartesianVelocity[5]);

        //  最终状态
        Console.WriteLine(output.Position.cartesianVelocity[id - 7]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 6]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 5]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 4]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 3]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 2]);
        Console.WriteLine(output.Position.cartesianVelocity[id - 1]);
        
        //  标准值为STK 的计算结果, 86400s 的数值
        Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
        Assert.AreEqual(1204063.1616230450618, output.Position.cartesianVelocity[id - 6], 3e-5);
        Assert.AreEqual(-743063.6855141354999, output.Position.cartesianVelocity[id - 5], 3e-5);
        Assert.AreEqual(-1411096.9734605064332, output.Position.cartesianVelocity[id - 4], 3e-5);
        Assert.AreEqual(921.3928915395979, output.Position.cartesianVelocity[id - 3], 2e-8);
        Assert.AreEqual(1261.6690886031445, output.Position.cartesianVelocity[id - 2], 2e-8);
        Assert.AreEqual(120.8381537708133, output.Position.cartesianVelocity[id - 1], 2e-8);

    }
}