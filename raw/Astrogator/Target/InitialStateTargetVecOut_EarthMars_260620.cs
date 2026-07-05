using System.Globalization;
using System.Text.Json;
using ASTROX.Time;

namespace ASTROX.Astrogator.Tests
{
    public partial class TargetTests
    {
        //  测试 初始段自变量为TargetVectorOutgoingAsymptote的情况
        //      先手动计算运行一段时间后的RMag,RA,Dec, 作为后面的约束参数
        //      然后再检查Target的收敛性,
        //      自变量:    TargetOut.C3,TargetOut.AsympRA,TargetOut.AsympDec
        [TestMethod()]
        public void InitialStateTargetVecOut_Smoke_260620()
        {
            const string truthJson = """
            {
              "CentralBody": "Earth",
              "ComputeCzmlPositions": false,
              "MainSequence": [
                {
                  "$type": "InitialState",
                  "Name": "InitialState",
                  "InitialState": {
                    "CoordSystemName": "Earth Inertial",
                    "Epoch": "2025-01-01T00:00:00.000Z",
                    "DryMass": 500, "FuelMass": 500, "Cd": 2.2, "Cr": 1.0, "DragArea": 20, "SRPArea": 20,
                    "Element": {
                      "$type": "TargetVecOut",
                      "GravitationalParameter": 3.986004415E14,
                      "RadiusOfPeriapsis": 6678.137,
                      "C3": 10.0,
                      "AsympRA": 350.0,
                      "AsympDec": 5.0,
                      "VelAzAtPeriapsis": 90.0,
                      "TrueAnomaly": 0.0
                    }
                  }
                },
                {
                  "$type": "Propagate",
                  "Name": "Propagate",
                  "PropagatorName": "Earth_Point_Mass",
                  "StopConditions": [ { "$type": "Duration", "Name": "Duration", "Active": true, "Trip": 10800.0 } ],
                  "Results": [
                    { "$type": "SphericalElement", "Name": "RMag", "CoordSystemName": "Earth Inertial", "ComponentName": "RadiusMagnitude" },
                    { "$type": "SphericalElement", "Name": "RA", "CoordSystemName": "Earth Inertial", "ComponentName": "RightAscension" },
                    { "$type": "SphericalElement", "Name": "Dec", "CoordSystemName": "Earth Inertial", "ComponentName": "Declination" }
                  ]
                }
              ]
            }
            """;

            var truthInput = JsonSerializer.Deserialize<AstrogatorMCS>(truthJson);
            var truthOut = truthInput!.RunMCS();
            if (!truthOut.IsSuccess)
                Assert.Fail(truthOut.Message);

            //  期望值取自真值传播段的Results字典(与微分修正约束使用同一标量定义,
            //  避免赤经0~360与-180~180的取值范围不一致导致约束差值异常)
            var truthProp = truthOut.MainSequenceResults[1];
            double desRMag = Convert.ToDouble(truthProp.Results["RMag"]);
            double desRA = Convert.ToDouble(truthProp.Results["RA"]);
            double desDec = Convert.ToDouble(truthProp.Results["Dec"]);

            const string dcJsonTemplate = """
            {
              "CentralBody": "Earth",
              "ComputeCzmlPositions": false,
              "MainSequence": [
                {
                  "$type": "TargetSequence",
                  "Name": "EarthMarsDeparture",
                  "Action": "RunActiveOperators",
                  "Segments": [
                    {
                      "$type": "InitialState",
                      "Name": "InitialState",
                      "InitialState": {
                        "CoordSystemName": "Earth Inertial",
                        "Epoch": "2025-01-01T00:00:00.000Z",
                        "DryMass": 500, "FuelMass": 500, "Cd": 2.2, "Cr": 1.0, "DragArea": 20, "SRPArea": 20,
                        "Element": {
                          "$type": "TargetVecOut",
                          "GravitationalParameter": 3.986004415E14,
                          "RadiusOfPeriapsis": 6678.137,
                          "C3": 9.0,
                          "AsympRA": 345.0,
                          "AsympDec": 2.0,
                          "VelAzAtPeriapsis": 90.0,
                          "TrueAnomaly": 0.0
                        }
                      }
                    },
                    {
                      "$type": "Propagate",
                      "Name": "Propagate",
                      "PropagatorName": "Earth_Point_Mass",
                      "StopConditions": [ { "$type": "Duration", "Name": "Duration", "Active": true, "Trip": 10800.0 } ],
                      "Results": [
                        { "$type": "SphericalElement", "Name": "RMag", "CoordSystemName": "Earth Inertial", "ComponentName": "RadiusMagnitude" },
                        { "$type": "SphericalElement", "Name": "RA", "CoordSystemName": "Earth Inertial", "ComponentName": "RightAscension" },
                        { "$type": "SphericalElement", "Name": "Dec", "CoordSystemName": "Earth Inertial", "ComponentName": "Declination" }
                      ]
                    }
                  ],
                  "Profiles": [
                    {
                      "$type": "DifferentialCorrector",
                      "Name": "DC: TargetVecOut",
                      "Active": true,
                      "MaximumIterations": 50,
                      "OnlyStoreFinalResults": false,
                      "ControlParameters": [
                        { "Enable": true, "Name": "InitialState.TargetVecOut.C3", "MaxStep": 2.0, "Perturbation": 0.01, "ParentName": "InitialState" },
                        { "Enable": true, "Name": "InitialState.TargetVecOut.AsympRA", "MaxStep": 5.0, "Perturbation": 0.01, "ParentName": "InitialState" },
                        { "Enable": true, "Name": "InitialState.TargetVecOut.AsympDec", "MaxStep": 5.0, "Perturbation": 0.01, "ParentName": "InitialState" }
                      ],
                      "Results": [
                        { "Enable": true, "Name": "RMag", "ParentName": "Propagate", "DesiredValue": "<<RMAG>>", "Tolerance": 500.0 },
                        { "Enable": true, "Name": "RA", "ParentName": "Propagate", "DesiredValue": "<<RA>>", "Tolerance": 0.001 },
                        { "Enable": true, "Name": "Dec", "ParentName": "Propagate", "DesiredValue": "<<DEC>>", "Tolerance": 0.001 }
                      ]
                    }
                  ]
                }
              ]
            }
            """;

            string dcJson = dcJsonTemplate
                .Replace("<<RMAG>>", desRMag.ToString(CultureInfo.InvariantCulture))
                .Replace("<<RA>>", desRA.ToString(CultureInfo.InvariantCulture))
                .Replace("<<DEC>>", desDec.ToString(CultureInfo.InvariantCulture));

            var dcInput = JsonSerializer.Deserialize<AstrogatorMCS>(dcJson);
            var dcOut = dcInput!.RunMCS();
            if (!dcOut.IsSuccess)
                Assert.Fail(dcOut.Message);

            Console.WriteLine(JsonSerializer.Serialize(dcOut, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            var ts = dcOut.MainSequenceResults[0] as MCSTargetSequenceResults;
            var dcr = ts!.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults;
            Assert.IsTrue(dcr!.Converged, "DC 未收敛");

            double GetFinal(string nm)
            {
                var c = dcr.ControlParameters.First(x => x.Name == nm);
                return double.Parse(c.FinalValue, CultureInfo.InvariantCulture);
            }

            Assert.AreEqual(10.0, GetFinal("InitialState.TargetVecOut.C3"), 0.1);
            Assert.AreEqual(350.0, GetFinal("InitialState.TargetVecOut.AsympRA"), 0.1);
            Assert.AreEqual(5.0, GetFinal("InitialState.TargetVecOut.AsympDec"), 0.1);
            Assert.IsTrue(ts.SegmentResults[0].InitialState.Keplerian.Eccentricity > 1.0, "应为双曲轨道");
        }

        //  地球→火星转移(2020-07-20出发,2021-02-09到达)
        //  积分器: Heliocentric; 停止: 火星近地点; 约束: B平面(Bt=3000km,Br=0) + 历元2021-02-09
        //  自变量: TargetVecOut.C3, AsympRA, AsympDec
        [TestMethod()]
        public void InitialStateTargetVecOut_Earth2Mars_260620()
        {
            const double desBDotT = 3_000_000.0;   // Bt = 3000 km
            const double desBDotR = 0.0;            // Br = 0 km
            const string desEpoch = "2021-02-09T00:00:00.000Z";

            const string dcJson = """
            {
              "CentralBody": "Earth",
              "ComputeCzmlPositions": false,
              "MainSequence": [
                {
                  "$type": "TargetSequence",
                  "Name": "Earth2Mars",
                  "Action": "RunActiveOperators",
                  "Segments": [
                    {
                      "$type": "InitialState",
                      "Name": "InitialState",
                      "InitialState": {
                        "CoordSystemName": "Earth Inertial",
                        "Epoch": "2020-07-20T00:00:00.000Z",
                        "DryMass": 500, "FuelMass": 500, "Cd": 2.2, "Cr": 1.0, "DragArea": 20, "SRPArea": 20,
                        "Element": {
                          "$type": "TargetVecOut",
                          "GravitationalParameter": 3.986004415E14,
                          "RadiusOfPeriapsis": 6678.137,
                          "C3": 13.17,
                          "AsympRA": 12.71,
                          "AsympDec": 22.57,
                          "VelAzAtPeriapsis": 90.0,
                          "TrueAnomaly": 0.0
                        }
                      }
                    },
                    {
                      "$type": "Propagate",
                      "Name": "Prop2Mars",
                      "PropagatorName": "Heliocentric",
                      "MaxPropagationTime": 86400000,
                      "StopConditions": [
                        {
                          "$type": "Periapsis",
                          "Name": "MarsPeriapsis",
                          "Active": true,
                          "Tolerance": 0.000001,
                          "CentralBodyName": "Mars",
                          "Mu": 4.2828375641E13
                        }
                      ],
                      "Results": [
                        { "$type": "Epoch", "Name": "Epoch" },
                        {
                          "$type": "BPlane",
                          "Name": "BDotT",
                          "CentralBodyName": "Mars",
                          "ComponentName": "BDotT",
                          "Mu": 4.2828375641E13
                        },
                        {
                          "$type": "BPlane",
                          "Name": "BDotR",
                          "CentralBodyName": "Mars",
                          "ComponentName": "BDotR",
                          "Mu": 4.2828375641E13
                        }
                      ]
                    }
                  ],
                  "Profiles": [
                    {
                      "$type": "DifferentialCorrector",
                      "Name": "DC: Earth2Mars B-Plane",
                      "Active": true,
                      "MaximumIterations": 150,
                      "OnlyStoreFinalResults": false,
                      "ControlParameters": [
                        { "Enable": true, "Name": "InitialState.TargetVecOut.C3", "MaxStep": 1.0, "Perturbation": 0.02, "ParentName": "InitialState" },
                        { "Enable": true, "Name": "InitialState.TargetVecOut.AsympRA", "MaxStep": 3.0, "Perturbation": 0.02, "ParentName": "InitialState" },
                        { "Enable": true, "Name": "InitialState.TargetVecOut.AsympDec", "MaxStep": 3.0, "Perturbation": 0.02, "ParentName": "InitialState" }
                      ],
                      "Results": [
                        { "Enable": true, "Name": "BDotT", "ParentName": "Prop2Mars", "DesiredValue": "3000000.0", "Tolerance": 1000.0 },
                        { "Enable": true, "Name": "BDotR", "ParentName": "Prop2Mars", "DesiredValue": "0.0", "Tolerance": 1000.0 },
                        { "Enable": true, "Name": "Epoch", "ParentName": "Prop2Mars", "DesiredValue": "2021-02-09T00:00:00.000Z", "Tolerance": 1.0 }
                      ]
                    }
                  ]
                }
              ]
            }
            """;

            var dcInput = JsonSerializer.Deserialize<AstrogatorMCS>(dcJson);
            var dcOut = dcInput!.RunMCS();
            if (!dcOut.IsSuccess)
                Assert.Fail(dcOut.Message);

            Console.WriteLine(JsonSerializer.Serialize(dcOut, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            var ts = dcOut.MainSequenceResults[0] as MCSTargetSequenceResults;
            var dcr = ts!.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults;
            Assert.IsTrue(dcr!.Converged, "DC 未收敛");

            var prop = ts.SegmentResults[1];
            Assert.IsTrue(ts.SegmentResults[0].InitialState.Epoch.StartsWith("2020-07-20", StringComparison.Ordinal));

            double GetResult(string nm)
            {
                var r = dcr.Results.First(x => x.Name == nm);
                return double.Parse(r.CurrentValue, CultureInfo.InvariantCulture);
            }

            var epochResult = dcr.Results.First(x => x.Name == "Epoch");

            Assert.AreEqual(desBDotT, GetResult("BDotT"), 1000.0);
            Assert.AreEqual(desBDotR, GetResult("BDotR"), 1000.0);
            Assert.IsTrue(Math.Abs(epochResult.Difference) < 1.0, $"历元约束残差应<1s, 实际={epochResult.Difference}s");

            var desJd = GregorianDate.Parse(desEpoch).ToJulianDate();
            var finalJd = GregorianDate.Parse(prop.FinalState.Epoch).ToJulianDate();
            Assert.IsTrue(Math.Abs((finalJd - desJd).TotalSeconds) < 1.0, $"末态历元应接近{desEpoch}, 实际={prop.FinalState.Epoch}");

            double GetFinalCtrl(string nm)
            {
                var c = dcr.ControlParameters.First(x => x.Name == nm);
                return double.Parse(c.FinalValue, CultureInfo.InvariantCulture);
            }

            double c3 = GetFinalCtrl("InitialState.TargetVecOut.C3");
            Console.WriteLine($"Final C3={c3}, AsympRA={GetFinalCtrl("InitialState.TargetVecOut.AsympRA")}, AsympDec={GetFinalCtrl("InitialState.TargetVecOut.AsympDec")}");
            Assert.IsTrue(c3 > 0.0, "应为双曲出发轨道(C3>0)");
            Assert.IsTrue(ts.SegmentResults[0].InitialState.Keplerian.Eccentricity > 1.0, "应为双曲轨道");

            //  Final C3=13.30911152072679, AsympRA=15.39134089649878, AsympDec=27.728930644318854

            //  STK 结果: C3: 12.3086,  AsympRA: 15.4002, AsympDec: 27.7231
            //  Heliocentric积分器有些不一样
        }
    }
}
