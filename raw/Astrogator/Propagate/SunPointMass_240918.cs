using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator
                MCS:
                >   Initial_State   Sun惯性系Cartesian
                >   Propagate       Sun_point_mass积分器

            初始轨道根数：  
                半长径：    1.5E11 (m)
                偏心率：    0.02
                轨道倾角：  5.0  (deg)
                近点角距：  20    (deg) 
                升交点经度：10    (deg) 
                平近点角：  30    (deg)

       
            与STK结果对比:   10位有效位数

            20240730    初次创建
        */
        [TestMethod()]
        public void SunPointMass_240918()
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
                      "SemiMajorAxis": 1.5E11,
                      "Eccentricity": 0.02,
                      "Inclination": 5.0,
                      "RAAN": 10,
                      "ArgOfPeriapsis": 20,
                      "TrueAnomaly": 30
                    },
                    "Epoch": "2018-12-01T00:00:00.000Z",
                    "FuelMass": 500,
                    "SRPArea": 20
                  }
                },
                {
                  "$type": "Propagate",
                  "Name": "轨道递推段",
                  "PropagatorName": "Sun_Point_Mass",
                  "StopConditions": [
                    {
                      "$type": "Duration",
                      "Name": "Duration",
                      "Active": true,
                      "Description": "积分固定的时长",
                      "Trip": 8640000
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

            /*
                STK结果
            Parameter Set Type:  Cartesian                                                                 
                     X:   -1.4155030167677242e+08 km              Vx:      -11.0553086084558103 km/sec     
                     Y:    5.4528242654077999e+07 km              Vy:      -27.1353527207867415 km/sec     
                     Z:    6.8485940383082414e+06 km              Vz:       -2.1700138275618954 km/sec     
                                                                                               
            Parameter Set Type:  Keplerian                                                                 
                   sma:    1.5000000000000018e+08 km            RAAN:         10.00000000000001 deg        
                   ecc:        0.0200000000000008                  w:         19.99999999999954 deg        
                   inc:         4.999999999999996 deg             TA:         128.8355903003875 deg   
            */

            //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
            //  标准值为STK 的计算结果, 86400s 的数值
            //  第多个数值，(与STK中的时间一致)
            int id = output.Position.cartesianVelocity.Length;
            //Assert.AreEqual(86400, output.Position.cartesianVelocity[id - 7], 0.1);
            Assert.AreEqual(-1.4155030167677242e+08, output.Position.cartesianVelocity[id - 6] * 0.001, 1e-3);
            Assert.AreEqual(5.4528242654077999e+07, output.Position.cartesianVelocity[id - 5] * 0.001, 1e-3);
            Assert.AreEqual(6.8485940383082414e+06, output.Position.cartesianVelocity[id - 4] * 0.001, 1e-3);
            Assert.AreEqual(-11.0553086084558103, output.Position.cartesianVelocity[id - 3] * 0.001, 1e-6);
            Assert.AreEqual(-27.1353527207867415, output.Position.cartesianVelocity[id - 2] * 0.001, 1e-6);
            Assert.AreEqual(-2.1700138275618954, output.Position.cartesianVelocity[id - 1] * 0.001, 1e-6);
        }
    }
}