using System.Reflection;
using System.Text;
using System.Text.Json;
using AeroSpace.OrbitCore;
using ASTROX.Coordinates;
using ASTROX.Extended;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            测试 Astrogator
                StoppingCondition:  Duration

                MCS:
                >   Initial_State   地球惯性系 Kepler Kozai平均根数
                >   Propagate       Earth_J2
               
            与STK结果对比:   1e-6 deg

            20250620    使用理论值进行对比
        */
        [TestMethod()]
        public void EarthJ2_MeanElm_250620()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthJ2_MeanElm_250620.json");
                                    
            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi
            
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


            /*  STK 结果
             UTC Gregorian Date: 18 Jun 2025 23:46:13.000  UTC Julian Date: 2460845.49042824                   
Julian Ephemeris Date: 2460845.49122898                                                           
Time past epoch: 9.45928e+07 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)        
                                                                                                  
State Vector in Coordinate System: Earth Inertial                                                 
                                                                                                  
Parameter Set Type:  Cartesian                                                                    
         X:    -6127.1157978760074911 km              Vx:        3.5754528486802823 km/sec        
         Y:     1039.2571036277149688 km              Vy:        0.5597953211921074 km/sec        
         Z:     3219.1033063947370465 km              Vz:        6.6205223432960061 km/sec        
                                                                                                  
Parameter Set Type:  Keplerian                                                                    
       sma:     6995.9822886015381300 km            RAAN:         174.4300355375389 deg           
       ecc:        0.0004839292046199                  w:         176.3195171260498 deg           
       inc:         97.77698685241202 deg             TA:         211.3399551304912 deg  
            */
            var seg1 = output.MainSequenceResults[1];

            Assert.AreEqual(6995982.2886015381300, seg1.FinalState.Keplerian.SemiMajorAxis, 0.2);
            Assert.AreEqual(0.0004839292046199, seg1.FinalState.Keplerian.Eccentricity, 1e-7);
            Assert.AreEqual(97.77698685241202, seg1.FinalState.Keplerian.Inclination, 1e-5);
            Assert.AreEqual(174.4300355375389, seg1.FinalState.Keplerian.RAAN, 1e-6);
            double u0 = 176.3195171260498 + 211.3399551304912 - 360;
            double u1 = seg1.FinalState.Keplerian.ArgOfPeriapsis + seg1.FinalState.Keplerian.TrueAnomaly - 360;
            Assert.AreEqual(u0, u1, 1e-6);


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
        "Epoch": "2025-06-18T23:02:23Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 4847851.602137997,
          "Y": -1148902.7154549367,
          "Z": -4906028.432712753,
          "Vx": -5393.3682789332015,
          "Vy": -195.8291034649098,
          "Vz": -5278.965564384625
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600435436095.94,
          "SemiMajorAxis": 6990690.0170546835,
          "Eccentricity": 0.0004774581301430224,
          "Inclination": 97.78001444834574,
          "RAAN": 174.39938415655047,
          "ArgOfPeriapsis": 341.8653666482427,
          "MeanAnomaly": 243.26907882525126,
          "TrueAnomaly": 243.22022648471454,
          "Period": 5816.892681846676
        },
        "Spherical": {
          "RightAscension": 346.66732936168114,
          "Declination": -44.55903363349805,
          "RadiusMagnitude": 6992192.616663075,
          "HorizFPA": -0.024427483549458076,
          "VelocityAzimuth": 190.9519386577232,
          "VelocityMagnitude": 7549.453480876973
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -44.596530343632594,
        "Geodetic_Longitude": 93.9972449384374,
        "Geodetic_Altitude": 624555.7078028473,
        "Geocentric_Latitude": -44.42132036247923,
        "Geocentric_Longitude": 93.9972449384374
      },
      "FinalState": {
        "Epoch": "2025-06-18T23:02:23Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 4847851.602137997,
          "Y": -1148902.7154549367,
          "Z": -4906028.432712753,
          "Vx": -5393.3682789332015,
          "Vy": -195.8291034649098,
          "Vz": -5278.965564384625
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600435436095.94,
          "SemiMajorAxis": 6990690.0170546835,
          "Eccentricity": 0.0004774581301430224,
          "Inclination": 97.78001444834574,
          "RAAN": 174.39938415655047,
          "ArgOfPeriapsis": 341.8653666482427,
          "MeanAnomaly": 243.26907882525126,
          "TrueAnomaly": 243.22022648471454,
          "Period": 5816.892681846676
        },
        "Spherical": {
          "RightAscension": 346.66732936168114,
          "Declination": -44.55903363349805,
          "RadiusMagnitude": 6992192.616663075,
          "HorizFPA": -0.024427483549458076,
          "VelocityAzimuth": 190.9519386577232,
          "VelocityMagnitude": 7549.453480876973
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -44.596530343632594,
        "Geodetic_Longitude": 93.9972449384374,
        "Geodetic_Altitude": 624555.7078028473,
        "Geocentric_Latitude": -44.42132036247923,
        "Geocentric_Longitude": 93.9972449384374
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "PropagateResult",
      "StoppedOnMaximumDuration": false,
      "StoppingConditionName": "Duration",
      "TypeName": "Propagate",
      "Name": "轨道递推段",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2025-06-18T23:02:23Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 4847851.602137997,
          "Y": -1148902.7154549367,
          "Z": -4906028.432712753,
          "Vx": -5393.3682789332015,
          "Vy": -195.8291034649098,
          "Vz": -5278.965564384625
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600435436095.94,
          "SemiMajorAxis": 6990690.0170546835,
          "Eccentricity": 0.0004774581301430224,
          "Inclination": 97.78001444834574,
          "RAAN": 174.39938415655047,
          "ArgOfPeriapsis": 341.8653666482427,
          "MeanAnomaly": 243.26907882525126,
          "TrueAnomaly": 243.22022648471454,
          "Period": 5816.892681846676
        },
        "Spherical": {
          "RightAscension": 346.66732936168114,
          "Declination": -44.55903363349805,
          "RadiusMagnitude": 6992192.616663075,
          "HorizFPA": -0.024427483549458076,
          "VelocityAzimuth": 190.9519386577232,
          "VelocityMagnitude": 7549.453480876973
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -44.596530343632594,
        "Geodetic_Longitude": 93.9972449384374,
        "Geodetic_Altitude": 624555.7078028473,
        "Geocentric_Latitude": -44.42132036247923,
        "Geocentric_Longitude": 93.9972449384374
      },
      "FinalState": {
        "Epoch": "2025-06-18T23:46:13Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -6127115.799059795,
          "Y": 1039256.879672204,
          "Z": 3219103.3343704706,
          "Vx": 3575.452825048722,
          "Vy": 559.7949658157694,
          "Vz": 6620.522409879262
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600435436095.94,
          "SemiMajorAxis": 6995982.39492294,
          "Eccentricity": 0.0004839134115741676,
          "Inclination": 97.77698356493073,
          "RAAN": 174.43003589509013,
          "ArgOfPeriapsis": 176.31849706774497,
          "MeanAnomaly": 211.36982669564748,
          "TrueAnomaly": 211.34097529747834,
          "Period": 5823.499544655883
        },
        "Spherical": {
          "RightAscension": 170.37333957593358,
          "Declination": 27.383618428318687,
          "RadiusMagnitude": 6998873.420371971,
          "HorizFPA": -0.014427190252751304,
          "VelocityAzimuth": 351.23432630327466,
          "VelocityMagnitude": 7545.101078687751
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 27.386827220655054,
        "Geodetic_Longitude": -93.30583613124097,
        "Geodetic_Altitude": 625233.591476165,
        "Geocentric_Latitude": 27.24395917808126,
        "Geocentric_Longitude": -93.30583613124097
      },
      "DurationSec": 2630,
      "Results": {}
    }
  ],
            */
        }
    }
}