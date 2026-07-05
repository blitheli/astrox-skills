using AeroSpace.OrbitCore;
using ASTROX.Coordinates;

namespace ASTROX.Astrogator.Tests;

public partial class AgVAElementTests
{
    static double AngleDiff(double aDeg, double bDeg)
    {
        double d = (aDeg - bDeg) % 360.0;
        if (d > 180.0) d -= 360.0;
        if (d < -180.0) d += 360.0;
        return Math.Abs(d);
    }

    [TestMethod()]
    public void AgVAElementTargetVector_RoundTrip_260620()
    {
        double mu = OrbitBase.EarthMu;

        var cases = new (string tag, double rp, double c3, double rao, double deco, double velAz, double ta)[]
        {
            ("A", 6678.137,  10.0,  350.0,  5.0,  90.0,  0.0),
            ("B", 7000.0,     2.0,  120.0, -30.0,  60.0, 20.0),
            ("C", 7000.0,   -20.0,  200.0,  15.0, 110.0, 40.0),
        };

        Motion<Cartesian> rvA = default;

        foreach (var (tag, rp, c3, rao, deco, velAz, ta) in cases)
        {
            var e1 = new AgVAElementTargetVectorOutgoingAsymptote
            {
                GravitationalParameter = mu,
                RadiusOfPeriapsis = rp,
                C3 = c3,
                AsympRA = rao,
                AsympDec = deco,
                VelAzAtPeriapsis = velAz,
                TrueAnomaly = ta
            };

            Motion<Cartesian> rv = e1.ToRV();
            if (tag == "A") rvA = rv;

            var e2 = new AgVAElementTargetVectorOutgoingAsymptote(rv, mu);

            Console.WriteLine($"{tag}: rp={e2.RadiusOfPeriapsis}, C3={e2.C3}, RAO={e2.AsympRA}, DecO={e2.AsympDec}, VelAz={e2.VelAzAtPeriapsis}, TA={e2.TrueAnomaly}");

            Assert.AreEqual(rp, e2.RadiusOfPeriapsis, 1e-3, $"{tag} rp");
            Assert.AreEqual(c3, e2.C3, 1e-3, $"{tag} C3");
            Assert.AreEqual(0.0, AngleDiff(rao, e2.AsympRA), 1e-3, $"{tag} RAO");
            Assert.AreEqual(deco, e2.AsympDec, 1e-3, $"{tag} DecO");
            Assert.AreEqual(0.0, AngleDiff(velAz, e2.VelAzAtPeriapsis), 1e-3, $"{tag} VelAz");
            Assert.AreEqual(0.0, AngleDiff(ta, e2.TrueAnomaly), 1e-3, $"{tag} TA");

            //  RV->参数->RV 重建(亚米级精度,相对量级~1e-7)
            Motion<Cartesian> rv2 = e2.ToRV();
            Assert.AreEqual(rv.Value.X, rv2.Value.X, 1.0, $"{tag} rx");
            Assert.AreEqual(rv.Value.Y, rv2.Value.Y, 1.0, $"{tag} ry");
            Assert.AreEqual(rv.Value.Z, rv2.Value.Z, 1.0, $"{tag} rz");
            Assert.AreEqual(rv.FirstDerivative.X, rv2.FirstDerivative.X, 1e-3, $"{tag} vx");
            Assert.AreEqual(rv.FirstDerivative.Y, rv2.FirstDerivative.Y, 1e-3, $"{tag} vy");
            Assert.AreEqual(rv.FirstDerivative.Z, rv2.FirstDerivative.Z, 1e-3, $"{tag} vz");
        }

        double rpM = 6678.137 * 1000.0;
        double e = 1.0 + rpM * (10.0 * 1.0e6) / mu;
        double vP = Math.Sqrt(mu * (1.0 + e) / rpM);
        Assert.AreEqual(rpM, rvA.Value.Magnitude, 1.0, "A |r|@peri == rp");
        Assert.AreEqual(vP, rvA.FirstDerivative.Magnitude, 1e-3, "A |v|@peri");
        Assert.AreEqual(0.0, rvA.Value.Dot(rvA.FirstDerivative) / (rvA.Value.Magnitude * rvA.FirstDerivative.Magnitude), 1e-9, "A FPA@peri==0");
    }
    
    //   椭圆轨道
    [TestMethod()]
    public void AgVAElementTargetVector_C3Less0_260620()
    {
        double mu = OrbitBase.EarthMu;
        double rp = 6678.137;
        double c3 = -2.0;
        double rao = 180.0;
        double deco = 10.0;
        double velAz = 61.5;
        double ta = 10.0;
        var e1 = new AgVAElementTargetVectorOutgoingAsymptote
        {
            GravitationalParameter = mu,
            RadiusOfPeriapsis = rp,
            C3 = c3,
            AsympRA = rao,
            AsympDec = deco,
            VelAzAtPeriapsis = velAz,
            TrueAnomaly = ta
        };
        Motion<Cartesian> rv = e1.ToRV();

        Console.WriteLine($"rv: {rv.Value.X}, {rv.Value.Y}, {rv.Value.Z}, {rv.FirstDerivative.X}, {rv.FirstDerivative.Y}, {rv.FirstDerivative.Z}");
        Assert.AreEqual(6622.2988650449706256, rv.Value.X * 0.001, 1e-6, "rx");
        Assert.AreEqual(1026.7837063020722326, rv.Value.Y * 0.001, 1e-6, "ry");
        Assert.AreEqual(-601.5915923043158955, rv.Value.Z * 0.001, 1e-6, "rz");
        Assert.AreEqual(-0.0514002933711568, rv.FirstDerivative.X * 0.001, 1e-6, "vx");
        Assert.AreEqual(9.4475160105643479, rv.FirstDerivative.Y * 0.001, 1e-6, "vy");
        Assert.AreEqual(5.2177780048133409, rv.FirstDerivative.Z * 0.001, 1e-6, "vz");
        
        var e2 = new AgVAElementTargetVectorOutgoingAsymptote(rv, mu);
        Assert.AreEqual(rp, e2.RadiusOfPeriapsis, 1e-3);
        Assert.AreEqual(c3, e2.C3, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(rao, e2.AsympRA), 1e-6);
        Assert.AreEqual(deco, e2.AsympDec, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(velAz, e2.VelAzAtPeriapsis), 1e-6);
        Assert.AreEqual(0.0, AngleDiff(ta, e2.TrueAnomaly), 1e-6);
    
        /*
        STK 结果:
        原始参数：
        RadiusOfPeriapsis: 6678.137 km
        C3: -2.0 km^2/sec^2
        RAO: 180 deg
        DecO: 10 deg
        VelAz: 61.5 deg
        TA: 10 deg

        UTC Gregorian Date: 23 May 2022 04:00:00.000  UTC Julian Date: 2459722.66666667                
Julian Ephemeris Date: 2459722.66746741                                                        
Time past epoch: 0 sec   (Epoch in UTC Gregorian Date: 23 May 2022 04:00:00.000)               
                                                                                               
State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
Parameter Set Type:  Cartesian                                                                 
         X:     6622.2988650449706256 km              Vx:       -0.0514002933711568 km/sec     
         Y:     1026.7837063020722326 km              Vy:        9.4475160105643479 km/sec     
         Z:     -601.5915923043158955 km              Vz:        5.2177780048133409 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:   199300.2207499962241855 km            RAAN:         17.73532408953673 deg        
       ecc:        0.9664920742442272                  w:         339.7189517159997 deg        
       inc:         30.06405172225313 deg             TA:         9.999999999999996 deg        
                                                                                               
Parameter Set Type:  Spherical                                                                 
 Right Asc:         8.813499004837183 deg     Horiz. FPA:         4.914585925960354 deg        
      Decl:         -5.12973004894171 deg        Azimuth:         60.33641168160309 deg        
       |R|:     6728.3756941355895833 km             |V|:       10.7927479386642879 km/sec     
                                                                                               
                                                                                               
Other Elliptic Orbit Parameters :                                                              
 Ecc. Anom:         1.308618942946573 deg       Mean Anom:       0.04395906520069321 deg       
 Long Peri:         357.4542758055365 deg        Arg. Lat:         349.7189517159997 deg       
 True Long:         7.454275805536466 deg        Vert FPA:         85.08541407403965 deg       
  Ang. Mom:         72350.68545365296 km^2/sec          p:    13132.5034812171143130 km        
        C3:        -2.000000000000038 km^2/sec^2   Energy:        -1.000000000000019 km^2/sec^2
   Vel. RA:         90.31172117785387 deg       Vel. Decl:         28.91111608304122 deg       
 Rad. Peri:     6678.1369999999969878 km        Vel. Peri:       10.8339624439649835 km/sec    
  Rad. Apo:   391922.3044999924022704 km         Vel. Apo:        0.1846046642993608 km/sec    
 Mean Mot.:     0.0004065648705717344 deg/sec                                                  
    Period:         885467.5503413458 sec          Period:          14757.7925056891 min       
    Period:         245.9632084281516 hr           Period:         10.24846701783965 day       
               Time Past Periapsis:           108.1231271626481 sec                            
          Time Past Ascending Node:           885352.9063031425 sec                            
   Beta Angle (Orbit plane to Sun):          -0.579073319879428 deg                            
Mean Sidereal Greenwich Hour Angle:            300.755841276529 deg                            
                                                                                               
Geodetic Parameters:                                                                           
  Latitude:        -5.039612851354567 deg                                                      
 Longitude:         68.34297329093603 deg                                                      
  Altitude:      350.4024003431134133 km                                                       
Geocentric Parameters:                                                                         
  Latitude:        -5.007795787420295 deg                                                      
 Longitude:         68.34297329093603 deg                                                      

        */
    }

    //   抛物线轨道
    [TestMethod()]
    public void AgVAElementTargetVector_C3Equal0_260620()
    {
        double mu = OrbitBase.EarthMu;
        double rp = 6678.137;
        double c3 = 0.0;
        double rao = 180.0;
        double deco = 10.0;
        double velAz = 61.5;
        double ta = 10.0;
        var e1 = new AgVAElementTargetVectorOutgoingAsymptote
        {
            GravitationalParameter = mu,
            RadiusOfPeriapsis = rp,
            C3 = c3,
            AsympRA = rao,
            AsympDec = deco,
            VelAzAtPeriapsis = velAz,
            TrueAnomaly = ta
        };
        Motion<Cartesian> rv = e1.ToRV();

        Console.WriteLine($"rv: {rv.Value.X}, {rv.Value.Y}, {rv.Value.Z}, {rv.FirstDerivative.X}, {rv.FirstDerivative.Y}, {rv.FirstDerivative.Z}");
        Assert.AreEqual(6623.1625745229130189, rv.Value.X * 0.001, 1e-6, "rx");
        Assert.AreEqual(1026.9176239697858364, rv.Value.Y * 0.001, 1e-6, "ry");
        Assert.AreEqual(-601.6700545378564584, rv.Value.Z * 0.001, 1e-6, "rz");
        Assert.AreEqual(-0.0358006432776158, rv.FirstDerivative.X * 0.001, 1e-6, "vx");
        Assert.AreEqual(9.5289090224875732, rv.FirstDerivative.Y * 0.001, 1e-6, "vy");
        Assert.AreEqual(5.2599019105163416, rv.FirstDerivative.Z * 0.001, 1e-6, "vz");

        var e2 = new AgVAElementTargetVectorOutgoingAsymptote(rv, mu);
        Assert.AreEqual(rp, e2.RadiusOfPeriapsis, 1e-3);
        Assert.AreEqual(c3, e2.C3, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(rao, e2.AsympRA), 1e-6);
        Assert.AreEqual(deco, e2.AsympDec, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(velAz, e2.VelAzAtPeriapsis), 1e-6);
        Assert.AreEqual(0.0, AngleDiff(ta, e2.TrueAnomaly), 1e-6);

        /*
        STK 结果:
        原始参数：
        RadiusOfPeriapsis: 6678.137 km
        C3: -2.0 km^2/sec^2
        RAO: 180 deg
        DecO: 10 deg
        VelAz: 61.5 deg
        TA: 10 deg

Parameter Set Type:  Cartesian                                                                 
         X:     6623.1625745229130189 km              Vx:       -0.0358006432776158 km/sec     
         Y:     1026.9176239697858364 km              Vy:        9.5289090224875732 km/sec     
         Z:     -601.6700545378564584 km              Vz:        5.2599019105163416 km/sec     
                                                                                               
                                                                                               
Parameter Set Type:  Spherical                                                                 
 Right Asc:         8.813499004837158 deg     Horiz. FPA:         4.999999999999995 deg        
      Decl:        -5.129730048941712 deg        Azimuth:         60.33641168160307 deg        
       |R|:     6729.2532386222701462 km             |V|:       10.8842986431445183 km/sec     
                                                                                               
                                                                                               
Other Parabolic Orbit Parameters                                                               
 Ecc. Anom:         18319.64588838536 deg       Mean Anom:         18319.64588838536 deg       
 Long Peri:         357.4542758055365 deg        Arg. Lat:         349.7189517159997 deg       
 True Long:         7.454275805536466 deg        Vert FPA:                        85 deg       
  Ang. Mom:         72964.48939857638 km^2/sec          p:    13356.2739999999866995 km        
        C3:                         0 km^2/sec^2   Energy:                         0 km^2/sec^2
   Vel. RA:         90.21526243010236 deg       Vel. Decl:         28.89826903540184 deg       
 Rad. Peri:     6678.1369999999951688 km        Vel. Peri:       10.9258748957346086 km/sec    
  Rad. Apo:   Infinite            Vel. Apo:                         0 km/sec                   
               Time Past Periapsis:            107.222896783404 sec                            
          Time Past Ascending Node:          -113.7512080105916 sec                            
   Beta Angle (Orbit plane to Sun):          -0.579073319879441 deg                            
Mean Sidereal Greenwich Hour Angle:            300.755841276529 deg                            
                                                                                               
Geodetic Parameters:                                                                           
  Latitude:        -5.039608676087652 deg                                                      
 Longitude:           68.342973290936 deg                                                      
  Altitude:      351.2799446945078898 km                                                       
Geocentric Parameters:                                                                         
  Latitude:        -5.007795787420298 deg                                                      
 Longitude:           68.342973290936 deg        

        */
    }

    //   双曲线轨道
    [TestMethod()]
    public void AgVAElementTargetVector_C3MoreThan0_260620()
    {
        double mu = OrbitBase.EarthMu;
        double rp = 6678.137;
        double c3 = 5.0;
        double rao = 180.0;
        double deco = 10.0;
        double velAz = 61.5;
        double ta = 10.0;
        var e1 = new AgVAElementTargetVectorOutgoingAsymptote
        {
            GravitationalParameter = mu,
            RadiusOfPeriapsis = rp,
            C3 = c3,
            AsympRA = rao,
            AsympDec = deco,
            VelAzAtPeriapsis = velAz,
            TrueAnomaly = ta
        };
        Motion<Cartesian> rv = e1.ToRV();

        Console.WriteLine($"rv: {rv.Value.X}, {rv.Value.Y}, {rv.Value.Z}, {rv.FirstDerivative.X}, {rv.FirstDerivative.Y}, {rv.FirstDerivative.Z}");
        Assert.AreEqual(5881.0882925464293294, rv.Value.X * 0.001, 1e-6, "rx");
        Assert.AreEqual(3193.6907996545583046, rv.Value.Y * 0.001, 1e-6, "ry");
        Assert.AreEqual(723.7883289160490676, rv.Value.Z * 0.001, 1e-6, "rz");
        Assert.AreEqual(-4.2308863346957688, rv.FirstDerivative.X * 0.001, 1e-6, "vx");
        Assert.AreEqual(8.6625208052953830, rv.FirstDerivative.Y * 0.001, 1e-6, "vy");
        Assert.AreEqual(5.5219414446873225, rv.FirstDerivative.Z * 0.001, 1e-6, "vz");

        var e2 = new AgVAElementTargetVectorOutgoingAsymptote(rv, mu);
        Assert.AreEqual(rp, e2.RadiusOfPeriapsis, 1e-3);
        Assert.AreEqual(c3, e2.C3, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(rao, e2.AsympRA), 1e-6);
        Assert.AreEqual(deco, e2.AsympDec, 1e-6);
        Assert.AreEqual(0.0, AngleDiff(velAz, e2.VelAzAtPeriapsis), 1e-6);
        Assert.AreEqual(0.0, AngleDiff(ta, e2.TrueAnomaly), 1e-6);

        /*
        STK 结果:
        原始参数：
        RadiusOfPeriapsis: 6678.137 km
        C3: 5.0 km^2/sec^2
        RAO: 180 deg
        DecO: 10 deg
        VelAz: 61.5 deg
        TA: 10 deg

Parameter Set Type:  Cartesian                                                                 
         X:     5881.0882925464293294 km              Vx:       -4.2308863346957688 km/sec     
         Y:     3193.6907996545583046 km              Vy:        8.6625208052953830 km/sec     
         Z:      723.7883289160490676 km              Vz:        5.5219414446873225 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:   -79720.0883000010653632 km            RAAN:         17.73532408953673 deg        
       ecc:        1.0837698143894292                  w:         2.394133766812335 deg        
       inc:         30.06405172225313 deg             TA:         10.00000000000001 deg        
                                                                                               
Parameter Set Type:  Spherical                                                                 
 Right Asc:         28.50391539625971 deg     Horiz. FPA:           5.2015164178694 deg        
      Decl:         6.172684984062722 deg        Azimuth:         60.51829018995704 deg        
       |R|:     6731.3245333737941110 km             |V|:       11.1099731411847369 km/sec     
                                                                                               
                                                                                               
Other Hyperbolic Orbit Parameters :                                                            
 Ecc. Anom:         2.010332377257266 deg       Mean Anom:        0.1688522353086683 deg       
 Long Peri:         20.12945785634906 deg        Arg. Lat:         12.39413376681234 deg       
 True Long:         30.12945785634907 deg        Vert FPA:          84.7984835821306 deg       
  Ang. Mom:         74476.87078649861 km^2/sec          p:    13915.7002969571731228 km        
        C3:         4.999999999999933 km^2/sec^2   Energy:         2.499999999999967 km^2/sec^2
   Vel. RA:          116.031476911322 deg       Vel. Decl:         29.80341186217986 deg       
 Rad. Peri:     6678.1369999999969878 km        Vel. Peri:       11.1523424551635646 km/sec    
  Rad. Apo:  -166118.3136000021477230 km       Excess Vel:         2.236067977499775 km/sec    
               Time Past Periapsis:           105.0671497191069 sec                            
          Time Past Ascending Node:           130.0963222396373 sec                            
   Beta Angle (Orbit plane to Sun):          -0.579073319879428 deg                            
Mean Sidereal Greenwich Hour Angle:            300.755841276529 deg                            
                                                                                    

        */
    }
}
