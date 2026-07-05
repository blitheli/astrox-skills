using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.Celestial;

namespace ASTROX.Astrogator.Tests
{
    public partial class PropagateTests
    {
        /*
         测试 Astrogator Propagate, 初始时刻为 Moon L2 坐标系 的位置速度
         STK 参考: Docs/EM-L1-L5.md
        */
        [TestMethod()]
        public void EarthMoonL2_250704()
        {
            PlanetsEphemeris.UseJplDe430File();

            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent!.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");
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

            var rv0 = output.MainSequenceResults[0].InitialState.Cartesian;
            const double posTol0 = 1e-6;   // km, 初始转换(历表差异约米级)
            const double velTol0 = 1e-10;  // km/s
            Assert.AreEqual(390709.0258058424, rv0.X * 0.001, posTol0);
            Assert.AreEqual(-270337.9107576004, rv0.Y * 0.001, posTol0);
            Assert.AreEqual(-65670.4479308186, rv0.Z * 0.001, posTol0);
            Assert.AreEqual(0.5681972349777227, rv0.Vx * 0.001, velTol0);
            Assert.AreEqual(0.7191943377787399, rv0.Vy * 0.001, velTol0);
            Assert.AreEqual(0.4038491781993330, rv0.Vz * 0.001, velTol0);

            var rv1 = output.MainSequenceResults[1].FinalState.Cartesian;
            const double posTol = 1e-5;    // km, CisLunar 积分末态
            const double velTol = 1e-10;   // km/s
            Assert.AreEqual(-395054.3729358027, rv1.X * 0.001, posTol);
            Assert.AreEqual(153007.8360070150, rv1.Y * 0.001, posTol);
            Assert.AreEqual(71032.4740385429, rv1.Z * 0.001, posTol);
            Assert.AreEqual(-0.4788896580479391, rv1.Vx * 0.001, velTol);
            Assert.AreEqual(-0.9040286956491417, rv1.Vy * 0.001, velTol);
            Assert.AreEqual(-0.4661528095774208, rv1.Vz * 0.001, velTol);

            var rvLp = output.MainSequenceResults[1].Results;
            const double lpPosTol = 1e-5;   // km
            const double lpVelTol = 1e-8;   // km/s
            Assert.AreEqual(5864.6711293461, (double)rvLp["L2_X"]! * 0.001, lpPosTol);
            Assert.AreEqual(-65.0612628683, (double)rvLp["L2_Y"]! * 0.001, lpPosTol);
            Assert.AreEqual(26997.7946342536, (double)rvLp["L2_Z"]! * 0.001, lpPosTol);
            Assert.AreEqual(-0.0012307267499546, (double)rvLp["L2_Vx"]! * 0.001, lpVelTol);
            Assert.AreEqual(-0.1603814931708428, (double)rvLp["L2_Vy"]! * 0.001, lpVelTol);
            Assert.AreEqual(0.0192169623250418, (double)rvLp["L2_Vz"]! * 0.001, lpVelTol);
            /*
         {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "TypeName": "InitialState",
      "Name": "初始段",
      "Description": "初始段参数",
      "UserComment": "初始段参数",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 390709025.8058384,
          "Y": -270337910.7576046,
          "Z": -65670447.93081723,
          "Vx": 568.197234977725,
          "Vy": 719.1943377787404,
          "Vz": 403.8491781993334
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 604930031.7126657,
          "Eccentricity": 0.20713600482772504,
          "Inclination": 25.220679191144537,
          "RAAN": 342.38501775673143,
          "ArgOfPeriapsis": 340.5248673092227,
          "MeanAnomaly": 0.47045859615450625,
          "TrueAnomaly": 0.7321455199650797,
          "AnomalyType": "True",
          "Period": 4682402.025848612
        },
        "Spherical": {
          "RightAscension": 325.32001188001976,
          "Declination": -7.869534180375285,
          "RadiusMagnitude": 479633961.02691483,
          "HorizFPA": 0.1256291351693232,
          "VelocityAzimuth": 65.96181256133131,
          "VelocityMagnitude": 1001.5901118030318
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
          "X": 390709025.8058384,
          "Y": -270337910.7576046,
          "Z": -65670447.93081723,
          "Vx": 568.197234977725,
          "Vy": 719.1943377787404,
          "Vz": 403.8491781993334
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 604930031.7126657,
          "Eccentricity": 0.20713600482772504,
          "Inclination": 25.220679191144537,
          "RAAN": 342.38501775673143,
          "ArgOfPeriapsis": 340.5248673092227,
          "MeanAnomaly": 0.47045859615450625,
          "TrueAnomaly": 0.7321455199650797,
          "AnomalyType": "True",
          "Period": 4682402.025848612
        },
        "Spherical": {
          "RightAscension": 325.32001188001976,
          "Declination": -7.869534180375285,
          "RadiusMagnitude": 479633961.02691483,
          "HorizFPA": 0.1256291351693232,
          "VelocityAzimuth": 65.96181256133131,
          "VelocityMagnitude": 1001.5901118030318
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
      "StoppingConditionName": "Duration",
      "TypeName": "Propagate",
      "Name": "递推至15Jan2028",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 390709025.8058384,
          "Y": -270337910.7576046,
          "Z": -65670447.93081723,
          "Vx": 568.197234977725,
          "Vy": 719.1943377787404,
          "Vz": 403.8491781993334
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 604930031.7126657,
          "Eccentricity": 0.20713600482772504,
          "Inclination": 25.220679191144537,
          "RAAN": 342.38501775673143,
          "ArgOfPeriapsis": 340.5248673092227,
          "MeanAnomaly": 0.47045859615450625,
          "TrueAnomaly": 0.7321455199650797,
          "AnomalyType": "True",
          "Period": 4682402.025848612
        },
        "Spherical": {
          "RightAscension": 325.32001188001976,
          "Declination": -7.869534180375285,
          "RadiusMagnitude": 479633961.02691483,
          "HorizFPA": 0.1256291351693232,
          "VelocityAzimuth": 65.96181256133131,
          "VelocityMagnitude": 1001.5901118030318
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
        "Epoch": "2028-01-15T05:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -395054372.9351625,
          "Y": 153007836.00651538,
          "Z": 71032474.03825444,
          "Vx": -478.8896580432579,
          "Vy": -904.0286956505955,
          "Vz": -466.15280957933885
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 673383257.7881722,
          "Eccentricity": 0.36369908113544025,
          "Inclination": 26.892225139296833,
          "RAAN": 358.1335489965985,
          "ArgOfPeriapsis": 150.648970999937,
          "MeanAnomaly": 3.4427268572730667,
          "TrueAnomaly": 7.907374139148118,
          "AnomalyType": "True",
          "Period": 5499262.520342834
        },
        "Spherical": {
          "RightAscension": 158.82820097586693,
          "Declination": 9.51812408876842,
          "RadiusMagnitude": 429563694.72126764,
          "HorizFPA": 2.1066062499037743,
          "VelocityAzimuth": 115.26972804658041,
          "VelocityMagnitude": 1124.233796422798
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 9.372178292239811,
        "Geodetic_Longitude": -29.99078581548125,
        "Geodetic_Altitude": 423186123.8436867,
        "Geocentric_Latitude": 9.371263166046043,
        "Geocentric_Longitude": -29.990785815481242
      },
      "DurationSec": 1227600,
      "Results": {
        "L2_X": 5864671.128545928,
        "L2_Y": -65061.26261649467,
        "L2_Z": 26997794.634156935,
        "L2_Vx": -1.2307267543140696,
        "L2_Vy": -160.38149316826542,
        "L2_Vz": 19.216962323600754
      }
    }
  ],
  "Positions": null
}
             */
        }
    }
}
