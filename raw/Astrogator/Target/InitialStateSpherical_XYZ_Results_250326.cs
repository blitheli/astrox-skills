using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTargetTests
    {
        /*
         测试 Astrogator 
            自变量: InitialState.Spherical.Right_Asc(Decl,VMag)
            约束: X,Y,Z
            
        ** 输出所有Results参数，进行测试
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **InitialState（初始状态）**
          - 初始化航天器在地心惯性坐标系中的球坐标: 赤经、赤纬、地心距、水平路径角、速度方位角、速度大小
          - 初始赤经：0 deg
          - 初始赤纬：0 deg
          - 初始地心距：7000 km
          - 初始水平路径角：0 deg
          - 初始速度方位角：45 deg
          - 初始速度大小：8000 m/s
          - 设置航天器物理参数：干质量100kg，燃料900kg          

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：Duration

          Resuts:            
            - X，Y，Z

        ## 目标求解配置

        **DC: Epoch_Altitude（霍曼转移微分校正器）**
          输出优化过程
          - 控制变量：
            - InitialState.Spherical.Right_Asc
            - InitialState.Spherical.Decl
            - InitialState.Spherical.VMag
          - 约束条件：
            - X
            - Y
            - Z
            - 其它所有Results参数

        结果:
            与STK对比，速度相对误差： 为1e-8
            STK设置的XYZ位置约束精度： 1e-5km,相对误差为1e-8，精度相当

            以自己的Results结果为对比，为了预防以后代码修改产生错误
            和STK结果对比，也基本一致！！

        */
        [TestMethod()]
        public void InitialStateSpherical_XYZ_Results_250326()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateSpherical_XYZ_Results_250326.json");

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
            
            初始段：
            UTC Gregorian Date: 1 Jan 2000 11:58:55.816  UTC Julian Date: 2451544.99925713                 
Julian Ephemeris Date: 2451545                                                                 
Time past epoch: -7.0897e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)     
                                                                                               
State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
Parameter Set Type:  Cartesian                                                                 
         X:     5586.8667412540753503 km              Vx:       -2.7894726990214216 km/sec     
         Y:     -867.8480082937785482 km              Vy:        7.0117761326708097 km/sec     
         Z:     4127.1975782568952127 km              Vz:        5.2504315066717240 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:    13570.4637784778915375 km            RAAN:         320.6468166901589 deg        
       ecc:        0.4841738562316739                  w:         45.91176678252599 deg        
       inc:         55.17114315011612 deg             TA:      9.54166404439055e-15 deg        
                                                                                               
Parameter Set Type:  Spherical                                                                 
 Right Asc:         351.1704034008877 deg     Horiz. FPA:                         0 deg        
      Decl:         36.12860358283428 deg        Azimuth:                        45 deg        
       |R|:     6999.9999999999990905 km             |V|:        9.1931057580955180 km/sec     

        结束段：
        UTC Gregorian Date: 1 Jan 2000 12:48:55.816  UTC Julian Date: 2451545.03397935                            
        Julian Ephemeris Date: 2451545.03472222                                                                   
        Time past epoch: -7.08967e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)               
                                                                                                          
        State Vector in Coordinate System: Earth Inertial                                                         
                                                                                                          
        Parameter Set Type:  Cartesian                                                                            
                 X:   -10000.0000062996914494 km              Vx:       -3.9373512839336793 km/sec                
                 Y:     9999.9999897437755862 km              Vy:        0.2620492250601265 km/sec                
                 Z:     1999.9999885046061081 km              Vz:       -3.2971463586495036 km/sec                
                                                                                                          
        Parameter Set Type:  Keplerian                                                                            
               sma:    13570.4637784821279638 km            RAAN:         320.6468166901589 deg                   
               ecc:        0.4841738562319092                  w:         45.91176678249511 deg                   
               inc:          55.1711431501161 deg             TA:         124.2663057722304 deg                   
                                                                                                          
        Parameter Set Type:  Spherical                                                                            
         Right Asc:         135.0000000474292 deg     Horiz. FPA:         28.81506949411619 deg                   
              Decl:         8.049466931440406 deg        Azimuth:         144.7735093357193 deg                   
               |R|:    14282.8568527058960171 km             |V|:        5.1422348293134652 km/sec         
            
                                                                                                          
        Other Elliptic Orbit Parameters :                                                                         
         Ecc. Anom:         96.22444866533586 deg       Mean Anom:         68.64686988866569 deg                  
         Long Peri:         6.558583512086676 deg        Arg. Lat:         170.1780727678527 deg                  
         True Long:         130.8248892767655 deg        Vert FPA:         61.18493049525984 deg                  
          Ang. Mom:         64351.74030988944 km^2/sec          p:    10389.2169946616813831 km                   
                C3:        -29.37264694057472 km^2/sec^2   Energy:        -14.68632347028736 km^2/sec^2           
           Vel. RA:         176.1923121094458 deg       Vel. Decl:        -39.88056599837589 deg                  
         Rad. Peri:     6999.9999999989913704 km        Vel. Peri:        9.1931057585569604 km/sec               
          Rad. Apo:    20140.9275647811664385 km         Vel. Apo:         3.195073320377567 km/sec               
         Mean Mot.:        0.0228822899628852 deg/sec                                                             
            Period:         15732.69111543975 sec          Period:         262.2115185906624 min                  
            Period:          4.37019197651104 hr           Period:        0.1820913323546267 day                  
                       Time Past Periapsis:           3000.000000000442 sec                                       
                  Time Past Ascending Node:           3655.517694022373 sec                                       
           Beta Angle (Orbit plane to Sun):            14.7917595960703 deg                                       
        Mean Sidereal Greenwich Hour Angle:            292.727580575335 deg                                       
                                                                                                          
        Geodetic Parameters:                                                                                      
          Latitude:         8.073230249747368 deg                                                                 
         Longitude:        -157.7278849665989 deg                                                                 
          Altitude:     7905.1396980148692819 km                                                                  
        Geocentric Parameters:                                                                                    
          Latitude:         8.049412457117054 deg                                                                 
         Longitude:        -157.7278849665989 deg      

            我的结果：
            "Spherical": {
              "RightAscension": 351.1704033965286,
              "Declination": 36.12860358694667,
              "RadiusMagnitude": 7000000.000000001,
              "HorizFPA": -1.2722218725854067E-14,
              "VelocityAzimuth": 44.999999999999964,
              "VelocityMagnitude": 9193.105761451663
            },
             
            "Cartesian": {
              "X": -10000000.009369433,
              "Y": 10000000.00541423,
              "Z": 2000000.0008690897,
              "Vx": -3937.3512884482298,
              "Vy": 262.04923451507887,
              "Vz": -3297.14635306196
                          "Results": {
            "X": -10000000.009369384,
            "Y": 10000000.005414259,
            "Z": 2000000.0008691638,
            "Vx": -3937.351288448252,
            "Vy": 262.0492345151066,
            "Vz": -3297.1463530619376,
            "Duration": 3000,
            "Epoch": "2000-01-01T12:48:55.81600000000617Z",
            "Right_Asc": 135.0000000113306,
            "Decl": 8.049466973108665,
            "RMag": 14282856.867558029,
            "Vert_FPA": 61.1849304283981,
            "Horiz_FPA": 28.815069571601903,
            "VelAzimuth": 144.77350933367802,
            "VelMag": 5142.234829669363,
            "Latitude": 8.073241377671037,
            "Longitude": -157.72699088737426,
            "Height": 7905139.7119802525,
            "Delta_Decl": -19.0777832030284,
            "Delta_RA": 272.14991656597874,
            "Delta_RMag": -388261537.93953675,
            "RadiusOfPeriapsis": 6999999.999999726,
            "RadiusOfApoapsis": 20140927.61397666,
            "SemimajorAxis": 13570463.806988193,
            "Period": 15732.691158215868,
            "Eccentricity": 0.48417385731539747,
            "Inclination": 55.1711431522047,
            "ArgumentOfPeriapsis": 45.91176678683126,
            "RAAN": 320.64681668333515,
            "TrueAnomaly": 124.26630571713669,
            "MeanAnomaly": 68.64686970201274,
            "AltitudeOfPeriapsis": 621862.9999997262,
            "AltitudeOfApoapsis": 13762790.613976661
            */

            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var seg0 = seg.SegmentResults[0];
            //  初始段，
            Assert.AreEqual(351.1704034008877, seg0.InitialState.Spherical.RightAscension, 1e-8);
            Assert.AreEqual(36.12860358283428, seg0.InitialState.Spherical.Declination, 1e-8);       
            Assert.AreEqual(7000.0, seg0.InitialState.Spherical.RadiusMagnitude*0.001,1e-9);
            Assert.AreEqual(0, seg0.InitialState.Spherical.HorizFPA, 1e-13);
            Assert.AreEqual(45.0, seg0.InitialState.Spherical.VelocityAzimuth, 1e-13);
            Assert.AreEqual(9.1931057580955180, seg0.InitialState.Spherical.VelocityMagnitude*0.001, 1e-8);     
            //  最后一段
            var seg3 = seg.SegmentResults[1];
            Assert.AreEqual(-10000.0000062996914494, seg3.FinalState.Cartesian.X*0.001, 1e-5);
            Assert.AreEqual(9999.9999897437755862, seg3.FinalState.Cartesian.Y*0.001, 1e-4);
            Assert.AreEqual(1999.9999885046061081, seg3.FinalState.Cartesian.Z*0.001, 2e-5);
            Assert.AreEqual(-3.9373512839336793, seg3.FinalState.Cartesian.Vx*0.001, 1e-8);
            Assert.AreEqual(0.2620492250601265, seg3.FinalState.Cartesian.Vy*0.001, 1e-8);
            Assert.AreEqual(-3.2971463586495036, seg3.FinalState.Cartesian.Vz*0.001, 1e-8);

            //  段的所有Results
            Assert.AreEqual(-10000000.009369433, (double)seg3.Results["X"], 1e-6);
            Assert.AreEqual(10000000.005414259, (double)seg3.Results["Y"], 1e-6);
            Assert.AreEqual(2000000.0008691638, (double)seg3.Results["Z"], 1e-6);
            Assert.AreEqual(-3937.351288448252, (double)seg3.Results["Vx"], 1e-9);
            Assert.AreEqual(262.0492345151066, (double)seg3.Results["Vy"], 1e-9);
            Assert.AreEqual(-3297.1463530619376, (double)seg3.Results["Vz"], 1e-9);
            Assert.AreEqual(3000, (double)seg3.Results["Duration"], 1e-9);
            Assert.AreEqual("2000-01-01T12:48:55.81600000000617Z", (string)seg3.Results["Epoch"]);
            Assert.AreEqual(135.0000000113306, (double)seg3.Results["Right_Asc"], 1e-9);
            Assert.AreEqual(8.049466973108665, (double)seg3.Results["Decl"], 1e-9);
            Assert.AreEqual(14282856.867558029, (double)seg3.Results["RMag"], 1e-8);
            Assert.AreEqual(61.1849304283981, (double)seg3.Results["Vert_FPA"], 1e-9);
            Assert.AreEqual(28.815069571601903, (double)seg3.Results["Horiz_FPA"], 1e-9);
            Assert.AreEqual(144.77350933367802, (double)seg3.Results["VelAzimuth"], 1e-9);
            Assert.AreEqual(5142.234829669363, (double)seg3.Results["VelMag"], 1e-9);
            Assert.AreEqual(8.073241377670739, (double)seg3.Results["Latitude"], 0.0001);   //  单独运行精度很高
            Assert.AreEqual(-157.72699088737426, (double)seg3.Results["Longitude"], 1e-9);
            Assert.AreEqual(7905139.7119802525, (double)seg3.Results["Height"], 1e-6);
            Assert.AreEqual(19.0777832030284, (double)seg3.Results["Delta_Decl"], 1e-9);
            Assert.AreEqual(-87.85008343402127, (double)seg3.Results["Delta_RA"], 1e-9);
            Assert.AreEqual(-388261537.93953675, (double)seg3.Results["Delta_RMag"], 1e-6);            
            Assert.AreEqual(6999999.999999726, (double)seg3.Results["RadiusOfPeriapsis"], 1e-6);
            Assert.AreEqual(20140927.61397666, (double)seg3.Results["RadiusOfApoapsis"], 1e-6);
            Assert.AreEqual(13570463.806988193, (double)seg3.Results["SemimajorAxis"], 1e-6);
            Assert.AreEqual(15732.691158215868, (double)seg3.Results["Period"], 1e-6);
            Assert.AreEqual(0.48417385731539747, (double)seg3.Results["Eccentricity"], 1e-9);
            Assert.AreEqual(55.1711431522047, (double)seg3.Results["Inclination"], 1e-9);
            Assert.AreEqual(45.91176678683126, (double)seg3.Results["ArgumentOfPeriapsis"], 1e-9);
            Assert.AreEqual(320.64681668333515, (double)seg3.Results["RAAN"], 1e-9);
            Assert.AreEqual(124.26630571713669, (double)seg3.Results["TrueAnomaly"], 1e-9);
            Assert.AreEqual(68.64686970201274, (double)seg3.Results["MeanAnomaly"], 1e-9);
            Assert.AreEqual(621862.9999997262, (double)seg3.Results["AltitudeOfPeriapsis"], 1e-6);
            Assert.AreEqual(13762790.613976661, (double)seg3.Results["AltitudeOfApoapsis"], 1e-6);            
        }
    }
}
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
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.Spherical_XYZ",
          "Description": null,
          "UserComment": null,
          "Converged": true,
          "TotalIterations": 15,
          "FinalRmsError": 0.006267774955560714,
          "FinalVariables": [
            {
              "Name": "InitialState.Spherical.Right_Asc",
              "Value": -8.829596603471453,
              "Unit": ""
            },
            {
              "Name": "InitialState.Spherical.Decl",
              "Value": 36.12860358694648,
              "Unit": ""
            },
            {
              "Name": "InitialState.Spherical.VMag",
              "Value": 1193.1057614516626,
              "Unit": ""
            }
          ],
          "FinalConstraints": [
            {
              "Name": "X",
              "DesiredValue": -10000000,
              "AchievedValue": -10000000.009369384,
              "Error": 0.009369384497404099,
              "Unit": ""
            },
            {
              "Name": "Y",
              "DesiredValue": 10000000,
              "AchievedValue": 10000000.005414259,
              "Error": 0.005414258688688278,
              "Unit": ""
            },
            {
              "Name": "Z",
              "DesiredValue": 2000000,
              "AchievedValue": 2000000.0008691638,
              "Error": 0.0008691637776792049,
              "Unit": ""
            }
          ],
          "IterationHistory": [
            {
              "StepNumber": 0,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": 0,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 0,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 0,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -8228745.590454027,
                  "Error": 1771254.409545973,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 2374079.043068731,
                  "Error": 7625920.956931269,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2374079.0430687293,
                  "Error": 374079.04306872934,
                  "Unit": ""
                }
              ],
              "RmsError": 4525186.8379535
            },
            {
              "StepNumber": 1,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -2.4483062019093818,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 4.47225295439123,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 100,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -8414230.58383294,
                  "Error": 1585769.41616706,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 3324062.6432584813,
                  "Error": 6675937.356741519,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2302014.2731127427,
                  "Error": 302014.2731127427,
                  "Unit": ""
                }
              ],
              "RmsError": 3965434.690486932
            },
            {
              "StepNumber": 2,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -3.9077722968440427,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 8.534983488271468,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 200,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -8587450.298462655,
                  "Error": 1412549.7015373446,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 4126572.8509335234,
                  "Error": 5873427.149066476,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2243316.0307597006,
                  "Error": 243316.03075970057,
                  "Unit": ""
                }
              ],
              "RmsError": 3490541.7834260366
            },
            {
              "StepNumber": 3,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -4.902536666039642,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 12.269172456049782,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 300,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -8751943.873928642,
                  "Error": 1248056.126071358,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 4850075.906893397,
                  "Error": 5149924.093106603,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2193371.972571308,
                  "Error": 193371.9725713078,
                  "Unit": ""
                }
              ],
              "RmsError": 3061413.3434499516
            },
            {
              "StepNumber": 4,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -5.640522681170655,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 15.725694191711359,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 400,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -8909631.403541308,
                  "Error": 1090368.596458692,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 5522268.236099065,
                  "Error": 4477731.763900935,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2150097.6261780793,
                  "Error": 150097.62617807928,
                  "Unit": ""
                }
              ],
              "RmsError": 2662174.2193787745
            },
            {
              "StepNumber": 5,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -6.225022813841584,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 18.94077224784909,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 500,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9061672.724586312,
                  "Error": 938327.2754136883,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 6157581.542220098,
                  "Error": 3842418.457779902,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2112343.21700998,
                  "Error": 112343.21700998023,
                  "Unit": ""
                }
              ],
              "RmsError": 2284531.9490639004
            },
            {
              "StepNumber": 6,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -6.713363307067742,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 21.941638454041815,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 600,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9208833.328339292,
                  "Error": 791166.6716607083,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 6764625.125202789,
                  "Error": 3235374.874797211,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2079443.489079596,
                  "Error": 79443.48907959601,
                  "Unit": ""
                }
              ],
              "RmsError": 1923530.309160121
            },
            {
              "StepNumber": 7,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -7.139483780623151,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 24.749368865891515,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 700,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9351651.232307043,
                  "Error": 648348.7676929571,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 7349002.565489499,
                  "Error": 2650997.434510501,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2051048.7457443275,
                  "Error": 51048.74574432755,
                  "Unit": ""
                }
              ],
              "RmsError": 1575938.8203853075
            },
            {
              "StepNumber": 8,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -7.524306226388408,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 27.380455488350876,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 800,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9490517.881458927,
                  "Error": 509482.11854107305,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 7914592.699020853,
                  "Error": 2085407.3009791467,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2027068.1009882917,
                  "Error": 27068.10098829167,
                  "Unit": ""
                }
              ],
              "RmsError": 1239519.8428129798
            },
            {
              "StepNumber": 9,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -7.881054199556788,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 29.84757444601231,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 900,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9625714.78697163,
                  "Error": 374285.2130283695,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 8464204.391521327,
                  "Error": 1535795.6084786728,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2007692.5870549087,
                  "Error": 7692.587054908741,
                  "Unit": ""
                }
              ],
              "RmsError": 912654.872265463
            },
            {
              "StepNumber": 10,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -8.218183456799133,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 32.159489666297816,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 1000,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9757416.204399113,
                  "Error": 242583.7956008874,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 8999935.534212042,
                  "Error": 1000064.4657879584,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 1993543.3094485323,
                  "Error": 6456.6905514677055,
                  "Unit": ""
                }
              ],
              "RmsError": 594142.946456705
            },
            {
              "StepNumber": 11,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -8.54103259930083,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 34.31891991314312,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 1100,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9885629.703580923,
                  "Error": 114370.29641907662,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 9523363.14112793,
                  "Error": 476636.8588720709,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 1986205.5999815403,
                  "Error": 13794.400018459652,
                  "Unit": ""
                }
              ],
              "RmsError": 283109.8405273656
            },
            {
              "StepNumber": 12,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -8.828527260997467,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 36.15596084244845,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 1192.3262257447884,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -10000270.446684575,
                  "Error": 270.44668457470834,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 9996500.458804261,
                  "Error": 3499.5411957390606,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 1991486.1467300463,
                  "Error": 8513.853269953746,
                  "Unit": ""
                }
              ],
              "RmsError": 5316.8169516536345
            },
            {
              "StepNumber": 13,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -8.829626827353936,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 36.12857624252832,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 1193.1042371824426,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -9999992.115793897,
                  "Error": 7.884206103160977,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 9999998.370943084,
                  "Error": 1.629056915640831,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 1999999.4600048568,
                  "Error": 0.5399951431900263,
                  "Unit": ""
                }
              ],
              "RmsError": 4.658545089982023
            },
            {
              "StepNumber": 14,
              "Variables": [
                {
                  "Name": "InitialState.Spherical.Right_Asc",
                  "Value": -8.829596603471453,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.Decl",
                  "Value": 36.12860358694648,
                  "Unit": ""
                },
                {
                  "Name": "InitialState.Spherical.VMag",
                  "Value": 1193.1057614516626,
                  "Unit": ""
                }
              ],
              "Constraints": [
                {
                  "Name": "X",
                  "DesiredValue": -10000000,
                  "AchievedValue": -10000000.009369384,
                  "Error": 0.009369384497404099,
                  "Unit": ""
                },
                {
                  "Name": "Y",
                  "DesiredValue": 10000000,
                  "AchievedValue": 10000000.005414259,
                  "Error": 0.005414258688688278,
                  "Unit": ""
                },
                {
                  "Name": "Z",
                  "DesiredValue": 2000000,
                  "AchievedValue": 2000000.0008691638,
                  "Error": 0.0008691637776792049,
                  "Unit": ""
                }
              ],
              "RmsError": 0.006267774955560714
            }
          ]
        }
      ],
      "SegmentResults": [
        {
          "$type": "SegmentResult",
          "TypeName": "InitialState",
          "Name": "InitialState",
          "Description": "初始段参数",
          "UserComment": "初始段参数",
          "InitialState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 5586866.740895344,
              "Y": -867848.0086733671,
              "Z": 4127197.578662682,
              "Vx": -2789.472699878682,
              "Vy": 7011.776135500682,
              "Vz": 5250.4315083134325
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 13570463.806987086,
              "Eccentricity": 0.4841738573153351,
              "Inclination": 55.171143152204685,
              "RAAN": 320.64681668333515,
              "ArgOfPeriapsis": 45.911766786841376,
              "MeanAnomaly": 9.671982480218955E-16,
              "TrueAnomaly": 3.1805546814635168E-15,
              "Period": 15732.691158213944
            },
            "Spherical": {
              "RightAscension": 351.1704033965285,
              "Declination": 36.12860358694649,
              "RadiusMagnitude": 6999999.999999999,
              "HorizFPA": 0,
              "VelocityAzimuth": 44.999999999999986,
              "VelocityMagnitude": 9193.105761451665
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20
          },
          "FinalState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 5586866.740895344,
              "Y": -867848.0086733671,
              "Z": 4127197.578662682,
              "Vx": -2789.472699878682,
              "Vy": 7011.776135500682,
              "Vz": 5250.4315083134325
            },
            "Keplerian": null,
            "Spherical": null,
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20
          },
          "DurationSec": 0,
          "Results": {
            "RadiusOfApoapsis": 20140927.613974173
          }
        },
        {
          "$type": "SegmentResult",
          "TypeName": "Propagate",
          "Name": "Propagate",
          "Description": "轨道递推段",
          "UserComment": "轨道递推段",
          "InitialState": {
            "Epoch": "2000-01-01T11:58:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": 5586866.740895344,
              "Y": -867848.0086733671,
              "Z": 4127197.578662682,
              "Vx": -2789.472699878682,
              "Vy": 7011.776135500682,
              "Vz": 5250.4315083134325
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 13570463.806987086,
              "Eccentricity": 0.4841738573153351,
              "Inclination": 55.171143152204685,
              "RAAN": 320.64681668333515,
              "ArgOfPeriapsis": 45.911766786841376,
              "MeanAnomaly": 9.671982480218955E-16,
              "TrueAnomaly": 3.1805546814635168E-15,
              "Period": 15732.691158213944
            },
            "Spherical": {
              "RightAscension": 351.1704033965285,
              "Declination": 36.12860358694649,
              "RadiusMagnitude": 6999999.999999999,
              "HorizFPA": 0,
              "VelocityAzimuth": 44.999999999999986,
              "VelocityMagnitude": 9193.105761451665
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20
          },
          "FinalState": {
            "Epoch": "2000-01-01T12:48:55.81600000000617Z",
            "CoordSystemName": "Earth Inertial",
            "Cartesian": {
              "X": -10000000.009369384,
              "Y": 10000000.005414259,
              "Z": 2000000.0008691638,
              "Vx": -3937.351288448252,
              "Vy": 262.0492345151066,
              "Vz": -3297.1463530619376
            },
            "Keplerian": {
              "ElementType": "Osculating",
              "GravitationalParameter": 398600441500000,
              "SemiMajorAxis": 13570463.806988193,
              "Eccentricity": 0.48417385731539747,
              "Inclination": 55.1711431522047,
              "RAAN": 320.64681668333515,
              "ArgOfPeriapsis": 45.91176678683126,
              "MeanAnomaly": 68.64686970201274,
              "TrueAnomaly": 124.26630571713669,
              "Period": 15732.691158215868
            },
            "Spherical": {
              "RightAscension": 135.0000000113306,
              "Declination": 8.049466973108661,
              "RadiusMagnitude": 14282856.867558029,
              "HorizFPA": 28.815069571608174,
              "VelocityAzimuth": 144.77350933367802,
              "VelocityMagnitude": 5142.234829669363
            },
            "DryMass": 100,
            "FuelMass": 900,
            "Cd": 2.2,
            "Cr": 2,
            "DragArea": 20,
            "SRPArea": 20
          },
          "DurationSec": 3000,
          "Results": {
            "X": -10000000.009369384,
            "Y": 10000000.005414259,
            "Z": 2000000.0008691638,
            "Vx": -3937.351288448252,
            "Vy": 262.0492345151066,
            "Vz": -3297.1463530619376,
            "Duration": 3000,
            "Epoch": "2000-01-01T12:48:55.81600000000617Z",
            "Right_Asc": 135.0000000113306,
            "Decl": 8.049466973108665,
            "RMag": 14282856.867558029,
            "Vert_FPA": 61.1849304283981,
            "Horiz_FPA": 28.815069571601903,
            "VelAzimuth": 144.77350933367802,
            "VelMag": 5142.234829669363,
            "Latitude": 8.073241377671037,
            "Longitude": -157.72699088737426,
            "Height": 7905139.7119802525,
            "Delta_Decl": -19.0777832030284,
            "Delta_RA": -87.85008343402127,
            "Delta_RMag": -388261537.93953675,
            "RadiusOfPeriapsis": 6999999.999999726,
            "RadiusOfApoapsis": 20140927.61397666,
            "SemimajorAxis": 13570463.806988193,
            "Period": 15732.691158215868,
            "Eccentricity": 0.48417385731539747,
            "Inclination": 55.1711431522047,
            "ArgumentOfPeriapsis": 45.91176678683126,
            "RAAN": 320.64681668333515,
            "TrueAnomaly": 124.26630571713669,
            "MeanAnomaly": 68.64686970201274,
            "AltitudeOfPeriapsis": 621862.9999997262,
            "AltitudeOfApoapsis": 13762790.613976661
          }
        }
      ],
      "TypeName": "TargetSequence",
      "Name": "Inner_Target_List",
      "Description": "目标轨道段",
      "UserComment": "目标轨道段",
      "InitialState": null,
      "FinalState": null,
      "DurationSec": 0,
      "Results": {}
    }
  ],
  "Position": null,
  "FinalFuelMass": 0
}

*/
