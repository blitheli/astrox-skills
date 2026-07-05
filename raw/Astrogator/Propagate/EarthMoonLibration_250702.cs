using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class PropagateTests
    {
        /*
         测试 Astrogator, 初始时刻为 月心平动点坐标系 的位置速度
            
        MCS(地球):
            >   InitialState   月心Libration系     Cartesian轨道参数
            >   Propagate      CisLunar, 20天            

        仅轨道递推,Cislunar积分器下20天后，地心系和月心Libration系下的坐标和速度与STK对比
            - Earth Inertial: ΔR < 1e-6 km, ΔV < 1e-10 km/s
            - Moon Libration: ΔR < 1e-6 km, ΔV < 1e-5 km/s
        */
        [TestMethod()]
        public void EarthMoonLibration_250702()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

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


            //----------------  初始时刻校对  -------------------------------------------------            
            var rv0 = output.MainSequenceResults[0].InitialState.Cartesian;
            const double posTol = 1e-8;   // km
            const double velTol = 1e-13;    // km/s
            //  与STK对比
            Assert.AreEqual(209881.7787255910516251, rv0.X * 0.001, posTol);
            Assert.AreEqual(-137298.3715511265327223, rv0.Y * 0.001, posTol);
            Assert.AreEqual(-49393.5943080413198913, rv0.Z * 0.001, posTol);
            Assert.AreEqual(0.8272407705015086, rv0.Vx * 0.001, velTol);
            Assert.AreEqual(1.0491646530632566, rv0.Vy * 0.001, velTol);
            Assert.AreEqual(0.5889939482418874, rv0.Vz * 0.001, velTol);

            //----------------  20天后校对  -------------------------------------------------
            var rv1 = output.MainSequenceResults[1].FinalState.Cartesian;
            const double posTol2 = 1e-6;   // km
            const double velTol2 = 1e-10;    // km/s
            //  与STK对比(Earth Inertial)
            Assert.AreEqual(-227409.9731031271512620, rv1.X * 0.001, posTol2);
            Assert.AreEqual(-162557.0744860093691386, rv1.Y * 0.001, posTol2);
            Assert.AreEqual(-100200.6852015070617199, rv1.Z * 0.001, posTol2);
            Assert.AreEqual(1.0829720087755526, rv1.Vx * 0.001, velTol2);
            Assert.AreEqual(-0.7221936103316435, rv1.Vy * 0.001, velTol2);
            Assert.AreEqual(-0.2605834556307281, rv1.Vz * 0.001, velTol2);

            //  与STK对比(Moon Libration）,位置有1e-6偏差，导致速度精度下降到1e-5
            var rv1_em = output.MainSequenceResults[1].Results;
            Assert.AreEqual(-104934.994460, (double)rv1_em["EM_X"] * 0.001, posTol2);
            Assert.AreEqual(-86484.185332, (double)rv1_em["EM_Y"] * 0.001, posTol2);
            Assert.AreEqual(-114.435832, (double)rv1_em["EM_Z"] * 0.001, posTol2);
            Assert.AreEqual(-0.232345, (double)rv1_em["EM_Vx"] * 0.001, 1e-6);
            Assert.AreEqual(0.591675, (double)rv1_em["EM_Vy"] * 0.001, 1e-6);
            Assert.AreEqual(0.000586, (double)rv1_em["EM_Vz"] * 0.001, 1e-6);

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
          "X": 209881778.72558814,
          "Y": -137298371.55112964,
          "Z": -49393594.30803994,
          "Vx": 827.2407705015112,
          "Vy": 1049.1646530632524,
          "Vz": 588.9939482418856
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 403963778.786241,
          "Eccentricity": 0.3672256201776852,
          "Inclination": 26.583556304613587,
          "RAAN": 349.9852287260884,
          "ArgOfPeriapsis": 334.14288370732163,
          "MeanAnomaly": 0.1185005288703306,
          "TrueAnomaly": 0.27527460922764085,
          "AnomalyType": "True",
          "Period": 2555198.358167786
        },
        "Spherical": {
          "RightAscension": 326.8084905434267,
          "Declination": -11.141432423763657,
          "RadiusMagnitude": 255618721.98701724,
          "HorizFPA": 0.07393641251011734,
          "VelocityAzimuth": 65.70801529750145,
          "VelocityMagnitude": 1460.1327448841198
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -11.011838949448881,
        "Geodetic_Longitude": -132.99918967827983,
        "Geodetic_Altitude": 249241363.83251992,
        "Geocentric_Latitude": -11.010044312537687,
        "Geocentric_Longitude": -132.99918967827983
      },
      "FinalState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 209881778.72558814,
          "Y": -137298371.55112964,
          "Z": -49393594.30803994,
          "Vx": 827.2407705015112,
          "Vy": 1049.1646530632524,
          "Vz": 588.9939482418856
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 403963778.786241,
          "Eccentricity": 0.3672256201776852,
          "Inclination": 26.583556304613587,
          "RAAN": 349.9852287260884,
          "ArgOfPeriapsis": 334.14288370732163,
          "MeanAnomaly": 0.1185005288703306,
          "TrueAnomaly": 0.27527460922764085,
          "AnomalyType": "True",
          "Period": 2555198.358167786
        },
        "Spherical": {
          "RightAscension": 326.8084905434267,
          "Declination": -11.141432423763657,
          "RadiusMagnitude": 255618721.98701724,
          "HorizFPA": 0.07393641251011734,
          "VelocityAzimuth": 65.70801529750145,
          "VelocityMagnitude": 1460.1327448841198
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -11.011838949448881,
        "Geodetic_Longitude": -132.99918967827983,
        "Geodetic_Altitude": 249241363.83251992,
        "Geocentric_Latitude": -11.010044312537687,
        "Geocentric_Longitude": -132.99918967827983
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "PropagateResult",
      "StoppedOnMaximumDuration": false,
      "StoppingConditionName": "Duration",
      "TypeName": "Propagate",
      "Name": "递推20天",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2028-01-01T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 209881778.72558814,
          "Y": -137298371.55112964,
          "Z": -49393594.30803994,
          "Vx": 827.2407705015112,
          "Vy": 1049.1646530632524,
          "Vz": 588.9939482418856
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 403963778.786241,
          "Eccentricity": 0.3672256201776852,
          "Inclination": 26.583556304613587,
          "RAAN": 349.9852287260884,
          "ArgOfPeriapsis": 334.14288370732163,
          "MeanAnomaly": 0.1185005288703306,
          "TrueAnomaly": 0.27527460922764085,
          "AnomalyType": "True",
          "Period": 2555198.358167786
        },
        "Spherical": {
          "RightAscension": 326.8084905434267,
          "Declination": -11.141432423763657,
          "RadiusMagnitude": 255618721.98701724,
          "HorizFPA": 0.07393641251011734,
          "VelocityAzimuth": 65.70801529750145,
          "VelocityMagnitude": 1460.1327448841198
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -11.011838949448881,
        "Geodetic_Longitude": -132.99918967827983,
        "Geodetic_Altitude": 249241363.83251992,
        "Geocentric_Latitude": -11.010044312537687,
        "Geocentric_Longitude": -132.99918967827983
      },
      "FinalState": {
        "Epoch": "2028-01-21T00:00:00.000Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -227409973.1036433,
          "Y": -162557074.48570192,
          "Z": -100200685.20131187,
          "Vx": 1082.9720087742596,
          "Vy": -722.1936103316649,
          "Vz": -260.58345563140216
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 432171768.5437193,
          "Eccentricity": 0.3990094161929289,
          "Inclination": 26.604951000141348,
          "RAAN": 349.8604359111757,
          "ArgOfPeriapsis": 284.799661475014,
          "MeanAnomaly": 335.8296629924438,
          "TrueAnomaly": 304.092147479509,
          "AnomalyType": "True",
          "Period": 2827454.147078996
        },
        "Spherical": {
          "RightAscension": 215.5579225631809,
          "Declination": -19.72043313618947,
          "RadiusMagnitude": 296951638.56621873,
          "HorizFPA": -15.111656338297825,
          "VelocityAzimuth": 108.22743823218445,
          "VelocityMagnitude": 1327.5148661846474
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -19.851929528952848,
        "Geodetic_Longitude": 96.05380357990772,
        "Geodetic_Altitude": 290575963.7129978,
        "Geocentric_Latitude": -19.849297097465534,
        "Geocentric_Longitude": 96.05380357990772
      },
      "DurationSec": 1728000,
      "Results": {
        "EM_X": -104934994.46015927,
        "EM_Y": -86484185.33213466,
        "EM_Z": -114435.83169651031,
        "EM_Vx": -232.3453581978302,
        "EM_Vy": 591.6749913049252,
        "EM_Vz": 0.5922245927922729
      }
    }
  ],
  "Positions": null
}
             */
        }
    }
}
