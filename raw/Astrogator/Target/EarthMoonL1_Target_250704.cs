using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.Celestial;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    [TestClass]
    public partial class AstrogatorTargetTests
    {
        /*
         Target: Moon L1 系初值 + Propagate(穿越 Z-X 平面, Increasing)
         微分修正 InitialState.Vy, 约束末态 L1_Vx = 0
        */
        [TestMethod()]
        public void EarthMoonL1_Target_250704()
        {
            PlanetsEphemeris.UseJplDe430File();

            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent!.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");
            string fp = Path.Combine(filePath0, "EarthMoonL1_250704.json");

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

            Console.WriteLine($"Target EarthMoonL1_250704.json: Converged={dcResult.Converged}, Iterations={dcResult.TotalIterations}");
            Console.WriteLine($"  Vy={finalVy} m/s, L1_Vx={finalVx} m/s");

            Assert.IsTrue(dcResult.Converged, "Target 未收敛");
            Assert.IsTrue(Math.Abs(finalVx) < 0.01, $"L1_Vx 应 < 0.01 m/s, 实际 {finalVx} m/s");
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
          "TotalIterations": 4,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "203.99999999999983",
              "FinalValue": "203.75729964152615",
              "Correction": -0.24270035847368535,
              "LastUpdate": -0.0006020466653089418,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "初始段",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": "",
              "Values": [
                203.99999999999983,
                203.77856527634452,
                203.75790168819145,
                203.75729964152615
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "L1_Vx",
              "DesiredValue": "0.0",
              "ParentName": "递推至CrossZX",
              "CurrentValue": "0.002022896749110714",
              "Unit": "",
              "Difference": 0.002022896749110714,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.01,
              "Weight": 1,
              "Values": [
                35.386546973418874,
                2.934407454426733,
                0.08463799085066626,
                0.002022896749110714
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.Vy_L1_Vx",
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
              "X": 276335220.3882799,
              "Y": -195517993.9106646,
              "Z": -38753707.5707695,
              "Vx": 574.6425703121708,
              "Vy": 728.34181659321,
              "Vz": 408.91774579127366
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 303847817.5897047,
              "Eccentricity": 0.12136167905846111,
              "Inclination": 24.786084114506785,
              "RAAN": 339.07374291461895,
              "ArgOfPeriapsis": 164.9099615130555,
              "MeanAnomaly": 179.17400448553573,
              "TrueAnomaly": 179.34797119406204,
              "AnomalyType": "True",
              "Period": 1666844.7928585364
            },
            "Spherical": {
              "RightAscension": 324.7190974490735,
              "Declination": -6.5309870293657415,
              "RadiusMagnitude": 340720251.5559805,
              "HorizFPA": 0.09005847791348712,
              "VelocityAzimuth": 66.03722286033428,
              "VelocityMagnitude": 1013.8587713465283
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -6.403654171763277,
            "Geodetic_Longitude": -135.09520282494998,
            "Geodetic_Altitude": 334342380.09656125,
            "Geocentric_Latitude": -6.402858324840253,
            "Geocentric_Longitude": -135.09520282494998
          },
          "FinalState": {
            "Epoch": "2028-01-01T00:00:00.000Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 276335220.3882799,
              "Y": -195517993.9106646,
              "Z": -38753707.5707695,
              "Vx": 574.6425703121708,
              "Vy": 728.34181659321,
              "Vz": 408.91774579127366
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 303847817.5897047,
              "Eccentricity": 0.12136167905846111,
              "Inclination": 24.786084114506785,
              "RAAN": 339.07374291461895,
              "ArgOfPeriapsis": 164.9099615130555,
              "MeanAnomaly": 179.17400448553573,
              "TrueAnomaly": 179.34797119406204,
              "AnomalyType": "True",
              "Period": 1666844.7928585364
            },
            "Spherical": {
              "RightAscension": 324.7190974490735,
              "Declination": -6.5309870293657415,
              "RadiusMagnitude": 340720251.5559805,
              "HorizFPA": 0.09005847791348712,
              "VelocityAzimuth": 66.03722286033428,
              "VelocityMagnitude": 1013.8587713465283
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -6.403654171763277,
            "Geodetic_Longitude": -135.09520282494998,
            "Geodetic_Altitude": 334342380.09656125,
            "Geocentric_Latitude": -6.402858324840253,
            "Geocentric_Longitude": -135.09520282494998
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
              "X": 276335220.3882799,
              "Y": -195517993.9106646,
              "Z": -38753707.5707695,
              "Vx": 574.6425703121708,
              "Vy": 728.34181659321,
              "Vz": 408.91774579127366
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 303847817.5897047,
              "Eccentricity": 0.12136167905846111,
              "Inclination": 24.786084114506785,
              "RAAN": 339.07374291461895,
              "ArgOfPeriapsis": 164.9099615130555,
              "MeanAnomaly": 179.17400448553573,
              "TrueAnomaly": 179.34797119406204,
              "AnomalyType": "True",
              "Period": 1666844.7928585364
            },
            "Spherical": {
              "RightAscension": 324.7190974490735,
              "Declination": -6.5309870293657415,
              "RadiusMagnitude": 340720251.5559805,
              "HorizFPA": 0.09005847791348712,
              "VelocityAzimuth": 66.03722286033428,
              "VelocityMagnitude": 1013.8587713465283
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -6.403654171763277,
            "Geodetic_Longitude": -135.09520282494998,
            "Geodetic_Altitude": 334342380.09656125,
            "Geocentric_Latitude": -6.402858324840253,
            "Geocentric_Longitude": -135.09520282494998
          },
          "FinalState": {
            "Epoch": "2028-01-13T03:59:23.108408Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -178747202.74565253,
              "Y": 210851902.49282512,
              "Z": 120008645.92437139,
              "Vx": -912.3839612852448,
              "Vy": -564.6501007520749,
              "Vz": -378.1545631678406
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 295004714.0631598,
              "Eccentricity": 0.021864493662897725,
              "Inclination": 31.179827526791822,
              "RAAN": 356.1325074015942,
              "ArgOfPeriapsis": 299.5424070853865,
              "MeanAnomaly": 190.62473283938996,
              "TrueAnomaly": 190.1748832957787,
              "AnomalyType": "True",
              "Period": 1594609.7474519643
            },
            "Spherical": {
              "RightAscension": 130.28921644540307,
              "Declination": 23.4680860732756,
              "RadiusMagnitude": 301348904.71153265,
              "HorizFPA": -0.22616731411905588,
              "VelocityAzimuth": 111.14072425536654,
              "VelocityMagnitude": 1137.6621215169923
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": 23.369562164060795,
            "Geodetic_Longitude": -41.32063034977243,
            "Geodetic_Altitude": 294974127.20776147,
            "Geocentric_Latitude": 23.366604620648367,
            "Geocentric_Longitude": -41.32063034977243
          },
          "DurationSec": 1051163.10840802,
          "Results": {
            "L1_Vx": 0.002022896749110714,
            "L1_Vy": 221.38351461604603,
            "L1_Y": 0.07376258634030819
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Target_L1_Vy_Vx",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 276335220.3882799,
          "Y": -195517993.9106646,
          "Z": -38753707.5707695,
          "Vx": 574.6425703121708,
          "Vy": 728.34181659321,
          "Vz": 408.91774579127366
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 303847817.5897047,
          "Eccentricity": 0.12136167905846111,
          "Inclination": 24.786084114506785,
          "RAAN": 339.07374291461895,
          "ArgOfPeriapsis": 164.9099615130555,
          "MeanAnomaly": 179.17400448553573,
          "TrueAnomaly": 179.34797119406204,
          "AnomalyType": "True",
          "Period": 1666844.7928585364
        },
        "Spherical": {
          "RightAscension": 324.7190974490735,
          "Declination": -6.5309870293657415,
          "RadiusMagnitude": 340720251.5559805,
          "HorizFPA": 0.09005847791348712,
          "VelocityAzimuth": 66.03722286033428,
          "VelocityMagnitude": 1013.8587713465283
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -6.403654171763277,
        "Geodetic_Longitude": -135.09520282494998,
        "Geodetic_Altitude": 334342380.09656125,
        "Geocentric_Latitude": -6.402858324840253,
        "Geocentric_Longitude": -135.09520282494998
      },
      "FinalState": {
        "Epoch": "2028-01-13T03:59:23.108408Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -178747202.74565253,
          "Y": 210851902.49282512,
          "Z": 120008645.92437139,
          "Vx": -912.3839612852448,
          "Vy": -564.6501007520749,
          "Vz": -378.1545631678406
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 295004714.0631598,
          "Eccentricity": 0.021864493662897725,
          "Inclination": 31.179827526791822,
          "RAAN": 356.1325074015942,
          "ArgOfPeriapsis": 299.5424070853865,
          "MeanAnomaly": 190.62473283938996,
          "TrueAnomaly": 190.1748832957787,
          "AnomalyType": "True",
          "Period": 1594609.7474519643
        },
        "Spherical": {
          "RightAscension": 130.28921644540307,
          "Declination": 23.4680860732756,
          "RadiusMagnitude": 301348904.71153265,
          "HorizFPA": -0.22616731411905588,
          "VelocityAzimuth": 111.14072425536654,
          "VelocityMagnitude": 1137.6621215169923
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 23.369562164060795,
        "Geodetic_Longitude": -41.32063034977243,
        "Geodetic_Altitude": 294974127.20776147,
        "Geocentric_Latitude": 23.366604620648367,
        "Geocentric_Longitude": -41.32063034977243
      },
      "DurationSec": 1051163.10840802,
      "Results": {}
    }
  ],
  "Positions": null
}
Target EarthMoonL1_250704.json: Converged=True, Iterations=4
  Vy=203.75729964152615 m/s, L1_Vx=0.002022896749110714 m/s
             */
        }
    }
}
