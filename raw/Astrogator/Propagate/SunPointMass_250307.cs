using System.Text.Json;
using ASTROX.Coordinates;
using ASTROX.Extended;
using ASTROX.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
        测试 Astrogator
            MCS:
            >   Initial_State   Sun MeanEclpJ2000惯性系
            >   Propagate       Sun_point_mass积分器
   
        与STK结果对比:   
            由于初始状态的坐标系为MeanEclpJ2000，
            MeanEclpJ2000 -> Inertial系 与STK存在差别，
            初始值与STK相差:  位置: 0.03km   速度: 1e-8 km/s，相对精度约为1e-10

            积分到2033年约8年多的结果，与STK相差: 位置: 0.04km   速度: 1e-8 km/s，相对精度约为1e-10
            可见，最终结果与STK基本一致，仅仅是初始值的差别

        20250309
        20251210    由于EOP-V1.1.txt更新了
    */
    [TestMethod()]
    public void SunPointMass_MeanEclpJ2000_250307()
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
              "PropagatorName": "Sun_Point_Mass",
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

        /*
         0
        149992877615.78275
        -19316259680.45297
        1458427657.79214
        -9107.85224804418
        33561.28461393735
        259027269.182
        72632059136.52397
        95093257992.10513
        46174145204.37721
        -32486.315614215906
        24352.8057125847
        8464.191298586606
        */

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

        最终状态             
        Parameter Set Type: Cartesian
         X:    7.2632059106583908e+07 km Vx:      -32.4863156203130927 km / sec
         Y: 9.5093258011574537e+07 km Vy:       24.3528057048233642 km / sec
         Z: 4.6174145214177005e+07 km Vz:        8.4641912965549544 km / sec
        */
        //Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
        Assert.AreEqual(7.2632059106583908e7, rv[id - 6] * 0.001, 0.04);
        Assert.AreEqual(9.5093258011574537e7, rv[id - 5] * 0.001, 0.04);
        Assert.AreEqual(4.6174145214177005e7, rv[id - 4] * 0.001, 0.04);
        Assert.AreEqual(-32.4863156203130927, rv[id - 3] * 0.001, 1e-8);
        Assert.AreEqual(24.3528057048233642, rv[id - 2] * 0.001, 1e-8);
        Assert.AreEqual(8.4641912965549544, rv[id - 1] * 0.001, 1e-8);

    }

    /*
     
        与上个例子相同

        初值改为 Sun Inertial 坐标系, 保证初始值没有误差

        与STK结果对比:
            位置: 0.02km 速度: 5e-9 km/s，相对精度约为1e-10

        我的结果与理论值更接近，
            位置: 0.0001km 速度: 1e-11 km/s，相对精度约为1e-12
        
     */
    [TestMethod()]
    public void SunPointMass_Inertial_250307()
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
                "CoordSystemName": "Sun Inertial",
                "Cr": 1.0,
                "DragArea": 20,
                "DryMass": 500,
                "Element": {
                  "$type": "Keplerian",
                  "GravitationalParameter": 1.3271244004193938E20,
                  "SemiMajorAxis": 3.7983698223359501e11,
                  "Eccentricity": 0.6641347,
                  "Inclination": 23.76213923968923,
                  "RAAN": 351.4066455176351,
                  "ArgOfPeriapsis": 53.93160789876569,
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
              "PropagatorName": "Sun_Point_Mass",
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
        Console.WriteLine(rv[6]);

        //  最终状态
        Console.WriteLine(rv[id - 7]);
        Console.WriteLine(rv[id - 6]);
        Console.WriteLine(rv[id - 5]);
        Console.WriteLine(rv[id - 4]);
        Console.WriteLine(rv[id - 3]);
        Console.WriteLine(rv[id - 2]);
        Console.WriteLine(rv[id - 1]);

        /*
          0
        149992877618.84607
        -19316259656.122604
        1458427664.9994621
        -9107.852254225847
        33561.284612033385
        14010.774292301485
        259027269.182
        72632059118.81877
        95093258002.40442
        46174145210.98986
        -32486.315618592347
        24352.805707075917
        8464.19129764864
        */

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

        最终状态             
        Parameter Set Type: Cartesian
         X:    7.2632059106583908e+07 km Vx:      -32.4863156203130927 km / sec
         Y: 9.5093258011574537e+07 km Vy:       24.3528057048233642 km / sec
         Z: 4.6174145214177005e+07 km Vz:        8.4641912965549544 km / sec
        */
        //Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
        Assert.AreEqual(7.2632059106583908e7, rv[id - 6] * 0.001, 0.02);
        Assert.AreEqual(9.5093258011574537e7, rv[id - 5] * 0.001, 0.01);
        Assert.AreEqual(4.6174145214177005e7, rv[id - 4] * 0.001, 0.01);
        Assert.AreEqual(-32.4863156203130927, rv[id - 3] * 0.001, 5e-9);
        Assert.AreEqual(24.3528057048233642, rv[id - 2] * 0.001, 5e-9);
        Assert.AreEqual(8.4641912965549544, rv[id - 1] * 0.001, 5e-9);

        //  理论值
        double d2r = Math.PI / 180;
        var elm0 = new KeplerianElements(3.7983698223359501e11, 0.6641347, 
            23.76213923968923*d2r,  53.93160789876569*d2r, 351.4066455176351*d2r, 307.4397327907328*d2r, 1.3271244004193938E20);

        var T0 = new JulianDate(GregorianDate.Parse("2024-10-16T23:58:50.818Z"));
        var T1 = new JulianDate(GregorianDate.Parse("2033-01-01T00:00:00.000Z"));
        var dtSec = (T1 - T0).TotalSeconds;
        var elmt = elm0.GetElementsAfterDt(dtSec);
        var rvt = elmt.ToCartesian();
        Console.WriteLine("理论值");
        Console.WriteLine(rvt.Value.X);
        Console.WriteLine(rvt.Value.Y);
        Console.WriteLine(rvt.Value.Z);
        Console.WriteLine(rvt.FirstDerivative.X);
        Console.WriteLine(rvt.FirstDerivative.Y);
        Console.WriteLine(rvt.FirstDerivative.Z);
        /*
           理论值
            72632059118.76218
            95093258002.44542
            46174145211.00427
            -32486.315618600358
            24352.80570706565
            8464.191297643743
         */
    }
}