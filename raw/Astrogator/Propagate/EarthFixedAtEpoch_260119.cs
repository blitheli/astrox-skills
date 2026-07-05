using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class PropagateTests
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
            //  FixedAtEpoch 依赖地固系姿态；清空 EOP 使本算例与自对比基准一致（不受前序 HPOP 测试污染）
            AstrogatorTestEnvironment.ResetEarthOrientationParameters();

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

            //  远地点(和自己比较；基准在空 EOP + JplDE430 下录制)
            //  Linux和windows平台运行略有不同，这里误差给大一点
            var seg1 = output.MainSequenceResults[1];
            Assert.AreEqual(6236664.576836813, seg1.FinalState.Cartesian.X, 1e-4);
            Assert.AreEqual(-21031193.831003122, seg1.FinalState.Cartesian.Y, 1e-4);
            Assert.AreEqual(15032434.156795576, seg1.FinalState.Cartesian.Z, 1e-4);
            Assert.AreEqual(1437.8956916357572, seg1.FinalState.Cartesian.Vx, 1e-7);
            Assert.AreEqual(1409.385060201948, seg1.FinalState.Cartesian.Vy, 1e-7);
            Assert.AreEqual(1375.2514846672584, seg1.FinalState.Cartesian.Vz, 1e-7);
        }
    }
}