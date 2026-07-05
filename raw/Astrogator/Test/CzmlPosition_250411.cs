using System.Reflection;
using System.Text;
using System.Text.Json;
using ASTROX.Coordinates;
using ASTROX.Extended;
using ASTROX.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
         测试 Astrogator 分段结果中，lagrange插值器的精度问题

            当段结果中的点数较少时，需要多计算点数，这样才能保证某个段的lagrange插值精确！
           
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        1. **Initial_State（初始状态）**
     
        2. **Propagate60s**
          - 积分器: Earth Point Mass
          - 停止条件：60s

        3. **ImpulsiveManuver**
          - 姿态： VNC(法向: 100m/s)

        4. **Propagate600s**
          - 积分器: Earth Point Mass
          - 停止条件：600s
        
        常规计算时，由于第2段仅有60s，所以轨道数据只有3个点，采用插值计算不准确!
        
        解决方法：
            在AgVAMCSPropagate段的ConvertSegmentResult方法中，如果数据点少，那么会多计算一些点数，
                    并填充到输入参数segmentResults.EntireComputedEphemeris属性中!

        数据点较多时，步长一般为70s左右，此时插值阶数为7阶时，插值计算的精度才能和理论精度差不多!

        20250411
        */
        [TestMethod()]
        public void CzmlPosition_250411()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Test");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "CzmlPosition_250411.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);

            //  mcs结果序列化输出
            Console.WriteLine(JsonSerializer.Serialize(output, 
                new JsonSerializerOptions { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            //-------------------------------------------------------------------------------
            //  轨道历元时刻
            JulianDate T0 = new JulianDate(GregorianDate.Parse(output.Positions.CzmlPositions[0].epoch));
            //  T0时刻位置速度, 轨道根数
            var rv0 = new Motion<Cartesian>(new Cartesian(6678137, 0, 0), new Cartesian(0, 6789.5303002727, 3686.4141744009));

            var elm0 = new KeplerianElements(rv0, 3.986004415e14);

            //===============================================================================
            //  第1段数据（原始只有3个点，后来扩充到5个点），创建积分器（lagrange，4阶插值）
            var pointEvaluator0 = output.Positions.CzmlPositions[0].CreatePoint();

            //  20s插值数据
            var rv1 = pointEvaluator0.GetEvaluator().Evaluate(T0.AddSeconds(20.0), 1);
            //  理论值(6676.349534     135.778491      73.721705      -0.178739       6.787713       3.685427)
            var rv1p = elm0.GetElementsAfterDt(20.0).ToCartesian();

           Assert.AreEqual(6676.349534, rv1.Value.X * 0.001, 1e-6);
            Assert.AreEqual(135.778491, rv1.Value.Y * 0.001, 1e-6);
            Assert.AreEqual(73.721705, rv1.Value.Z * 0.001, 1e-6);
            Assert.AreEqual(-0.178739, rv1.FirstDerivative.X * 0.001, 1e-6);
            Assert.AreEqual(6.787713, rv1.FirstDerivative.Y * 0.001, 1e-6);
            Assert.AreEqual(3.685427, rv1.FirstDerivative.Z * 0.001, 1e-6);
            //==============================================================================
            //  第2段数据，创建积分器（默认： lagrange 7阶）
            var pointEvaluator1 = output.Positions.CzmlPositions[1].CreatePoint();
            //  80s插值数据（{6649556.656118212, 541433.1105588032, 296249.77660537953}
            var rv2 = pointEvaluator1.GetEvaluator().Evaluate(T0.AddSeconds(80.0), 1);
            //  STK理论值
            //6649.556680     541.433116     296.249779 - 0.713998       6.712770       3.758496
            Assert.AreEqual(6649.556680, rv2.Value.X*0.001, 1e-6);
            Assert.AreEqual(541.433116, rv2.Value.Y * 0.001, 1e-6);
            Assert.AreEqual(296.249779, rv2.Value.Z * 0.001, 1e-6);
            Assert.AreEqual(-0.713998, rv2.FirstDerivative.X * 0.001, 1e-6);
            Assert.AreEqual(6.712770, rv2.FirstDerivative.Y * 0.001, 1e-6);
            Assert.AreEqual(3.758496, rv2.FirstDerivative.Z * 0.001, 1e-6);

        }

        /*
          {
  "IsSuccess": true,
  "Message": "Success",
  "MainSequenceResults": [
    {
      "$type": "SegmentResult",
      "TypeName": "InitialState",
      "Name": "Initial_State",
      "Description": "初始段参数",
      "UserComment": "初始段参数",
      "InitialState": {
        "Epoch": "2025-03-25T04:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6678137,
          "Y": 0,
          "Z": 0,
          "Vx": 0,
          "Vy": 6789.530297717652,
          "Vz": 3686.414173013652
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6678137,
          "Eccentricity": 3.5353762907987447E-16,
          "Inclination": 28.5,
          "RAAN": 0,
          "ArgOfPeriapsis": 0,
          "MeanAnomaly": 0,
          "TrueAnomaly": 0,
          "Period": 5431.177131191049
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 6678137,
          "HorizFPA": 0,
          "VelocityAzimuth": 61.50000000000001,
          "VelocityMagnitude": 7725.760229169805
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2025-03-25T04:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6678137,
          "Y": 0,
          "Z": 0,
          "Vx": 0,
          "Vy": 6789.530297717652,
          "Vz": 3686.414173013652
        },
        "Keplerian": null,
        "Spherical": null,
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "SegmentResult",
      "TypeName": "Propagate",
      "Name": "Propagate60s",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2025-03-25T04:00:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6678137,
          "Y": 0,
          "Z": 0,
          "Vx": 0,
          "Vy": 6789.530297717652,
          "Vz": 3686.414173013652
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6678137,
          "Eccentricity": 3.5353762907987447E-16,
          "Inclination": 28.5,
          "RAAN": 0,
          "ArgOfPeriapsis": 0,
          "MeanAnomaly": 0,
          "TrueAnomaly": 0,
          "Period": 5431.177131191049
        },
        "Spherical": {
          "RightAscension": 0,
          "Declination": 0,
          "RadiusMagnitude": 6678137,
          "HorizFPA": 0,
          "VelocityAzimuth": 61.50000000000001,
          "VelocityMagnitude": 7725.760229169805
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2025-03-25T04:01:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6662055.548222059,
          "Y": 407044.7714350037,
          "Z": 221007.27865866007,
          "Vx": -535.8331481627247,
          "Vy": 6773.180602573988,
          "Vz": 3677.5370128480495
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6678137,
          "Eccentricity": 3.5546523864795853E-16,
          "Inclination": 28.5,
          "RAAN": 5.548853725589339E-16,
          "ArgOfPeriapsis": 0,
          "MeanAnomaly": 3.97703839853633,
          "TrueAnomaly": 3.97703839853633,
          "Period": 5431.177131191049
        },
        "Spherical": {
          "RightAscension": 3.496367029802685,
          "Declination": 1.8965015059211792,
          "RadiusMagnitude": 6678137.000000001,
          "HorizFPA": 0,
          "VelocityAzimuth": 61.55788846002592,
          "VelocityMagnitude": 7725.760229169804
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 60,
      "Results": {}
    },
    {
      "$type": "SegmentResult",
      "TypeName": "ManeuverImpulsive",
      "Name": "ImpulsiveManeuver",
      "Description": "轨道机动段",
      "UserComment": "轨道机动段",
      "InitialState": {
        "Epoch": "2025-03-25T04:01:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6662055.548222059,
          "Y": 407044.7714350037,
          "Z": 221007.27865866007,
          "Vx": -535.8331481627247,
          "Vy": 6773.180602573988,
          "Vz": 3677.5370128480495
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6678137,
          "Eccentricity": 3.5546523864795853E-16,
          "Inclination": 28.5,
          "RAAN": 5.548853725589339E-16,
          "ArgOfPeriapsis": 0,
          "MeanAnomaly": 3.97703839853633,
          "TrueAnomaly": 3.97703839853633,
          "Period": 5431.177131191049
        },
        "Spherical": {
          "RightAscension": 3.496367029802685,
          "Declination": 1.8965015059211792,
          "RadiusMagnitude": 6678137.000000001,
          "HorizFPA": 0,
          "VelocityAzimuth": 61.55788846002592,
          "VelocityMagnitude": 7725.760229169804
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2025-03-25T04:01:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6662055.548222059,
          "Y": 407044.7714350037,
          "Z": 221007.27865866007,
          "Vx": -535.833148162729,
          "Vy": 6725.464726548027,
          "Vz": 3765.418724114246
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6679256.040077478,
          "Eccentricity": 0.00016753962877897105,
          "Inclination": 29.239834201213167,
          "RAAN": 0.10529285530105924,
          "ArgOfPeriapsis": 3.884829606257869,
          "MeanAnomaly": 359.99999999982305,
          "TrueAnomaly": 359.999999999823,
          "Period": 5432.542323192352
        },
        "Spherical": {
          "RightAscension": 3.496367029802685,
          "Declination": 1.8965015059211792,
          "RadiusMagnitude": 6678137.000000001,
          "HorizFPA": -3.81666561775622E-14,
          "VelocityAzimuth": 60.816309918057065,
          "VelocityMagnitude": 7726.407387565184
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "SegmentResult",
      "TypeName": "Propagate",
      "Name": "Propagate600s",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2025-03-25T04:01:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 6662055.548222059,
          "Y": 407044.7714350037,
          "Z": 221007.27865866007,
          "Vx": -535.833148162729,
          "Vy": 6725.464726548027,
          "Vz": 3765.418724114246
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6679256.040077478,
          "Eccentricity": 0.00016753962877897105,
          "Inclination": 29.239834201213167,
          "RAAN": 0.10529285530105924,
          "ArgOfPeriapsis": 3.884829606257869,
          "MeanAnomaly": 359.99999999982305,
          "TrueAnomaly": 359.999999999823,
          "Period": 5432.542323192352
        },
        "Spherical": {
          "RightAscension": 3.496367029802685,
          "Declination": 1.8965015059211792,
          "RadiusMagnitude": 6678137.000000001,
          "HorizFPA": -3.81666561775622E-14,
          "VelocityAzimuth": 60.816309918057065,
          "VelocityMagnitude": 7726.407387565184
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "FinalState": {
        "Epoch": "2025-03-25T04:11:00Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 4824280.773722159,
          "Y": 4031830.7600951274,
          "Z": 2252027.3735829424,
          "Vx": -5342.043520350771,
          "Vy": 4868.147894293297,
          "Vz": 2730.650247661656
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 6679256.040077482,
          "Eccentricity": 0.00016753962879124387,
          "Inclination": 29.239834201213167,
          "RAAN": 0.10529285530105974,
          "ArgOfPeriapsis": 3.8848295972671334,
          "MeanAnomaly": 39.76038973975932,
          "TrueAnomaly": 39.77267074426146,
          "Period": 5432.542323192359
        },
        "Spherical": {
          "RightAscension": 39.886758149246326,
          "Declination": 19.707033172850327,
          "RadiusMagnitude": 6678395.881730987,
          "HorizFPA": 0.0061403045944356,
          "VelocityAzimuth": 67.95208534463903,
          "VelocityMagnitude": 7726.107925022054
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20
      },
      "DurationSec": 600,
      "Results": {}
    }
  ],
  "Positions": [
    {
      "CentralBody": "Earth",
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 4,
      "referenceFrame": "INERTIAL",
      "epoch": "2025-03-25T04:00:00.000Z",
      "interval": "2025-03-25T04:00:00.000Z/2025-03-25T04:01:00.000Z",
      "cartesian": null,
      "cartesianVelocity": [
        0,
        6678137,
        0,
        0,
        0,
        6789.530297717652,
        3686.414173013652,
        19.827000000004773,
        6676380.322238555,
        134604.21346626477,
        73084.12489685729,
        -177.19279184774638,
        6787.744318052266,
        3685.4444651748927,
        39.65400000000227,
        6671111.213139144,
        269137.61190474295,
        146129.8003707578,
        -354.2923628953766,
        6782.387318656711,
        3682.5358518201165,
        49.82700000000477,
        6667045.0896118805,
        338114.6069208395,
        183581.25295867948,
        -445.0935793315429,
        6778.25337098798,
        3680.291301371372,
        60,
        6662055.548222059,
        407044.7714350037,
        221007.27865866007,
        -535.8331481627247,
        6773.180602573988,
        3677.5370128480495
      ]
    },
    {
      "CentralBody": "Earth",
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 7,
      "referenceFrame": "INERTIAL",
      "epoch": "2025-03-25T04:00:00.000Z",
      "interval": "2025-03-25T04:01:00.000Z/2025-03-25T04:11:00.000Z",
      "cartesian": null,
      "cartesianVelocity": [
        60,
        6662055.548222059,
        407044.7714350037,
        221007.27865866007,
        -535.833148162729,
        6725.464726548027,
        3765.418724114246,
        120,
        6613888.64678314,
        809268.5007477261,
        446218.8229643581,
        -1069.0854292635133,
        6676.609310040113,
        3738.618384267384,
        186.89400000000023,
        6522649.619677993,
        1253025.8468430461,
        694724.9014797315,
        -1657.4214001199575,
        6584.246943782843,
        3687.5198288719,
        253.77900000000227,
        6392387.013462071,
        1689224.6794207164,
        939039.9226731756,
        -2235.7559643105437,
        6452.4931669895595,
        3614.3599521987726,
        320.6619999999966,
        6223876.605471197,
        2115302.2721178466,
        1177728.4924173849,
        -2800.689702409116,
        6282.133285196946,
        3519.5748656153664,
        387.54899999999907,
        6018110.402167226,
        2528746.632582197,
        1409383.3946818842,
        -3348.8904949695184,
        6074.171072366411,
        3403.7230467989457,
        454.4479999999967,
        5776282.697085827,
        2927127.8442310705,
        1632643.1507702484,
        -3877.135121924757,
        5829.814652760877,
        3267.4774793061747,
        521.3669999999984,
        5499787.717450752,
        3308096.8681522403,
        1846191.3510537446,
        -4382.307713178559,
        5550.474095406795,
        3111.624304376066,
        588.3139999999985,
        5190213.151578351,
        3669397.2175587784,
        2048763.196907009,
        -4861.415260794323,
        5237.754838174378,
        2937.0591557346524,
        655.2989999999991,
        4849322.261384866,
        4008886.0922087887,
        2239157.350007958,
        -5311.615670575599,
        4893.4395796345225,
        2744.7770516425016,
        660,
        4824280.773722159,
        4031830.7600951274,
        2252027.3735829424,
        -5342.043520350771,
        4868.147894293297,
        2730.650247661656
      ]
    }
  ]
         */

    }
}