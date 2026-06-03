using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
        测试 Astrogator   太阳系积分器（太阳二体+行星摄动）
            MCS:
            >   Initial_State   Sun MeanEclpJ2000惯性系
            >   Propagate       Sun_point_mass积分器
   
        此行星轨道为2024YR5小行星，与地球距离较近

        STK 采用的Heliocentric模型，将木星，土星，天王星，海王星，冥王星改为: EphemerisSource: DeFile
            原来为 SPICE Barycenter
    
        与STK结果对比:   
            由于初始状态的坐标系为MeanEclpJ2000，
            MeanEclpJ2000 -> Inertial系 与STK存在差别，
            初始值与STK相差:  位置: 0.03km   速度: 1e-8 km/s，相对精度约为1e-10

            主要误差来源：
            地球摄动力影响, 位置: 1km， 待后续自我编程验证
            土星摄动力影响，位置: 4km,  待后续
            
        20250309
    */
    [TestMethod()]
    public void Heliocentric_250309()
    {
        //  json字符串,并序列化为类对象
        string inputStr = """
        {
          "CentralBody": "Sun",

          "MainSequence": [
            {
              "$type": "InitialState",
              "Name": "初始段",
              "InitialState": {
                "Cd": 2.2,
                "CoordSystemName": "Sun MeanEclpJ2000",
                "Cr": 1.0,
                "DragArea": 20,
                "DryMass": 500,
                "Element": {
                  "$type": "Keplerian",
                  "GravitationalParameter": 1.3271244004193938E20,
                  "SemiMajorAxis": 3.7983698223359466e11,
                  "Eccentricity": 0.6641347,
                  "Inclination": 3.45277,
                  "RAAN": 271.41236,
                  "ArgOfPeriapsis": 134.64226,
                  "TrueAnomaly": 307.4397327907328
                },
                "Epoch": "2024-10-16T23:58:50.818Z",
                "FuelMass": 500,
                "SRPArea": 20
              }
            },
            {
              "$type": "Propagate",
              "Name": "轨道递推段",
              "PropagatorName": "Heliocentric",
              "MaxPropagationTime": 864000000,
              "StopConditions": [
                {
                    "$type": "Epoch",
                    "Name": "Epoch",
                    "Active": true,
                    "Description": "Stop on a specified date & time",
                    "Trip": "2033-01-01T00:00:00.000Z"
                }
              ]
            }
          ]
        }
        """;
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();
                   
        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        //  标准值为STK 的计算结果, 86400s 的数值
        //  第多个数值，(与STK中的时间一致)
        //int id = output.Position.cartesianVelocity.Length;
        int id = output.Position.cartesianVelocity.Length;
        var rv = output.Position.cartesianVelocity;
        //  初始值
        Console.WriteLine(rv[0]);
        Console.WriteLine(rv[1]);
        Console.WriteLine(rv[2]);
        Console.WriteLine(rv[3]);
        Console.WriteLine(rv[4]);
        Console.WriteLine(rv[5]);

        //  最终状态
        Console.WriteLine(rv[id - 7]);
        Console.WriteLine(rv[id - 6]);
        Console.WriteLine(rv[id - 5]);
        Console.WriteLine(rv[id - 4]);
        Console.WriteLine(rv[id - 3]);
        Console.WriteLine(rv[id - 2]);
        Console.WriteLine(rv[id - 1]);

        /*  STK结果 （Sun Inertial 坐标系）                
        初始状态 
            Parameter Set Type:  Cartesian                                                                 
             X:    1.4999287761884630e+08 km              Vx:       -9.1078522542258558 km/sec     
             Y:   -1.9316259656122576e+07 km              Vy:       33.5612846120333614 km/sec     
             Z:    1.4584276649994708e+06 km              Vz:       14.0107742923014680 km/sec    
                                                                                                       
            Parameter Set Type:  Keplerian                                                                 
            sma:    3.7983698223359501e+08 km            RAAN:         351.4066455176351 deg        
            ecc:        0.6641346999999995                  w:         53.93160789876569 deg        
            inc:         23.76213923968923 deg             TA:         307.4397327907328 deg 

        Parameter Set Type:  Cartesian                                                                        
         X:   -3.1947810308346119e+07 km              Vx:      -35.7574972366463157 km/sec            
         Y:    1.4229156566178265e+08 km              Vy:        6.7102820327871733 km/sec            
         Z:    5.9717806815745264e+07 km              Vz:        0.5543389473818358 km/sec  
        */
        //Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
        Assert.AreEqual(-3.1947810308346119e+07, rv[id - 6] * 0.001, 5);
        Assert.AreEqual(1.4229156566178265e+08, rv[id - 5] * 0.001, 1);
        Assert.AreEqual(5.9717806815745264e+07, rv[id - 4] * 0.001, 1);
        Assert.AreEqual(-35.7574972366463157, rv[id - 3] * 0.001, 2e-6);
        Assert.AreEqual(6.7102820327871733, rv[id - 2] * 0.001, 2e-6);
        Assert.AreEqual(0.5543389473818358, rv[id - 1] * 0.001, 2e-6);
        /*
            0
        149992877615.78275
        -19316259680.45297
        1458427657.79214
        -9107.85224804418
        33561.28461393735
        259027269.182
        -31947813982.99216
        142291565788.6298
        59717806768.05769
        -35757.498097654156
        6710.281148120535
        554.3386784915219
         */

    }

}