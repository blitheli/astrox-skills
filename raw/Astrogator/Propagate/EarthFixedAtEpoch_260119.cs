using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator        
                MCS:
                >   Initial_State   地球FixedAtEpoch坐标系(用于星箭分离时刻)
                >   Propagate      
               
            与STK结果对比:   1e-6 deg
        */
        [TestMethod()]
        public void EarthFixedAtEpoch_260119()
        {

            string inputStr = """
                {
                  "CentralBody": "Earth",
                  "MainSequence": [
                    {
                      "$type": "InitialState",
                      "Name": "初始段",
                      "Text": "星箭分离时的轨道参数",
                      "InitialState": {
                        "Cd": 2.2,
                        "CoordSystemName": "Earth FixedAtEpoch",
                        "Cr": 1.0,
                        "DragArea": 20,
                        "DryMass": 500,
                        "Element": {
                          "$type": "Keplerian",                          
                          "GravitationalParameter": 3.986004415E14,
                          "SemiMajorAxis": 16569248.4,
                          "Eccentricity": 0.602980143,
                          "Inclination": 53.1305,
                          "RAAN": 303.821,
                          "ArgOfPeriapsis": 224.9991,
                          "TrueAnomaly": 13.6192
                        },
                        "Epoch": "1 Nov 2026 18:04:00.000",
                        "FuelMass": 500,
                        "SRPArea": 20
                      }
                    },
                    {
                      "$type": "Propagate",
                      "Name": "轨道递推段",
                      "PropagatorName": "CisLunar",
                        "StopConditions": [
                          {
                            "$type": "Apoapsis",
                            "Name": "Apoapsis",
                            "Active": true,
                            "Tolerance": 0.000001,
                            "CentralBodyName" : "Earth",
                            "Mu": 3.986004415E14
                          }
                        ]
                    }
                  ]
                }                
                """;
            //  读取json文件，并序列化为类对象            
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();
                       
            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            /*  STK 结果
State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
Parameter Set Type:  Cartesian                                                                 
         X:    -2443.4006663155973911 km              Vx:       -5.3632702589302088 km/sec     
         Y:     4203.7645994599879486 km              Vy:       -6.7464317678387582 km/sec     
         Z:    -4534.6005909837622312 km              Vz:       -4.6442072193492461 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:    16569.2483999999349180 km            RAAN:         255.4872777707735 deg        
       ecc:        0.6029801429999989                  w:         225.0482156962746 deg        
       inc:         52.98539739531835 deg             TA:         13.61919999999996 deg   
            */
            var seg0 = output.MainSequenceResults[0];

            Assert.AreEqual(16569248.3999999349180, seg0.FinalState.Keplerian.SemiMajorAxis, 0.2);
            Assert.AreEqual(0.6029801429999989, seg0.FinalState.Keplerian.Eccentricity, 1e-10);
            Assert.AreEqual(52.98539739531835, seg0.FinalState.Keplerian.Inclination, 1e-8);
            Assert.AreEqual(255.4872777707735, seg0.FinalState.Keplerian.RAAN, 1e-8);
            Assert.AreEqual(225.0482156962746, seg0.FinalState.Keplerian.ArgOfPeriapsis, 1e-8);
            Assert.AreEqual(13.61919999999996, seg0.FinalState.Keplerian.TrueAnomaly, 1e-8);

            //  远地点(和自己比较)
            var seg1 = output.MainSequenceResults[1];
            Assert.AreEqual(6236664.5768538974, seg1.FinalState.Cartesian.X, 1e-6);
            Assert.AreEqual(-21031193.830986511, seg1.FinalState.Cartesian.Y, 1e-6);
            Assert.AreEqual(15032434.156811941, seg1.FinalState.Cartesian.Z, 1e-6);
            Assert.AreEqual(1437.8956916341854, seg1.FinalState.Cartesian.Vx, 1e-9);
            Assert.AreEqual(1409.3850602072262, seg1.FinalState.Cartesian.Vy, 1e-9);
            Assert.AreEqual(1375.251484663476, seg1.FinalState.Cartesian.Vz, 1e-9);
        }
    }
}