using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator Target, 初始时刻为 月心平动点坐标系 的位置速度

        MCS(地球):
            >   TargetSequence
                >   InitialState   月心Libration系     Cartesian轨道参数
                >   Propagate      CisLunar, 停止于Cross Z-X平面(Y=0)

        微分修正:
            自变量: InitialState.Cartesian.Vy
            约束:   Propagate.Results.EM_Vx = 0  (< 0.01 m/s)
        */
        [TestMethod()]
        public void EarthMoonLibration_250702()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthMoonLibration_250702.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            input.ComputeCzmlPositions = false;
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

            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var dcResult = seg!.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults;

            var ctrl = dcResult!.ControlParameters[0];
            var cstr = dcResult.Results[0];
            double finalVy = double.Parse(ctrl.FinalValue, CultureInfo.InvariantCulture);
            double finalEmVx = double.Parse(cstr.CurrentValue, CultureInfo.InvariantCulture);

            Console.WriteLine("Target 微分修正结果:");
            Console.WriteLine($"  收敛: {dcResult.Converged}");
            Console.WriteLine($"  迭代次数: {dcResult.TotalIterations}");
            Console.WriteLine($"  自变量 InitialState.Cartesian.Vy = {finalVy} m/s");
            Console.WriteLine($"  约束 EM_Vx = {finalEmVx} m/s (目标 0, 容差 0.01 m/s)");

            Assert.IsTrue(dcResult.Converged, "Target 未收敛");
            Assert.IsTrue(Math.Abs(finalEmVx) < 0.01, $"EM_Vx 应 < 0.01 m/s, 实际 {finalEmVx} m/s");

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
          "TotalIterations": 5,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Cartesian.Vy",
              "InitialValue": "850.0000000000002",
              "FinalValue": "868.0544709159589",
              "Correction": 18.0544709159586,
              "LastUpdate": -0.011026211408427855,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "初始段",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": "",
              "Values": [
                850.0000000000002,
                876.9505946480045,
                868.9171326856529,
                868.0654971273673,
                868.0544709159589
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "EM_Vx",
              "DesiredValue": "0.0",
              "ParentName": "递推至CrossZX",
              "CurrentValue": "0.00018822882077529357",
              "Unit": "",
              "Difference": 0.00018822882077529357,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.01,
              "Weight": 1,
              "Values": [
                -152.8035264622939,
                112.30390035819153,
                9.834565344956502,
                0.12444450400823825,
                0.00018822882077529357
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.Vy_EM_Vx",
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
              "X": 209881778.72558814,
              "Y": -137298371.5511296,
              "Z": -49393594.30803994,
              "Vx": 837.4504417528148,
              "Vy": 1062.1500480838422,
              "Vz": 596.2813379772264
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 426916326.92349225,
              "Eccentricity": 0.4012456758946489,
              "Inclination": 26.58355630461359,
              "RAAN": 349.9852287260884,
              "ArgOfPeriapsis": 334.1631081380052,
              "MeanAnomaly": 0.09982568634897473,
              "TrueAnomaly": 0.2550501785441148,
              "AnomalyType": "True",
              "Period": 2776036.2120358837
            },
            "Spherical": {
              "RightAscension": 326.8084905434267,
              "Declination": -11.141432423763657,
              "RadiusMagnitude": 255618721.98701724,
              "HorizFPA": 0.07303335879811025,
              "VelocityAzimuth": 65.70801529750143,
              "VelocityMagnitude": 1478.1872009513556
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -11.011838949448881,
            "Geodetic_Longitude": -132.99918967827983,
            "Geodetic_Altitude": 249241363.83251995,
            "Geocentric_Latitude": -11.010044312537687,
            "Geocentric_Longitude": -132.99918967827983
          },
          "FinalState": {
            "Epoch": "2028-01-01T00:00:00.000Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 209881778.72558814,
              "Y": -137298371.5511296,
              "Z": -49393594.30803994,
              "Vx": 837.4504417528148,
              "Vy": 1062.1500480838422,
              "Vz": 596.2813379772264
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 426916326.92349225,
              "Eccentricity": 0.4012456758946489,
              "Inclination": 26.58355630461359,
              "RAAN": 349.9852287260884,
              "ArgOfPeriapsis": 334.1631081380052,
              "MeanAnomaly": 0.09982568634897473,
              "TrueAnomaly": 0.2550501785441148,
              "AnomalyType": "True",
              "Period": 2776036.2120358837
            },
            "Spherical": {
              "RightAscension": 326.8084905434267,
              "Declination": -11.141432423763657,
              "RadiusMagnitude": 255618721.98701724,
              "HorizFPA": 0.07303335879811025,
              "VelocityAzimuth": 65.70801529750143,
              "VelocityMagnitude": 1478.1872009513556
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -11.011838949448881,
            "Geodetic_Longitude": -132.99918967827983,
            "Geodetic_Altitude": 249241363.83251995,
            "Geocentric_Latitude": -11.010044312537687,
            "Geocentric_Longitude": -132.99918967827983
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
              "X": 209881778.72558814,
              "Y": -137298371.5511296,
              "Z": -49393594.30803994,
              "Vx": 837.4504417528148,
              "Vy": 1062.1500480838422,
              "Vz": 596.2813379772264
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 426916326.92349225,
              "Eccentricity": 0.4012456758946489,
              "Inclination": 26.58355630461359,
              "RAAN": 349.9852287260884,
              "ArgOfPeriapsis": 334.1631081380052,
              "MeanAnomaly": 0.09982568634897473,
              "TrueAnomaly": 0.2550501785441148,
              "AnomalyType": "True",
              "Period": 2776036.2120358837
            },
            "Spherical": {
              "RightAscension": 326.8084905434267,
              "Declination": -11.141432423763657,
              "RadiusMagnitude": 255618721.98701724,
              "HorizFPA": 0.07303335879811025,
              "VelocityAzimuth": 65.70801529750143,
              "VelocityMagnitude": 1478.1872009513556
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -11.011838949448881,
            "Geodetic_Longitude": -132.99918967827983,
            "Geodetic_Altitude": 249241363.83251995,
            "Geocentric_Latitude": -11.010044312537687,
            "Geocentric_Longitude": -132.99918967827983
          },
          "FinalState": {
            "Epoch": "2028-01-25T21:33:09.625521Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 108760801.0422302,
              "Y": -194843907.74665645,
              "Z": -86495723.23290199,
              "Vx": 1375.6441437213475,
              "Vy": 565.7196658520702,
              "Vz": 400.6557714668656
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 416017810.47554564,
              "Eccentricity": 0.4248926910561278,
              "Inclination": 26.61660214383355,
              "RAAN": 349.83984360244483,
              "ArgOfPeriapsis": 303.7564808542063,
              "MeanAnomaly": 0.9017802596321002,
              "TrueAnomaly": 2.467677458170403,
              "AnomalyType": "True",
              "Period": 2670415.662693046
            },
            "Spherical": {
              "RightAscension": 299.17001531769654,
              "Declination": -21.187507102685398,
              "RadiusMagnitude": 239321061.26899475,
              "HorizFPA": 0.7357791155662242,
              "VelocityAzimuth": 73.50398467258587,
              "VelocityMagnitude": 1540.4416891581216
            },
            "DryMass": 500,
            "FuelMass": 500,
            "Cd": 2.2,
            "Cr": 1,
            "DragArea": 20,
            "SRPArea": 20,
            "Geodetic_Latitude": -21.114565650624144,
            "Geodetic_Longitude": -148.4310653641684,
            "Geodetic_Altitude": 232945694.84718704,
            "Geocentric_Latitude": -21.1111289895806,
            "Geocentric_Longitude": -148.4310653641684
          },
          "DurationSec": 2151189.625520842,
          "Results": {
            "EM_X": -164595943.94630373,
            "EM_Y": 0.8159104362130165,
            "EM_Z": 45943.99061414227,
            "EM_Vx": 0.00018822882077529357,
            "EM_Vy": 960.9177385819851,
            "EM_Vz": 1.1269211924590508
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Target_EML_Vy_EM_Vx",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 209881778.72558814,
          "Y": -137298371.5511296,
          "Z": -49393594.30803994,
          "Vx": 837.4504417528148,
          "Vy": 1062.1500480838422,
          "Vz": 596.2813379772264
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 426916326.92349225,
          "Eccentricity": 0.4012456758946489,
          "Inclination": 26.58355630461359,
          "RAAN": 349.9852287260884,
          "ArgOfPeriapsis": 334.1631081380052,
          "MeanAnomaly": 0.09982568634897473,
          "TrueAnomaly": 0.2550501785441148,
          "AnomalyType": "True",
          "Period": 2776036.2120358837
        },
        "Spherical": {
          "RightAscension": 326.8084905434267,
          "Declination": -11.141432423763657,
          "RadiusMagnitude": 255618721.98701724,
          "HorizFPA": 0.07303335879811025,
          "VelocityAzimuth": 65.70801529750143,
          "VelocityMagnitude": 1478.1872009513556
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -11.011838949448881,
        "Geodetic_Longitude": -132.99918967827983,
        "Geodetic_Altitude": 249241363.83251995,
        "Geocentric_Latitude": -11.010044312537687,
        "Geocentric_Longitude": -132.99918967827983
      },
      "FinalState": {
        "Epoch": "2028-01-25T21:33:09.625521Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 108760801.0422302,
          "Y": -194843907.74665645,
          "Z": -86495723.23290199,
          "Vx": 1375.6441437213475,
          "Vy": 565.7196658520702,
          "Vz": 400.6557714668656
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 416017810.47554564,
          "Eccentricity": 0.4248926910561278,
          "Inclination": 26.61660214383355,
          "RAAN": 349.83984360244483,
          "ArgOfPeriapsis": 303.7564808542063,
          "MeanAnomaly": 0.9017802596321002,
          "TrueAnomaly": 2.467677458170403,
          "AnomalyType": "True",
          "Period": 2670415.662693046
        },
        "Spherical": {
          "RightAscension": 299.17001531769654,
          "Declination": -21.187507102685398,
          "RadiusMagnitude": 239321061.26899475,
          "HorizFPA": 0.7357791155662242,
          "VelocityAzimuth": 73.50398467258587,
          "VelocityMagnitude": 1540.4416891581216
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -21.114565650624144,
        "Geodetic_Longitude": -148.4310653641684,
        "Geodetic_Altitude": 232945694.84718704,
        "Geocentric_Latitude": -21.1111289895806,
        "Geocentric_Longitude": -148.4310653641684
      },
      "DurationSec": 2151189.625520842,
      "Results": {}
    }
  ],
  "Positions": null
}
Target 微分修正结果:
  收敛: True
  迭代次数: 5
  自变量 InitialState.Cartesian.Vy = 868.0544709159589 m/s
  约束 EM_Vx = 0.00018822882077529357 m/s (目标 0, 容差 0.01 m/s)
             */
        }
    }
}
