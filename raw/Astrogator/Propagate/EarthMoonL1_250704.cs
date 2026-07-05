using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.Celestial;

namespace ASTROX.Astrogator.Tests
{
    public partial class PropagateTests
    {
        /*
         测试 Astrogator Propagate, 初始时刻为 Moon L1 坐标系 的位置速度
         STK 参考: Docs/EM-L1-L5.md
        */
        [TestMethod()]
        public void EarthMoonL1_250704()
        {
            PlanetsEphemeris.UseJplDe430File();

            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent!.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");
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

            var rv0 = output.MainSequenceResults[0].InitialState.Cartesian;
            const double posTol0 = 1e-6;   // km, 初始转换
            const double velTol0 = 1e-10;  // km/s
            Assert.AreEqual(276335.2203882828, rv0.X * 0.001, posTol0);
            Assert.AreEqual(-195517.9939106615, rv0.Y * 0.001, posTol0);
            Assert.AreEqual(-38753.7075707705, rv0.Z * 0.001, posTol0);
            Assert.AreEqual(0.5747798155897577, rv0.Vx * 0.001, velTol0);
            Assert.AreEqual(0.7285163750192698, rv0.Vy * 0.001, velTol0);
            Assert.AreEqual(0.4090157077924502, rv0.Vz * 0.001, velTol0);

            var rv1 = output.MainSequenceResults[1].FinalState.Cartesian;
            const double posTol = 1e-5;    // km, CisLunar 积分末态
            const double velTol = 1e-10;   // km/s
            Assert.AreEqual(-180267.8257851772, rv1.X * 0.001, posTol);
            Assert.AreEqual(215074.6098653715, rv1.Y * 0.001, posTol);
            Assert.AreEqual(121472.3848217533, rv1.Z * 0.001, posTol);
            Assert.AreEqual(-0.9317023041776823, rv1.Vx * 0.001, velTol);
            Assert.AreEqual(-0.5355371250554700, rv1.Vy * 0.001, velTol);
            Assert.AreEqual(-0.3748133257167897, rv1.Vz * 0.001, velTol);

            var rvLp = output.MainSequenceResults[1].Results;
            const double lpPosTol = 1e-5;   // km
            const double lpVelTol = 1e-8;   // km/s
            Assert.AreEqual(-360.4947651381, (double)rvLp["L1_X"]! * 0.001, lpPosTol);
            Assert.AreEqual(-1392.9829909785, (double)rvLp["L1_Y"]! * 0.001, lpPosTol);
            Assert.AreEqual(27925.1973416424, (double)rvLp["L1_Z"]! * 0.001, lpPosTol);
            Assert.AreEqual(0.0299638291797560, (double)rvLp["L1_Vx"]! * 0.001, lpVelTol);
            Assert.AreEqual(0.2075244771185927, (double)rvLp["L1_Vy"]! * 0.001, lpVelTol);
            Assert.AreEqual(-0.0261958444228603, (double)rvLp["L1_Vz"]! * 0.001, lpVelTol);
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
          "X": 276335220.3882799,
          "Y": -195517993.9106646,
          "Z": -38753707.57076952,
          "Vx": 574.7798155897603,
          "Vy": 728.516375019269,
          "Vz": 409.01570779244986
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 303961860.09603393,
          "Eccentricity": 0.12094099372812273,
          "Inclination": 24.786085948194238,
          "RAAN": 339.0737416816983,
          "ArgOfPeriapsis": 164.91238740478747,
          "MeanAnomaly": 179.1715975738497,
          "TrueAnomaly": 179.34554642167316,
          "AnomalyType": "True",
          "Period": 1667783.3004715831
        },
        "Spherical": {
          "RightAscension": 324.7190974490735,
          "Declination": -6.530987029365744,
          "RadiusMagnitude": 340720251.5559805,
          "HorizFPA": 0.09003692463082902,
          "VelocityAzimuth": 66.03722095519022,
          "VelocityMagnitude": 1014.1014714029233
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -6.403654171763279,
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
          "Z": -38753707.57076952,
          "Vx": 574.7798155897603,
          "Vy": 728.516375019269,
          "Vz": 409.01570779244986
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 303961860.09603393,
          "Eccentricity": 0.12094099372812273,
          "Inclination": 24.786085948194238,
          "RAAN": 339.0737416816983,
          "ArgOfPeriapsis": 164.91238740478747,
          "MeanAnomaly": 179.1715975738497,
          "TrueAnomaly": 179.34554642167316,
          "AnomalyType": "True",
          "Period": 1667783.3004715831
        },
        "Spherical": {
          "RightAscension": 324.7190974490735,
          "Declination": -6.530987029365744,
          "RadiusMagnitude": 340720251.5559805,
          "HorizFPA": 0.09003692463082902,
          "VelocityAzimuth": 66.03722095519022,
          "VelocityMagnitude": 1014.1014714029233
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -6.403654171763279,
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
      "StoppingConditionName": "Duration",
      "TypeName": "Propagate",
      "Name": "递推至13Jan2028",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 276335220.3882799,
          "Y": -195517993.9106646,
          "Z": -38753707.57076952,
          "Vx": 574.7798155897603,
          "Vy": 728.516375019269,
          "Vz": 409.01570779244986
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 303961860.09603393,
          "Eccentricity": 0.12094099372812273,
          "Inclination": 24.786085948194238,
          "RAAN": 339.0737416816983,
          "ArgOfPeriapsis": 164.91238740478747,
          "MeanAnomaly": 179.1715975738497,
          "TrueAnomaly": 179.34554642167316,
          "AnomalyType": "True",
          "Period": 1667783.3004715831
        },
        "Spherical": {
          "RightAscension": 324.7190974490735,
          "Declination": -6.530987029365744,
          "RadiusMagnitude": 340720251.5559805,
          "HorizFPA": 0.09003692463082902,
          "VelocityAzimuth": 66.03722095519022,
          "VelocityMagnitude": 1014.1014714029233
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -6.403654171763279,
        "Geodetic_Longitude": -135.09520282494998,
        "Geodetic_Altitude": 334342380.09656125,
        "Geocentric_Latitude": -6.402858324840253,
        "Geocentric_Longitude": -135.09520282494998
      },
      "FinalState": {
        "Epoch": "2028-01-13T04:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -180267825.7841455,
          "Y": 215074609.86256686,
          "Z": 121472384.82082342,
          "Vx": -931.7023041646316,
          "Vy": -535.5371250750434,
          "Vz": -374.8133257186877
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 303893691.32521284,
          "Eccentricity": 0.021737026962909998,
          "Inclination": 31.423504542473065,
          "RAAN": 355.07963179613193,
          "ArgOfPeriapsis": 22.470230721556163,
          "MeanAnomaly": 105.51331299481105,
          "TrueAnomaly": 107.89543850683008,
          "AnomalyType": "True",
          "Period": 1667222.2875058604
        },
        "Spherical": {
          "RightAscension": 129.9685319731598,
          "Declination": 23.405613863402333,
          "RadiusMagnitude": 305792604.7087087,
          "HorizFPA": 1.1929812033933633,
          "VelocityAzimuth": 111.58871823266311,
          "VelocityMagnitude": 1138.136294586738
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 23.30771862760074,
        "Geodetic_Longitude": -41.795361706225115,
        "Geodetic_Altitude": 299417810.43950593,
        "Geocentric_Latitude": 23.304809997886434,
        "Geocentric_Longitude": -41.79536170622512
      },
      "DurationSec": 1051200,
      "Results": {
        "L1_X": -360494.7681156117,
        "L1_Y": -1392982.9901115876,
        "L1_Z": 27925197.34197606,
        "L1_Vx": 29.96382915956096,
        "L1_Vy": 207.52447712762842,
        "L1_Vz": -26.195844416950656
      }
    }
  ],
  "Positions": null
}
             */
        }
    }
}
