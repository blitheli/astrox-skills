using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.Celestial;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         Target: Moon L2 系初值 + Propagate(穿越 Z-X 平面, Decreasing)
         微分修正 InitialState.Vy, 约束末态 L2_Vx = 0
        */
        [TestMethod()]
        public void EarthMoonL2_Target_250704()
        {
            PlanetsEphemeris.UseJplDe430File();

            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent!.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");
            string fp = Path.Combine(filePath0, "EarthMoonL2_250704.json");

            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            input!.ComputeCzmlPositions = false;
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            Console.WriteLine(JsonSerializer.Serialize(output,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var dcResult = seg!.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults;

            var ctrl = dcResult!.ControlParameters[0];
            var cstr = dcResult.Results[0];
            double finalVy = double.Parse(ctrl.FinalValue, CultureInfo.InvariantCulture);
            double finalVx = double.Parse(cstr.CurrentValue, CultureInfo.InvariantCulture);

            Console.WriteLine($"Target EarthMoonL2_250704.json: Converged={dcResult.Converged}, Iterations={dcResult.TotalIterations}");
            Console.WriteLine($"  Vy={finalVy} m/s, L2_Vx={finalVx} m/s");

            Assert.IsTrue(dcResult.Converged, "Target 未收敛");
            Assert.IsTrue(Math.Abs(finalVx) < 0.01, $"L2_Vx 应 < 0.01 m/s, 实际 {finalVx} m/s");
            /*
         {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "$type": "TargetSequenceResult",
      "OperatorResults": [
        {
          "$type": "DifferentialCorrectorResults",
          "Converged": true,
          "TotalIterations": 3,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "-140.99999999999983",
              "FinalValue": "-140.99264334419777",
              "Correction": 0.007356655802060923,
              "LastUpdate": -0.00034648019276283405,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "初始段",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": "",
              "Values": [
                -140.99999999999983,
                -140.992296864005,
                -140.99264334419777
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "L2_Vx",
              "DesiredValue": "0.0",
              "ParentName": "递推至CrossZX",
              "CurrentValue": "-0.002322450194164105",
              "Unit": "",
              "Difference": -0.002322450194164105,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.01,
              "Weight": 1,
              "Values": [
                -1.074196206736211,
                0.04796857437969493,
                -0.002322450194164105
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.Vy_L2_Vx",
          "Description": "描述",
          "UserComment": "用户注释"
        }
      ],
      "SegmentResults": [
        {
          "TypeName": "InitialState",
          "Name": "初始段",
          "Description": "初始段参数",
          "UserComment": "初始段参数",
          "InitialState": {
            "Epoch": "2028-01-01T00:00:00.000Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 390709025.80583835,
              "Y": -270337910.7576046,
              "Z": -65670447.93081723,
              "Vx": 568.2013951127739,
              "Vy": 719.1996289380364,
              "Vz": 403.85214759224596
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 604943561.2590755,
              "Eccentricity": 0.2071537362587722,
              "Inclination": 25.22067924590635,
              "RAAN": 342.3850177131233,
              "ArgOfPeriapsis": 340.5249246430217,
              "MeanAnomaly": 0.47040254450499935,
              "TrueAnomaly": 0.7320882256171464,
              "AnomalyType": "True",
              "Period": 4682559.11293244
            },
            "Spherical": {
              "RightAscension": 325.32001188001976,
              "Declination": -7.869534180375285,
              "RadiusMagnitude": 479633961.02691483,
              "HorizFPA": 0.12562821243379624,
              "VelocityAzimuth": 65.96181250350281,
              "VelocityMagnitude": 1001.5974684410804
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -7.741131104382526,
            "Geodetic_Longitude": -134.4923007794158,
            "Geodetic_Altitude": 473256211.3459408,
            "Geocentric_Latitude": -7.740450292681372,
            "Geocentric_Longitude": -134.4923007794158
          },
          "FinalState": {
            "Epoch": "2028-01-01T00:00:00.000Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 390709025.80583835,
              "Y": -270337910.7576046,
              "Z": -65670447.93081723,
              "Vx": 568.2013951127739,
              "Vy": 719.1996289380364,
              "Vz": 403.85214759224596
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 604943561.2590755,
              "Eccentricity": 0.2071537362587722,
              "Inclination": 25.22067924590635,
              "RAAN": 342.3850177131233,
              "ArgOfPeriapsis": 340.5249246430217,
              "MeanAnomaly": 0.47040254450499935,
              "TrueAnomaly": 0.7320882256171464,
              "AnomalyType": "True",
              "Period": 4682559.11293244
            },
            "Spherical": {
              "RightAscension": 325.32001188001976,
              "Declination": -7.869534180375285,
              "RadiusMagnitude": 479633961.02691483,
              "HorizFPA": 0.12562821243379624,
              "VelocityAzimuth": 65.96181250350281,
              "VelocityMagnitude": 1001.5974684410804
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -7.741131104382526,
            "Geodetic_Longitude": -134.4923007794158,
            "Geodetic_Altitude": 473256211.3459408,
            "Geocentric_Latitude": -7.740450292681372,
            "Geocentric_Longitude": -134.4923007794158
          },
          "DurationSec": 0,
          "Results": {}
        },
        {
          "$type": "PropagateResult",
          "StoppedOnMaximumDuration": false,
          "StoppingConditionName": "CrossZX",
          "TypeName": "Propagate",
          "Name": "递推至CrossZX",
          "Description": "轨道递推段",
          "UserComment": "轨道递推段",
          "InitialState": {
            "Epoch": "2028-01-01T00:00:00.000Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 390709025.80583835,
              "Y": -270337910.7576046,
              "Z": -65670447.93081723,
              "Vx": 568.2013951127739,
              "Vy": 719.1996289380364,
              "Vz": 403.85214759224596
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 604943561.2590755,
              "Eccentricity": 0.2071537362587722,
              "Inclination": 25.22067924590635,
              "RAAN": 342.3850177131233,
              "ArgOfPeriapsis": 340.5249246430217,
              "MeanAnomaly": 0.47040254450499935,
              "TrueAnomaly": 0.7320882256171464,
              "AnomalyType": "True",
              "Period": 4682559.11293244
            },
            "Spherical": {
              "RightAscension": 325.32001188001976,
              "Declination": -7.869534180375285,
              "RadiusMagnitude": 479633961.02691483,
              "HorizFPA": 0.12562821243379624,
              "VelocityAzimuth": 65.96181250350281,
              "VelocityMagnitude": 1001.5974684410804
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -7.741131104382526,
            "Geodetic_Longitude": -134.4923007794158,
            "Geodetic_Altitude": 473256211.3459408,
            "Geocentric_Latitude": -7.740450292681372,
            "Geocentric_Longitude": -134.4923007794158
          },
          "FinalState": {
            "Epoch": "2028-01-15T04:46:12.475645Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -394795978.9982596,
              "Y": 153871682.31858128,
              "Z": 71483433.77863085,
              "Vx": -482.28980770021553,
              "Vy": -902.9542399385349,
              "Vz": -465.0984082371573
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 674493507.8273487,
              "Eccentricity": 0.36461253808836025,
              "Inclination": 26.868714728603262,
              "RAAN": 358.15661719603787,
              "ArgOfPeriapsis": 150.30509211564785,
              "MeanAnomaly": 3.517104408132956,
              "TrueAnomaly": 8.097649532054657,
              "AnomalyType": "True",
              "Period": 5512868.603480941
            },
            "Spherical": {
              "RightAscension": 158.70665669687003,
              "Declination": 9.575838212570355,
              "RadiusMagnitude": 429709484.36999685,
              "HorizFPA": 2.1611601690129834,
              "VelocityAzimuth": 115.22387170234848,
              "VelocityMagnitude": 1124.3870985471265
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 9.430020345618686,
            "Geodetic_Longitude": -26.654760003225775,
            "Geodetic_Altitude": 423331920.43877643,
            "Geocentric_Latitude": 9.429100087699574,
            "Geocentric_Longitude": -26.654760003225775
          },
          "DurationSec": 1226772.475644506,
          "Results": {
            "L2_Vx": -0.002322450194164105,
            "L2_Vy": -160.93889159782154,
            "L2_Y": -0.009583448991179466
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Target_L2_Vy_Vx",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 390709025.80583835,
          "Y": -270337910.7576046,
          "Z": -65670447.93081723,
          "Vx": 568.2013951127739,
          "Vy": 719.1996289380364,
          "Vz": 403.85214759224596
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 604943561.2590755,
          "Eccentricity": 0.2071537362587722,
          "Inclination": 25.22067924590635,
          "RAAN": 342.3850177131233,
          "ArgOfPeriapsis": 340.5249246430217,
          "MeanAnomaly": 0.47040254450499935,
          "TrueAnomaly": 0.7320882256171464,
          "AnomalyType": "True",
          "Period": 4682559.11293244
        },
        "Spherical": {
          "RightAscension": 325.32001188001976,
          "Declination": -7.869534180375285,
          "RadiusMagnitude": 479633961.02691483,
          "HorizFPA": 0.12562821243379624,
          "VelocityAzimuth": 65.96181250350281,
          "VelocityMagnitude": 1001.5974684410804
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -7.741131104382526,
        "Geodetic_Longitude": -134.4923007794158,
        "Geodetic_Altitude": 473256211.3459408,
        "Geocentric_Latitude": -7.740450292681372,
        "Geocentric_Longitude": -134.4923007794158
      },
      "FinalState": {
        "Epoch": "2028-01-15T04:46:12.475645Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -394795978.9982596,
          "Y": 153871682.31858128,
          "Z": 71483433.77863085,
          "Vx": -482.28980770021553,
          "Vy": -902.9542399385349,
          "Vz": -465.0984082371573
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 674493507.8273487,
          "Eccentricity": 0.36461253808836025,
          "Inclination": 26.868714728603262,
          "RAAN": 358.15661719603787,
          "ArgOfPeriapsis": 150.30509211564785,
          "MeanAnomaly": 3.517104408132956,
          "TrueAnomaly": 8.097649532054657,
          "AnomalyType": "True",
          "Period": 5512868.603480941
        },
        "Spherical": {
          "RightAscension": 158.70665669687003,
          "Declination": 9.575838212570355,
          "RadiusMagnitude": 429709484.36999685,
          "HorizFPA": 2.1611601690129834,
          "VelocityAzimuth": 115.22387170234848,
          "VelocityMagnitude": 1124.3870985471265
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 9.430020345618686,
        "Geodetic_Longitude": -26.654760003225775,
        "Geodetic_Altitude": 423331920.43877643,
        "Geocentric_Latitude": 9.429100087699574,
        "Geocentric_Longitude": -26.654760003225775
      },
      "DurationSec": 1226772.475644506,
      "Results": {}
    }
  ],
  "Positions": null
}
Target EarthMoonL2_250704.json: Converged=True, Iterations=3
  Vy=-140.99264334419777 m/s, L2_Vx=-0.002322450194164105 m/s
             */
        }
    }
}
