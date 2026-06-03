using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator  段Results参数

        # 飞行任务各段简要说明

        ## 主序列飞行段
        
        1. **InitialState（初始状态）**
        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：Duration

          Resuts:            
            - 大部分参数


        结果: 注意，内部加载了EOP-v1.1.txt，和STK使用的一致，否则会导致经纬度有差别!

            与STK对比，位置误差: < 1e-5 m
                       速度误差：< 1e-8 m/s
                        角度误差: <1e-8°
                平均轨道根数误差稍大，具体为：
                        半长轴: < 0.1m;    
                        纬度幅角，RAAN等 < 1e-4°
                相对平均轨道根数        


            总体来说，非常精确！

        */
        [TestMethod()]
        public void EarthPointMass_Duration_Results_250628()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Propagate");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "EarthPointMass_Duration_Results_250628.json");

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


            /*
              STK 结果                                                                         
            UTC Gregorian Date: 20 Jun 2022 05:00:00.000  UTC Julian Date: 2459750.70833333                           
Julian Ephemeris Date: 2459750.70913407                                                                   
Time past epoch: 3600 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)                       
                                                                                                          
State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                          
Parameter Set Type:  Cartesian                                                                            
         X:    -4849.3021322189306375 km              Vx:       -4.5019972992209425 km/sec                
         Y:    12356.4893113509569957 km              Vy:       -0.9013912462590279 km/sec                
         Z:     3295.0638163602561690 km              Vz:       -0.2403709990024070 km/sec                
                                                                                                          
Parameter Set Type:  Keplerian                                                                            
       sma:    10729.4551073083675874 km            RAAN:     2.049056751003762e-14 deg                   
       ecc:        0.3136623287960019                  w:         311.8404058567693 deg                   
       inc:         14.93141717813755 deg             TA:         158.9262583280531 deg                   
                                                                                                          
Parameter Set Type:  Spherical                                                                            
 Right Asc:         111.4275164644725 deg     Horiz. FPA:         9.059689826289341 deg                   
      Decl:         13.94101446274881 deg        Azimuth:         95.40126653476493 deg                   
       |R|:    13676.8419170859815495 km             |V|:        4.5976367927650044 km/sec   
                                                                                                                   
User-selected results:                                                                                    
    Altitude =     7299.9401904626820397 km                                                               
    Latitude =          13.9388873902707 deg                                                              
    Longitude =         128.3489083024687 deg                                                             
    Altitude Of Apoapsis =     7716.7439829788654606 km                                                   
    Altitude Of Periapsis =      985.8922316378681217 km                                                  
    Argument of Latitude =         110.7666641848224 deg                                                  
    Argument of Periapsis =         311.8404058567693 deg                                                 
    Eccentric Anomaly =         151.1379217812175 deg                                                     
    Eccentricity =        0.3136623287960019                                                              
    Inclination =         14.93141717813755 deg                                                           
    MeanAnomaly =         142.4630143082589 deg                                                           
    Orbit Period =    11060.5692992454969499 sec                                                          
    RAAN =     2.049056751003762e-14 deg                                                                  
    Radius Of Apoapsis =    14094.8809829788660863 km                                                     
    Radius Of Periapsis =     7364.0292316378681790 km                                                    
    Semimajor Axis =    10729.4551073083675874 km                                                         
    True Anomaly =         158.9262583280531 deg                                                          
    Mean Argument of Latitude =         110.7304537532023 deg                                             
    Mean Argument of Perigee =         311.7768523598801 deg                                              
    Mean Eccentricity =        0.3139911779979170                                                         
    Mean Inclination =         14.93396423510713 deg                                                      
    Mean Mean Anomaly =          142.487872793572 deg                                                     
    Mean Orbit Period =    11064.7624523452886933 sec                                                     
    Mean RAAN =       0.02246369625017967 deg                                                             
    Mean Semimajor Axis =    10732.1666859808156005 km                                                    
    Mean True Anomaly =          158.953601394993 deg          
    Mean Radius Of Periapsis =     7362.3610257796981386 km                                               
    Mean Radius Of Apoapsis =    14101.9723461819303338 km   
    Rel Mean Arg of Lat =         5.002133366846048 deg                                                   
    Rel Mean Eccentricity =       -0.0406498426701604                                                     
    Rel Mean RAAN =       -0.7183720689515808 deg                                                         
    Rel Mean Inclination =        0.1495026203017983 deg                                                  
    Rel Mean Arg of Perigee =        0.6570946451662757 deg                                               
    Rel Mean Period =     -869.8630172549510462 sec                                                       
    Rel Mean Mean Anomaly =         10.62728561440139 deg                                                 
    Rel Mean Semimajor Axis =     -555.3535685573201590 km         
            */

            //  最后一段
            var seg = output.MainSequenceResults[1];
            //Assert.AreEqual(-10000.0000062996914494, seg3.FinalState.Cartesian.X*0.001, 1e-5);            
            Assert.AreEqual(-4849302.1322189306375, (double)seg.Results["X"], 1e-5);
            Assert.AreEqual(12356489.3113509569957,(double)seg.Results["Y"], 1e-5);
            Assert.AreEqual(3295063.8163602561690, (double)seg.Results["Z"], 1e-5);
            Assert.AreEqual(-4501.9972992209425, (double)seg.Results["Vx"], 1e-8);
            Assert.AreEqual(-901.3912462590279, (double)seg.Results["Vy"], 1e-8);
            Assert.AreEqual(-240.3709990024070, (double)seg.Results["Vz"], 1e-8);
            Assert.AreEqual(3600, (double)seg.Results["Duration"], 1e-8);
            Assert.AreEqual("2022-06-20T05:00:00Z", (string)seg.Results["Epoch"]);
            Assert.AreEqual(111.4275164644725, (double)seg.Results["Right_Asc"], 1e-8);
            Assert.AreEqual(13.94101446274881, (double)seg.Results["Decl"], 1e-8);
            Assert.AreEqual(13676841.9170859815495, (double)seg.Results["RMag"], 1e-5);
            Assert.AreEqual(9.059689826289341, (double)seg.Results["Horiz_FPA"], 1e-8);
            Assert.AreEqual(95.40126653476493, (double)seg.Results["VelAzimuth"], 1e-8);
            Assert.AreEqual(4597.6367927650044, (double)seg.Results["VelMag"], 1e-8);

            Assert.AreEqual(13.9388873902707, (double)seg.Results["Latitude"], 1e-8);
            Assert.AreEqual(128.3489083024687, (double)seg.Results["Longitude"], 1e-8);
            Assert.AreEqual(7299940.190462682, (double)seg.Results["Height"], 1e-5);
            //  轨道参数
            Assert.AreEqual(10729455.107308367, (double)seg.Results["SemimajorAxis"], 1e-5);
            Assert.AreEqual(0.3136623287960019, (double)seg.Results["Eccentricity"], 1e-11);
            Assert.AreEqual(14.93141717813755, (double)seg.Results["Inclination"], 1e-8);
            Assert.AreEqual(0, (double)seg.Results["RAAN"], 1e-11);
            Assert.AreEqual(311.8404058567693, (double)seg.Results["ArgumentOfPeriapsis"], 1e-8);
            Assert.AreEqual(110.7666641848224, (double)seg.Results["ArgumentOfLatitude"], 1e-8);
            Assert.AreEqual(158.9262583280531, (double)seg.Results["TrueAnomaly"], 1e-8);
            Assert.AreEqual(142.4630143082589, (double)seg.Results["MeanAnomaly"], 1e-8);
            Assert.AreEqual(11060.56929924549, (double)seg.Results["Period"], 1e-8);

            Assert.AreEqual(7716743.982978865,(double)seg.Results["AltitudeOfApoapsis"],1e-5);
            Assert.AreEqual(985892.2316378681,(double)seg.Results["AltitudeOfPeriapsis"], 1e-5);
            Assert.AreEqual(14094880.98297886, (double)seg.Results["RadiusOfApoapsis"], 1e-5);
            Assert.AreEqual(7364029.231637868, (double)seg.Results["RadiusOfPeriapsis"], 1e-5);

            //  平均轨道参数
            Assert.AreEqual(10732166.685980815, (double)seg.Results["Mean SemimajorAxis"], 0.1);
            Assert.AreEqual(0.3139911779979170, (double)seg.Results["Mean Eccentricity"], 1e-8);
            Assert.AreEqual(14.93396423510713, (double)seg.Results["Mean Inclination"], 1e-5);
            Assert.AreEqual(311.7768523598801, (double)seg.Results["Mean ArgumentOfPeriapsis"], 1e-4);
            Assert.AreEqual(110.7304537532023, (double)seg.Results["Mean ArgumentOfLatitude"], 1e-4);
            Assert.AreEqual(0.02246369625017967, (double)seg.Results["Mean RAAN"], 1e-4);
            Assert.AreEqual(158.953601394993, (double)seg.Results["Mean TrueAnomaly"], 1e-4);
            Assert.AreEqual(142.487872793572, (double)seg.Results["Mean MeanAnomaly"], 1e-6);
            Assert.AreEqual(11064.76245234529, (double)seg.Results["Mean Period"], 1e-4);

            Assert.AreEqual(14101972.34618193, (double)seg.Results["Mean RadiusOfApoapsis"], 0.1);
            Assert.AreEqual(7362361.025779698, (double)seg.Results["Mean RadiusOfPeriapsis"], 0.1);
            //  相对轨道根数
            Assert.AreEqual(5.002133366846048, (double)seg.Results["Rel Mean ArgumentOfLatitude"], 1e-4);
            Assert.AreEqual(-0.0406498426701604, (double)seg.Results["Rel Mean Eccentricity"], 1e-8);
            Assert.AreEqual(-0.7183720689515808, (double)seg.Results["Rel Mean RAAN"], 1e-4);
            Assert.AreEqual(0.1495026203017983, (double)seg.Results["Rel Mean Inclination"], 1e-4);
            Assert.AreEqual(0.6570946451662757, (double)seg.Results["Rel Mean ArgumentOfPeriapsis"], 1e-4);
            Assert.AreEqual(-869.8630172549510462, (double)seg.Results["Rel Mean Period"], 1e-4);
            Assert.AreEqual(10.62728561440139, (double)seg.Results["Rel Mean MeanAnomaly"], 1e-4);
            Assert.AreEqual(-555353.5685573201590, (double)seg.Results["Rel Mean SemimajorAxis"], 1e-3);



        }
    }
}
/*
  "Results": {
       "X": -4849302.132221642,
        "Y": 12356489.311346067,
        "Z": 3295063.816358952,
        "Vx": -4501.9972992212515,
        "Vy": -901.3912462619953,
        "Vz": -240.37099900319907,
        "Duration": 3600,
        "Epoch": "2022-06-20T05:00:00Z",
        "Right_Asc": 111.42751646449112,
        "Decl": 13.941014462747091,
        "RMag": 13676841.917082211,
        "Vert_FPA": 80.94031017372322,
        "Horiz_FPA": 9.05968982627678,
        "VelAzimuth": 95.40126653476942,
        "VelMag": 4597.636792765931,
        "Latitude": 13.938887390087409,
        "Longitude": 128.3489083025136,
        "Height": 7299940.190457071,
        "Delta_Decl": 23.373858859136327,
        "Delta_RA": 121.48840118231513,
        "Delta_RMag": -363835936.8611917,
        "RadiusOfPeriapsis": 7364029.231638809,
        "RadiusOfApoapsis": 14094880.982973557,
        "SemimajorAxis": 10729455.107306182,
        "Period": 11060.569299242119,
        "Eccentricity": 0.31366232879577455,
        "Inclination": 14.931417178137558,
        "ArgumentOfPeriapsis": 311.8404058567626,
        "ArgumentOfLatitude": 110.76666418484052,
        "RAAN": 0,
        "TrueAnomaly": 158.92625832807798,
        "MeanAnomaly": 142.46301430831676,
        "AltitudeOfPeriapsis": 985892.2316388087,
        "AltitudeOfApoapsis": 7716743.982973557,
        "Mean RadiusOfPeriapsis": 7362361.019539607,
        "Mean RadiusOfApoapsis": 14101972.372731116,
        "Mean SemimajorAxis": 10732166.69613536,
        "Mean Period": 11064.76246804915,
        "Mean Eccentricity": 0.313991179228442,
        "Mean Inclination": 14.93396694103129,
        "Mean ArgumentOfPeriapsis": 311.77689889372454,
        "Mean ArgumentOfLatitude": 110.73050039094643,
        "Mean RAAN": 0.022418585108081424,
        "Mean TrueAnomaly": 158.95360149722185,
        "Mean MeanAnomaly": 142.4878728865238,
        "Mean AltitudeOfPeriapsis": 984224.0195396068,
        "Mean AltitudeOfApoapsis": 7723835.372731116
        "Rel Mean SemimajorAxis": -555353.569115337,
        "Rel Mean Period": -869.8630185411475,
        "Rel Mean Eccentricity": -0.04064984249893888,
        "Rel Mean Inclination": 0.14950276986083644,
        "Rel Mean ArgumentOfPeriapsis": 0.6570940744621225,
        "Rel Mean ArgumentOfLatitude": 5.002132801771069,
        "Rel Mean RAAN": -0.7183714749775907,
        "Rel Mean TrueAnomaly": 4.345038727308918,
        "Rel Mean MeanAnomaly": 10.627285623522226
      }

*/
