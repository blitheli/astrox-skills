using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
        测试 Astrogator 两段不同的中心天体
            MCS:
            >   InitialState   地球惯性系 Cartesian轨道参数
            >   Propagate      CisLunar, 10天
            >   Propagate      HelioCentric 积分到深空机动
        
        在返回的结果中，原始的坐标系分别为Earth和Sun 惯性系，此处的json结果都默认为Earth Inertial了

        与STK结果对比精度: 
            Cislunar 位置精度～3e-3 m, 速度精度～1e-8 m/s， 10^-11的精度
            HelioCentric 位置精度～0.04 km, 速度精度～1e-8 km/s， 10^-8的精度
            HelioCentric积分器和STK的不一样,所以这里的误差也正常

        20260111    初次编写
    */
    [TestMethod()]
    public void Cislunar2HelioCentric_20260110()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Cislunar2HelioCentric_20260110.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi

        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        var outJson = JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        //  保存到文件
        string outputFilePath = Path.Combine(filePath0, "Cislunar2HelioCentric_20260110_output.json");
        File.WriteAllText(outputFilePath, outJson, Encoding.UTF8);

        Console.WriteLine(outJson);

        //  递推10天后结果，与STK对比
        var rv = output.MainSequenceResults[1].FinalState.Cartesian;
        Assert.AreEqual(-651916213.9132958836854, rv.X, 3e-3);
        Assert.AreEqual(883083439.6853159414604, rv.Y, 3e-3);
        Assert.AreEqual(6616690.2691110390151, rv.Z, 3e-3);
        Assert.AreEqual(-598.2707008312266, rv.Vx, 1e-8);
        Assert.AreEqual(657.3838661230570, rv.Vy, 1e-8);
        Assert.AreEqual(72.5672574908721, rv.Vz, 1e-8);

        //  最后一段结果，与STK对比
        var rv2 = output.MainSequenceResults[2].FinalState.Cartesian;
        Assert.AreEqual(-5.7333127393146511e+06, rv2.X * 0.001, 0.04);
        Assert.AreEqual(-7.6684176520903930e+06, rv2.Y * 0.001, 0.04);
        Assert.AreEqual(-3.0154209065947267e+06, rv2.Z * 0.001, 0.04);
        Assert.AreEqual(1.3066361044638215, rv2.Vx * 0.001, 1e-8);
        Assert.AreEqual(-1.0580776967772181, rv2.Vy * 0.001, 1e-8);
        Assert.AreEqual(-0.3235815682885714, rv2.Vz * 0.001, 1e-8);
    }
    /*
     STK 结果：    10天后 Earth Inertial
                                                                                                        
UTC Gregorian Date: 26 Feb 2028 21:22:01.605  UTC Julian Date: 2461828.39029635                   
Julian Ephemeris Date: 2461828.39109709                                                           
Time past epoch: 5.07929e+06 sec   (Epoch in UTC Gregorian Date: 30 Dec 2027 02:27:15.141)        
                                                                                                  
State Vector in Coordinate System: Earth Inertial                                                 
                                                                                                  
Parameter Set Type:  Cartesian                                                                    
         X:  -651916.2139132958836854 km              Vx:       -0.5982707008312266 km/sec        
         Y:   883083.4396853159414604 km              Vy:        0.6573838661230570 km/sec        
         Z:     6616.6902691110390151 km              Vz:        0.0725672574908721 km/sec        
                                                                                                  
Parameter Set Type:  Keplerian                                                                    
       sma:   -5.7701631891853586e+06 km            RAAN:          125.968887829253 deg           
       ecc:        1.0033422598186184                  w:         196.5126419964362 deg           
       inc:         36.49404174354563 deg             TA:          164.068086770931 deg           
                                                                                                  
Parameter Set Type:  Spherical                                                                    
 Right Asc:         126.4357522071122 deg     Horiz. FPA:         82.71710648025216 deg           
      Decl:        0.3453783345251057 deg        Azimuth:         53.50736539519183 deg           
       |R|:    1.0976679333931608e+06 km             |V|:        0.8918225079979373 km/sec        
                                                                                                  
                                                                                                  
Other Hyperbolic Orbit Parameters :                                                               
 Ecc. Anom:         34.44952944010549 deg       Mean Anom:         2.235692488143106 deg          
 Long Peri:         322.4815298256892 deg        Arg. Lat:         0.580728767367179 deg          
 True Long:         126.5496165966202 deg        Vert FPA:         7.282893519747812 deg          
  Ang. Mom:         124096.8094143434 km^2/sec          p:    38635.2259141184840701 km           
        C3:       0.06907957858922792 km^2/sec^2   Energy:       0.03453978929461396 km^2/sec^2   
   Vel. RA:         132.3046440126458 deg       Vel. Decl:         4.667295803764802 deg          
 Rad. Peri:    19285.3845740849610593 km        Vel. Peri:        6.4347593867067836 km/sec       
  Rad. Apo:   -1.1559611762944801e+07 km       Excess Vel:        0.2628299423376795 km/sec       
               Time Past Periapsis:           856648.5641134495 sec                               
          Time Past Ascending Node:           91099.58912818076 sec                               
   Beta Angle (Orbit plane to Sun):            11.4453494642598 deg                               
Mean Sidereal Greenwich Hour Angle:            116.763898645235 deg                               
                                                                                                  
Geodetic Parameters:                                                                              
  Latitude:        0.2520284638669036 deg                                                         
 Longitude:         10.03327383050555 deg                                                         
  Altitude:    1.0912897968062160e+06 km                                                          
Geocentric Parameters:                                                                            
  Latitude:        0.2520186604551184 deg                                                         
 Longitude:         10.03327383050555 deg                                                       
    */

    /*  STK 结果  最后段状态  Earth Inertial
UTC Gregorian Date: 14 Aug 2028 21:22:01.605  UTC Julian Date: 2461998.39029635                       
Julian Ephemeris Date: 2461998.39109709                                                               
Time past epoch: 1.97673e+07 sec   (Epoch in UTC Gregorian Date: 30 Dec 2027 02:27:15.141)            
                                                                                                      
State Vector in Coordinate System: Earth Inertial                                                     
                                                                                                      
Parameter Set Type:  Cartesian                                                                        
         X:   -5.7333127393146511e+06 km              Vx:        1.3066361044638215 km/sec            
         Y:   -7.6684176520903930e+06 km              Vy:       -1.0580776967772181 km/sec            
         Z:   -3.0154209065947267e+06 km              Vz:       -0.3235815682885714 km/sec            
                                                                                                      
Parameter Set Type:  Keplerian                                                                        
       sma:  -139756.0538768190599512 km            RAAN:         353.0231478085335 deg               
       ecc:       72.5121215706635667                  w:         236.2890404797019 deg               
       inc:          19.9484492160754 deg             TA:         5.408815194386415 deg               
                                                                                                      
Parameter Set Type:  Spherical                                                                        
 Right Asc:          233.216275250883 deg     Horiz. FPA:         5.335342830366192 deg               
      Decl:        -17.48105280721294 deg        Azimuth:          99.7640031973648 deg               
       |R|:    1.0038339877569785e+07 km             |V|:        1.7121715314891393 km/sec            
                                                                                                      
                                                                                                      
Other Hyperbolic Orbit Parameters :                                                                   
 Ecc. Anom:         5.342563583200927 deg       Mean Anom:         382.6196886569455 deg              
 Long Peri:         229.3121882882354 deg        Arg. Lat:         241.6978556740883 deg              
 True Long:         234.7210034826218 deg        Vert FPA:         84.66465716963381 deg              
  Ang. Mom:         17112896.04241478 km^2/sec          p:    7.3469866178885210e+08 km               
        C3:         2.852115743417651 km^2/sec^2   Energy:         1.426057871708826 km^2/sec^2       
   Vel. RA:          321.000406728925 deg       Vel. Decl:        -10.89378869144879 deg              
 Rad. Peri:    9.9942519150752928e+06 km        Vel. Peri:        1.7122738337825723 km/sec           
  Rad. Apo:   -1.0273764022828929e+07 km       Excess Vel:         1.688820814479041 km/sec           
               Time Past Periapsis:           552626.5399453596 sec                                   
         Time Past Descending Node:           9230457.779452926 sec                                   
   Beta Angle (Orbit plane to Sun):            4.09762123159791 deg                                   
Mean Sidereal Greenwich Hour Angle:            284.323951938985 deg                                   
                                                                                                      
Geodetic Parameters:                                                                                  
  Latitude:        -17.57794597290302 deg                                                             
 Longitude:        -50.70025767112504 deg                                                             
  Altitude:    1.0031963687997136e+07 km                                                              
Geocentric Parameters:                                                                                
  Latitude:        -17.57787578834759 deg                                                             
 Longitude:        -50.70025767112504 deg                                                             

     */

    /*  STK 结果 最后段   Sun Inertial
     UTC Gregorian Date: 14 Aug 2028 21:22:01.605  UTC Julian Date: 2461998.39029635                       
Julian Ephemeris Date: 2461998.39109709                                                               
Time past epoch: 1.97673e+07 sec   (Epoch in UTC Gregorian Date: 30 Dec 2027 02:27:15.141)            
                                                                                                      
State Vector in Coordinate System: Sun Inertial                                                       
                                                                                                      
Parameter Set Type:  Cartesian                                                                        
         X:    1.1398113881404044e+08 km              Vx:       19.0919993298012827 km/sec            
         Y:   -9.2879685549550757e+07 km              Vy:       20.4321807182297768 km/sec            
         Z:   -3.9953485802152142e+07 km              Vz:        8.9927308490702114 km/sec            
                                                                                                      
Parameter Set Type:  Keplerian                                                                        
       sma:    1.5094623909085074e+08 km            RAAN:         359.3941436328549 deg               
       ecc:        0.0203676282322547                  w:         77.46115869213662 deg               
       inc:         23.54968555212376 deg             TA:         241.5197747717769 deg               
                                                                                                      
Parameter Set Type:  Spherical                                                                        
 Right Asc:         320.8244795977928 deg     Horiz. FPA:        -1.035700785734667 deg               
      Decl:         -15.2021203817282 deg        Azimuth:         71.79704671941828 deg               
       |R|:    1.5236343728359425e+08 km             |V|:       29.3742685940493224 km/sec            
                                                                                                      
                                                                                                      
Other Elliptic Orbit Parameters :                                                                     
 Ecc. Anom:         242.5506133442928 deg       Mean Anom:         243.5862120809739 deg              
 Long Peri:         76.85530232499151 deg        Arg. Lat:         318.9809334639136 deg              
 True Long:         318.3750770967685 deg        Vert FPA:         91.03570078573466 deg              
  Ang. Mom:         4474833342.187059 km^2/sec          p:    1.5088362051079044e+08 km               
        C3:        -879.2033563821559 km^2/sec^2   Energy:         -439.601678191078 km^2/sec^2       
   Vel. RA:         46.94203470250187 deg       Vel. Decl:         17.82695121909059 deg              
 Rad. Peri:    1.4787182220999128e+08 km        Vel. Peri:       30.2615689406491164 km/sec           
  Rad. Apo:    1.5402065597171023e+08 km         Vel. Apo:         29.05346243304518 km/sec           
 Mean Mot.:     1.125498717025677e-05 deg/sec                                                         
    Period:         31985820.55707373 sec          Period:         533097.0092845621 min              
    Period:         8884.950154742703 hr           Period:         370.2062564476126 day              
               Time Past Periapsis:           21642513.52722038 sec                                   
          Time Past Ascending Node:           28323156.44977686 sec                                   
   Beta Angle (Orbit plane to Sun):                          90 deg                                   
Mean Sidereal Greenwich Hour Angle:            284.323951938985 deg                                   
                                                                                                      
Heliodetic Parameters:                                                                                
  Latitude:         6.546418188476709 deg                                                             
 Longitude:        -95.69137209559841 deg                                                             
  Altitude:    1.5166773728359425e+08 km                                                              
Heliocentric Parameters:                                                                              
  Latitude:         6.546418188476709 deg                                                             
 Longitude:        -95.69137209559841 deg                                                             
     */

    /*

 标准输出: 
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
        "Epoch": "2028-02-16T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8248691.548001449,
          "Y": -8171573.9944618875,
          "Z": -1505233.408960216,
          "Vx": 4056.8221567706023,
          "Vy": 5038.530626118252,
          "Vz": -5121.631736745521
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 29376176191.863525,
          "Eccentricity": 0.9996014399343426,
          "Inclination": 39.33972001851887,
          "RAAN": 126.16877618046738,
          "ArgOfPeriapsis": 191.70113489732918,
          "MeanAnomaly": 1.0099797180795484E-15,
          "TrueAnomaly": 1.7949142289371212E-10,
          "Period": 1584542445.2390494
        },
        "Spherical": {
          "RightAscension": 315.26908686993255,
          "Declination": -7.3865410328968855,
          "RadiusMagnitude": 11708170.711792955,
          "HorizFPA": 8.972980867344874E-11,
          "VelocityAzimuth": 128.75106978463833,
          "VelocityMagnitude": 8250.800478064866
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -7.301009875795018,
        "Geodetic_Longitude": -151.26352365884276,
        "Geodetic_Altitude": 5330377.264603557,
        "Geocentric_Latitude": -7.274670218930744,
        "Geocentric_Longitude": -151.26352365884276
      },
      "FinalState": {
        "Epoch": "2028-02-16T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8248691.548001449,
          "Y": -8171573.9944618875,
          "Z": -1505233.408960216,
          "Vx": 4056.8221567706023,
          "Vy": 5038.530626118252,
          "Vz": -5121.631736745521
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 29376176191.863525,
          "Eccentricity": 0.9996014399343426,
          "Inclination": 39.33972001851887,
          "RAAN": 126.16877618046738,
          "ArgOfPeriapsis": 191.70113489732918,
          "MeanAnomaly": 1.0099797180795484E-15,
          "TrueAnomaly": 1.7949142289371212E-10,
          "Period": 1584542445.2390494
        },
        "Spherical": {
          "RightAscension": 315.26908686993255,
          "Declination": -7.3865410328968855,
          "RadiusMagnitude": 11708170.711792955,
          "HorizFPA": 8.972980867344874E-11,
          "VelocityAzimuth": 128.75106978463833,
          "VelocityMagnitude": 8250.800478064866
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -7.301009875795018,
        "Geodetic_Longitude": -151.26352365884276,
        "Geodetic_Altitude": 5330377.264603557,
        "Geocentric_Latitude": -7.274670218930744,
        "Geocentric_Longitude": -151.26352365884276
      },
      "DurationSec": 0,
      "Results": {}
    },
    {
      "$type": "PropagateResult",
      "StoppedOnMaximumDuration": false,
      "StoppingConditionName": "Duration",
      "TypeName": "Propagate",
      "Name": "递推10天",
      "Description": "轨道递推段",
      "UserComment": "轨道递推段",
      "InitialState": {
        "Epoch": "2028-02-16T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 8248691.548001449,
          "Y": -8171573.9944618875,
          "Z": -1505233.408960216,
          "Vx": 4056.8221567706023,
          "Vy": 5038.530626118252,
          "Vz": -5121.631736745521
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": 29376176191.863525,
          "Eccentricity": 0.9996014399343426,
          "Inclination": 39.33972001851887,
          "RAAN": 126.16877618046738,
          "ArgOfPeriapsis": 191.70113489732918,
          "MeanAnomaly": 1.0099797180795484E-15,
          "TrueAnomaly": 1.7949142289371212E-10,
          "Period": 1584542445.2390494
        },
        "Spherical": {
          "RightAscension": 315.26908686993255,
          "Declination": -7.3865410328968855,
          "RadiusMagnitude": 11708170.711792955,
          "HorizFPA": 8.972980867344874E-11,
          "VelocityAzimuth": 128.75106978463833,
          "VelocityMagnitude": 8250.800478064866
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -7.301009875795018,
        "Geodetic_Longitude": -151.26352365884276,
        "Geodetic_Altitude": 5330377.264603557,
        "Geocentric_Latitude": -7.274670218930744,
        "Geocentric_Longitude": -151.26352365884276
      },
      "FinalState": {
        "Epoch": "2028-02-26T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -651916213.9125766,
          "Y": 883083439.6830233,
          "Z": 6616690.269810311,
          "Vy": 657.3838661200175,
          "Vz": 72.56725749151542
        },
        "Keplerian": {
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": -5770163189.804263,
          "Eccentricity": 1.0033422598182418,
          "Inclination": 36.49404174363326,
          "RAAN": 125.96888782924506,
          "ArgOfPeriapsis": 196.51264199639638,
          "MeanAnomaly": 2.235692487782908,
          "TrueAnomaly": 164.06808677103214,
          "Period": 0
        },
        "Spherical": {
          "RightAscension": 126.43575220715312,
          "Declination": 0.3453783345623203,
          "RadiusMagnitude": 1097667933.3908935,
          "HorizFPA": 82.71710648023031,
          "VelocityAzimuth": 53.50736539510449,
          "VelocityMagnitude": 891.8225079946285
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": 0.2520284639040257,
        "Geodetic_Longitude": 10.033273830546534,
        "Geodetic_Altitude": 1091289796.803949,
        "Geocentric_Latitude": 0.25201866049224103,
        "Geocentric_Longitude": 10.033273830546532
      },
      "DurationSec": 864000,
      "Results": {}
    },
    {
      "$type": "PropagateResult",
      "StoppedOnMaximumDuration": false,
      "StoppingConditionName": "持续时间",
      "TypeName": "Propagate",
      "Name": "递推至深空机动",
      "Description": "轨道递推直到满足停止条件",
      "UserComment": "轨道递推直到满足停止条件",
      "InitialState": {
        "Epoch": "2028-02-26T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": 136107489571.33452,
          "Y": -51315719025.73256,
          "Z": -22620882170.699158,
          "Vx": 11329.690454262396,
          "Vy": 26005.92514339472,
          "Vz": 11061.196790625654
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": -429982.8761595762,
          "Eccentricity": 342343.7840153918,
          "Inclination": 23.149014681467616,
          "RAAN": 0.6719769982583124,
          "ArgOfPeriapsis": 337.5359620174232,
          "MeanAnomaly": -186389.84139271086,
          "TrueAnomaly": 359.45450947667894,
          "Period": 0
        },
        "Spherical": {
          "RightAscension": 339.342379310194,
          "Declination": -8.83943127821836,
          "RadiusMagnitude": 147208206451.67688,
          "HorizFPA": -0.545488929949941,
          "VelocityAzimuth": 68.51878099385115,
          "VelocityMagnitude": 30447.004824681873
        },
        "DryMass": 500,
        "FuelMass": 500,
        "DragArea": 20,
        "SRPArea": 20,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -8.69148132230296,
        "Geodetic_Longitude": -137.0520120075331,
        "Geodetic_Altitude": 147201828802.20462,
        "Geocentric_Latitude": -8.691478839649246,
        "Geocentric_Longitude": -137.0520120075331
      },
      "FinalState": {
        "Epoch": "2028-08-14T21:22:01.604999999995925Z",
        "CoordSystemName": "Earth Inertial",
        "Cartesian": {
          "X": -5733312739.052017,
          "Y": -7668417685.973358,
          "Z": -3015420922.812477,
          "Vx": 1306.6361083117372,
          "Vy": -1058.0777043991438,
          "Vz": -323.58157157920687
        },
        "Keplerian": {
          "ElementType": "Osculating",
          "GravitationalParameter": 398600441500000,
          "SemiMajorAxis": -139756052.47752503,
          "Eccentricity": 72.51212247501915,
          "Inclination": 19.94844924482212,
          "RAAN": 353.02314783158397,
          "ArgOfPeriapsis": 236.28904032628714,
          "MeanAnomaly": 382.619711648489,
          "TrueAnomaly": 5.408815448571289,
          "Period": 0
        },
        "Spherical": {
          "RightAscension": 233.21627537355224,
          "Declination": -17.48105284924627,
          "RadiusMagnitude": 10038339908.175076,
          "HorizFPA": 5.3353430820120336,
          "VelocityAzimuth": 99.76400318106971,
          "VelocityMagnitude": 1712.171539757706
        },
        "DryMass": 500,
        "FuelMass": 500,
        "Cd": 2.2,
        "Cr": 1,
        "DragArea": 20,
        "SRPArea": 20,
        "Geodetic_Latitude": -17.577946014660153,
        "Geodetic_Longitude": -50.700257548286906,
        "Geodetic_Altitude": 10031963718.602428,
        "Geocentric_Latitude": -17.57787583010482,
        "Geocentric_Longitude": -50.700257548286906
      },
      "DurationSec": 14688000,
      "Results": {}
    }
  ],
  "Positions": {
    "CentralBody": "Earth",
    "CzmlPositions": [
      {
        "CentralBody": "Earth",
    */
}