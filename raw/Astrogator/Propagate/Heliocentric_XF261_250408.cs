using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
        测试 Astrogator   太阳系积分器（太阳二体+行星摄动）
            MCS:
            >   InitialState    Sun MeanEclpJ2000惯性系
            >   Propagate       Heliocentric积分器
                                终止条件: 2029-04-06T13:00:00.000Z
   
        初始状态为XF261小行星，与地球距离较近

        STK 采用的Heliocentric模型，其中木星，土星，天王星，海王星，冥王星
                                    星历为 SPICE Barycenter
        
        而我这里的Heliocentric模型，采用的历表为JplDE430

        与STK结果对比:   
            由于初始状态的坐标系为MeanEclpJ2000，
            MeanEclpJ2000 -> Inertial系 与STK存在差别，
            初始值与STK相差:  位置: 0.03km   速度: 1e-8 km/s，相对精度约为1e-10
            
            位置误差: 6km, 速度误差: 2e-6 km/s

            带后续排查 地球等天体的引力摄动影响
        20250408
    */
    [TestMethod()]
    public void Heliocentric_XF261_250408()
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
                  "SemiMajorAxis": 1.4807255686060825e11,
                  "Eccentricity": 0.3190449959172958,
                  "Inclination": 0.7938110241547416,
                  "RAAN": 209.8223416775454,
                  "ArgOfPeriapsis": 100.8824094686265,
                  "TrueAnomaly": 249.3159183861611
                },
                "Epoch": "2025-05-04T23:58:50.815Z",
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
                    "Trip": "2029-04-06T13:00:00.000Z"
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

        //  mcs结果序列化输出
        Console.WriteLine(JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

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
        Console.WriteLine("最后状态");
        Console.WriteLine(rv[id - 7]);
        Console.WriteLine(rv[id - 6]);
        Console.WriteLine(rv[id - 5]);
        Console.WriteLine(rv[id - 4]);
        Console.WriteLine(rv[id - 3]);
        Console.WriteLine(rv[id - 2]);
        Console.WriteLine(rv[id - 1]);
        /*
             0
            -140832513600.80856
            -46943598204.90579
            -20737890753.103355
            18453.28847424302
            -21359.103761752824
            最后状态
            123771669.185
            -149753317909.90842
            -35387469439.04535
            -15950657491.357347
            15562.229819796137
            -22194.6648595458
            -9190.471030390818
         */

        /*  STK结果 （Sun Inertial 坐标系）                
                                                                                                           
UTC Gregorian Date: 6 Apr 2029 13:00:00.000  UTC Julian Date: 2462233.04166667                        
Julian Ephemeris Date: 2462233.04246741                                                               
Time past epoch: 3.98772e+07 sec   (Epoch in UTC Gregorian Date: 1 Jan 2028 00:00:00.000)             
                                                                                                      
State Vector in Coordinate System: Sun Inertial                                                       
                                                                                                      
Parameter Set Type:  Cartesian                                                                        
         X:   -1.4975332214605790e+08 km              Vx:       15.5622285573218395 km/sec            
         Y:   -3.5387463911978923e+07 km              Vy:      -22.1946651504792278 km/sec            
         Z:   -1.5950655381578688e+07 km              Vz:       -9.1904711443132641 km/sec            
                                                                                                      
Parameter Set Type:  Keplerian                                                                        
       sma:    1.4803978261276379e+08 km            RAAN:         358.9846383767705 deg               
       ecc:        0.3187040020800770                  w:         311.5731733394218 deg               
       inc:         22.75128294439372 deg             TA:         243.8893252358499 deg               
                                                                                                      
Parameter Set Type:  Spherical                                                                        
 Right Asc:         193.2953902917368 deg     Horiz. FPA:        -18.41095901834386 deg               
      Decl:        -5.918032255523651 deg        Azimuth:         112.0076931046749 deg               
       |R|:    1.5470214446802115e+08 km             |V|:       28.6225589119260135 km/sec    
        */

        var seg1 = output.MainSequenceResults[1];
        //  末态(与STK对比）日心惯性系（单位: km, km/s）
        Assert.AreEqual(-1.4975332214605790e+08, seg1.FinalState.Cartesian.X * 0.001, 5);
        Assert.AreEqual(-3.5387463911978923e+07, seg1.FinalState.Cartesian.Y * 0.001, 6);
        Assert.AreEqual(-1.5950655381578688e+07, seg1.FinalState.Cartesian.Z * 0.001, 5);
        Assert.AreEqual(15.5622285573218395, seg1.FinalState.Cartesian.Vx * 0.001, 2e-6);
        Assert.AreEqual(-22.1946651504792278, seg1.FinalState.Cartesian.Vy * 0.001, 2e-6);
        Assert.AreEqual(-9.1904711443132641, seg1.FinalState.Cartesian.Vz * 0.001, 2e-6);
    }

    /*
      {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "$type": "SegmentResult",
      "TypeName": "InitialState",
      "Name": "初始段",
      "Description": "初始段参数",
      "UserComment": "初始段参数",
      "InitialState": {
        "Epoch": "2025-05-04T23:58:50.81500000000233Z",
        "CoordSystemName": "Sun Inertial",
        "Cartesian": {
          "X": -140832513600.80856,
          "Y": -46943598204.90579,
          "Z": -20737890753.103355,
          "Vx": 18453.28847424302,
          "Vy": -21359.103761752824,
          "Vz": -8819.00250469841
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 1.327124400419394E+20,
          "SemiMajorAxis": 148072556860.60834,
          "Eccentricity": 0.3190449959172957,
          "Inclination": 22.75383844505876,
          "RAAN": 358.97926750072645,
          "ArgOfPeriapsis": 311.6436922555014,
          "MeanAnomaly": 286.05938606534846,
          "TrueAnomaly": 249.31591838616114,
          "Period": 31076772.95019035
        },
        "Spherical": {
          "RightAscension": 198.4347390188808,
          "Declination": -7.952516931377445,
          "RadiusMagnitude": 149891822366.77628,
          "HorizFPA": -18.59232824358846,
          "VelocityAzimuth": 111.3883991127631,
          "VelocityMagnitude": 29572.114807649974
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2025-05-04T23:58:50.81500000000233Z",
        "CoordSystemName": "Sun Inertial",
        "Cartesian": {
          "X": -140832513600.80856,
          "Y": -46943598204.90579,
          "Z": -20737890753.103355,
          "Vx": 18453.28847424302,
          "Vy": -21359.103761752824,
          "Vz": -8819.00250469841
        },
        "Keplerian": null,
        "Spherical": null,
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "SegmentResult",
      "TypeName": "Propagate",
      "Name": "轨道递推段",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2025-05-04T23:58:50.81500000000233Z",
        "CoordSystemName": "Sun Inertial",
        "Cartesian": {
          "X": -140832513600.80856,
          "Y": -46943598204.90579,
          "Z": -20737890753.103355,
          "Vx": 18453.28847424302,
          "Vy": -21359.103761752824,
          "Vz": -8819.00250469841
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 1.327124400419394E+20,
          "SemiMajorAxis": 148072556860.60834,
          "Eccentricity": 0.3190449959172957,
          "Inclination": 22.75383844505876,
          "RAAN": 358.97926750072645,
          "ArgOfPeriapsis": 311.6436922555014,
          "MeanAnomaly": 286.05938606534846,
          "TrueAnomaly": 249.31591838616114,
          "Period": 31076772.95019035
        },
        "Spherical": {
          "RightAscension": 198.4347390188808,
          "Declination": -7.952516931377445,
          "RadiusMagnitude": 149891822366.77628,
          "HorizFPA": -18.59232824358846,
          "VelocityAzimuth": 111.3883991127631,
          "VelocityMagnitude": 29572.114807649974
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2029-04-06T13:00:00.000000002386514Z",
        "CoordSystemName": "Sun Inertial",
        "Cartesian": {
          "X": -149753317909.90842,
          "Y": -35387469439.04535,
          "Z": -15950657491.357347,
          "Vx": 15562.229819796137,
          "Vy": -22194.6648595458,
          "Vz": -9190.471030390818
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 1.327124400419394E+20,
          "SemiMajorAxis": 148039781826.9857,
          "Eccentricity": 0.3187039996726576,
          "Inclination": 22.751282941754436,
          "RAAN": 358.98463853635195,
          "ArgOfPeriapsis": 311.57317362378456,
          "MeanAnomaly": 279.9596140503153,
          "TrueAnomaly": 243.88932731787662,
          "Period": 31066455.5159303
        },
        "Spherical": {
          "RightAscension": 193.29539265729787,
          "Declination": -5.91803314163092,
          "RadiusMagnitude": 154702141849.20667,
          "HorizFPA": -18.4109589656044,
          "VelocityAzimuth": 112.00769287468229,
          "VelocityMagnitude": 28622.559336162834
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 123771669.185,
      "Results": {}
    }
  ],
     */

    [TestMethod()]
    public void Heliocentric_XF261_260422()
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
                  "SemiMajorAxis": 1.4807255686060825e11,
                  "Eccentricity": 0.3190449959172958,
                  "Inclination": 0.7938110241547416,
                  "RAAN": 209.8223416775454,
                  "ArgOfPeriapsis": 100.8824094686265,
                  "MeanAnomaly": 286.05938606534835,
                  "AnomalyType": "Mean"
                },
                "Epoch": "2025-05-04T23:58:50.815Z",
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
                    "Trip": "2029-04-06T13:00:00.000Z"
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
        Console.WriteLine("最后状态");
        Console.WriteLine(rv[id - 7]);
        Console.WriteLine(rv[id - 6]);
        Console.WriteLine(rv[id - 5]);
        Console.WriteLine(rv[id - 4]);
        Console.WriteLine(rv[id - 3]);
        Console.WriteLine(rv[id - 2]);
        Console.WriteLine(rv[id - 1]);
        /*
             0
            -140832513600.80856
            -46943598204.90579
            -20737890753.103355
            18453.28847424302
            -21359.103761752824
            最后状态
            123771669.185
            -149753317909.90842
            -35387469439.04535
            -15950657491.357347
            15562.229819796137
            -22194.6648595458
            -9190.471030390818
         */

        /*  STK结果 （Sun Inertial 坐标系）                
                                                                                                           
UTC Gregorian Date: 6 Apr 2029 13:00:00.000  UTC Julian Date: 2462233.04166667                        
Julian Ephemeris Date: 2462233.04246741                                                               
Time past epoch: 3.98772e+07 sec   (Epoch in UTC Gregorian Date: 1 Jan 2028 00:00:00.000)             
                                                                                                      
State Vector in Coordinate System: Sun Inertial                                                       
                                                                                                      
Parameter Set Type:  Cartesian                                                                        
         X:   -1.4975332214605790e+08 km              Vx:       15.5622285573218395 km/sec            
         Y:   -3.5387463911978923e+07 km              Vy:      -22.1946651504792278 km/sec            
         Z:   -1.5950655381578688e+07 km              Vz:       -9.1904711443132641 km/sec            
                                                                                                      
Parameter Set Type:  Keplerian                                                                        
       sma:    1.4803978261276379e+08 km            RAAN:         358.9846383767705 deg               
       ecc:        0.3187040020800770                  w:         311.5731733394218 deg               
       inc:         22.75128294439372 deg             TA:         243.8893252358499 deg               
                                                                                                      
Parameter Set Type:  Spherical                                                                        
 Right Asc:         193.2953902917368 deg     Horiz. FPA:        -18.41095901834386 deg               
      Decl:        -5.918032255523651 deg        Azimuth:         112.0076931046749 deg               
       |R|:    1.5470214446802115e+08 km             |V|:       28.6225589119260135 km/sec    
        */

        var seg1 = output.MainSequenceResults[1];
        //  末态(与STK对比）日心惯性系（单位: km, km/s）
        Assert.AreEqual(-1.4975332214605790e+08, seg1.FinalState.Cartesian.X * 0.001, 5);
        Assert.AreEqual(-3.5387463911978923e+07, seg1.FinalState.Cartesian.Y * 0.001, 6);
        Assert.AreEqual(-1.5950655381578688e+07, seg1.FinalState.Cartesian.Z * 0.001, 5);
        Assert.AreEqual(15.5622285573218395, seg1.FinalState.Cartesian.Vx * 0.001, 2e-6);
        Assert.AreEqual(-22.1946651504792278, seg1.FinalState.Cartesian.Vy * 0.001, 2e-6);
        Assert.AreEqual(-9.1904711443132641, seg1.FinalState.Cartesian.Vz * 0.001, 2e-6);
    }
}