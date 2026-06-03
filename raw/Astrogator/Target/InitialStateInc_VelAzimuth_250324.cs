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
            自变量: InitialState.Keplerian.Inc
            约束: VelAzimuth
        
        # 飞行任务各段简要说明

        ## 主序列飞行段

        ## 目标序列段
        
        1. **InitialState（初始状态）**
          - 初始化航天器在地心惯性坐标系中的位置和速度
          - 初始位置：(8000000, 0, 0) 米
          - 初始速度：(1500, 7500, 0) 米/秒
          - 设置航天器物理参数：干质量100kg，燃料900kg
          - 结果计算：远地点距离（RadiusOfApoapsis）

        2. **Propagate**
          - 使用地球点质量模型传播
          - 停止条件：Duration

          Resuts:            
            - VelocityAzimuth

        ## 目标求解配置

        **DC: Epoch_Altitude（霍曼转移微分校正器）**
          输出优化过程
          - 控制变量：
            - InitialState.Keplerian.Inc
          - 约束条件：
            - VelAzimuth

        结果:
            与STK对比，轨道倾角相差： 为1e-5°

        TBD:  之前可以，现在不行了，待查原因
        */
        [TestMethod()]
        public void InitialStateInc_VelAzimuth_250324()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "InitialStateInc_VelAzimuth_250324.json");

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


            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var seg0 = seg.SegmentResults[0];
            //  初始段
            Assert.AreEqual(9686497.505536763, seg0.InitialState.Keplerian.SemiMajorAxis, 1e-6);
            Assert.AreEqual(0.2600177958875259, seg0.InitialState.Keplerian.Eccentricity, 1e-9);            
            Assert.AreEqual(0, seg0.InitialState.Keplerian.RAAN, 1e-9);
            Assert.AreEqual(299.730955671197, seg0.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);
            Assert.AreEqual(60.26904432880302, seg0.InitialState.Keplerian.TrueAnomaly, 1e-9);
            // 与STK对比
            Assert.AreEqual(36.523792891467730, seg0.InitialState.Keplerian.Inclination, 1e-5);
            /*
              STK 结果                                                                                                                      
                                                                                               
            UTC Gregorian Date: 1 Jan 2000 11:58:55.816  UTC Julian Date: 2451544.99925713                 
            Julian Ephemeris Date: 2451545                                                                 
            Time past epoch: -7.0897e+08 sec   (Epoch in UTC Gregorian Date: 20 Jun 2022 04:00:00.000)     
                                                                                               
            State Vector in Coordinate System: Earth Inertial                                              
                                                                                               
            Parameter Set Type:  Cartesian                                                                 
                     X:     7999.9999999999981810 km              Vx:        1.5000000000000087 km/sec     
                     Y:       -0.0000000000083819 km              Vy:        6.0270733698674661 km/sec     
                     Z:       -0.0000000000067521 km              Vz:        4.4636741138029299 km/sec     
                                                                                               
            Parameter Set Type:  Keplerian                                                                 
                   sma:     9686.4975055367522145 km            RAAN:     5.264770332083878e-15 deg        
                   ecc:        0.2600177958875253                  w:         299.7309556711968 deg        
                   inc:         36.52379289146773 deg             TA:         60.26904432880313 deg        
                                                                                               
            Parameter Set Type:  Spherical                                                                 
             Right Asc:                         0 deg     Horiz. FPA:         11.30993247402021 deg        
                  Decl:                         0 deg        Azimuth:         53.47620710853227 deg        
            
            */
            //  最后一段
            var seg3 = seg.SegmentResults[1];

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
          "Converged": true,
          "TotalIterations": 15,
          "ControlParameters": [
            {
              "Enable": true,
              "Name": "InitialState.Spherical.Right_Asc",
              "InitialValue": "0.0",
              "FinalValue": "-8.829596603471453",
              "Correction": -8.829596603471453,
              "LastUpdate": 3.0223882482971476E-05,
              "Dimension": "",
              "MaxStep": 10,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                0,
                -2.4483062019093818,
                -3.9077722968440427,
                -4.902536666039642,
                -5.640522681170655,
                -6.225022813841584,
                -6.713363307067742,
                -7.139483780623151,
                -7.524306226388408,
                -7.881054199556788,
                -8.218183456799133,
                -8.54103259930083,
                -8.828527260997467,
                -8.829626827353936,
                -8.829596603471453
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Spherical.Decl",
              "InitialValue": "0.0",
              "FinalValue": "36.12860358694648",
              "Correction": 36.12860358694648,
              "LastUpdate": 2.7344418164432227E-05,
              "Dimension": "",
              "MaxStep": 10,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                0,
                4.47225295439123,
                8.534983488271468,
                12.269172456049782,
                15.725694191711359,
                18.94077224784909,
                21.941638454041815,
                24.749368865891515,
                27.380455488350876,
                29.84757444601231,
                32.159489666297816,
                34.31891991314312,
                36.15596084244845,
                36.12857624252832,
                36.12860358694648
              ]
            },
            {
              "Enable": true,
              "Name": "InitialState.Spherical.VMag",
              "InitialValue": "8000.0",
              "FinalValue": "9193.105761451663",
              "Correction": 1193.1057614516626,
              "LastUpdate": 0.0015242692199990415,
              "Dimension": "",
              "MaxStep": 100,
              "ParentName": "InitialState",
              "Perturbation": 0.1,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.0001,
              "Unit": null,
              "Values": [
                8000,
                8100,
                8200,
                8300,
                8400,
                8500,
                8600,
                8700,
                8800,
                8900,
                9000,
                9100,
                9192.326225744788,
                9193.104237182442,
                9193.105761451663
              ]
            }
          ],
          "Results": [
            {
              "Enable": true,
              "Name": "X",
              "DesiredValue": "-10000000.0",
              "ParentName": "Propagate",
              "CurrentValue": "-10000000.009369384",
              "Unit": null,
              "Difference": -0.009369384497404099,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                -8228745.590454027,
                -8414230.58383294,
                -8587450.298462655,
                -8751943.873928642,
                -8909631.403541308,
                -9061672.724586312,
                -9208833.328339292,
                -9351651.232307043,
                -9490517.881458927,
                -9625714.78697163,
                -9757416.204399113,
                -9885629.703580923,
                -10000270.446684575,
                -9999992.115793897,
                -10000000.009369384
              ]
            },
            {
              "Enable": true,
              "Name": "Y",
              "DesiredValue": "10000000.0",
              "ParentName": "Propagate",
              "CurrentValue": "10000000.005414259",
              "Unit": null,
              "Difference": 0.005414258688688278,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                2374079.043068731,
                3324062.6432584813,
                4126572.8509335234,
                4850075.906893397,
                5522268.236099065,
                6157581.542220098,
                6764625.125202789,
                7349002.565489499,
                7914592.699020853,
                8464204.391521327,
                8999935.534212042,
                9523363.14112793,
                9996500.458804261,
                9999998.370943084,
                10000000.005414259
              ]
            },
            {
              "Enable": true,
              "Name": "Z",
              "DesiredValue": "2000000.0",
              "ParentName": "Propagate",
              "CurrentValue": "2000000.0008691638",
              "Unit": null,
              "Difference": 0.0008691637776792049,
              "ScalingMethod": "NoScaling",
              "ScalingValue": 1,
              "Tolerance": 0.1,
              "Weight": 1,
              "Values": [
                2374079.0430687293,
                2302014.2731127427,
                2243316.0307597006,
                2193371.972571308,
                2150097.6261780793,
                2112343.21700998,
                2079443.489079596,
                2051048.7457443275,
                2027068.1009882917,
                2007692.5870549087,
                1993543.3094485323,
                1986205.5999815403,
                1991486.1467300463,
                1999999.4600048568,
                2000000.0008691638
              ]
            }
          ],
          "TypeName": "DifferentialCorrector",
          "Name": "DC: InitialState.Spherical_XYZ",
          "Description": null,
          "UserComment": null
        }
      ],
      "SegmentResults": [
        {
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
            "SRPArea": 20,
            "Geodetic_Latitude": 36.294256329151224,
            "Geodetic_Longitude": 70.97928031454174,
            "Geodetic_Altitude": 629317.9712046866,
            "Geocentric_Latitude": 36.127324241348745,
            "Geocentric_Longitude": 70.97928031454174
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
            "SRPArea": 20,
            "Geodetic_Latitude": 36.294256329151224,
            "Geodetic_Longitude": 70.97928031454174,
            "Geodetic_Altitude": 629317.9712046866,
            "Geocentric_Latitude": 36.127324241348745,
            "Geocentric_Longitude": 70.97928031454174
          },
          "DurationSec": 0,
          "Results": {
            "RadiusOfApoapsis": 20140927.613974173
          }
        },
        {
          "$type": "PropagateResult",
          "StoppedOnMaximumDuration": false,
          "StoppingConditionName": "Duration",
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
            "SRPArea": 20,
            "Geodetic_Latitude": 36.294256329151224,
            "Geodetic_Longitude": 70.97928031454174,
            "Geodetic_Altitude": 629317.9712046866,
            "Geocentric_Latitude": 36.127324241348745,
            "Geocentric_Longitude": 70.97928031454174
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
            "SRPArea": 20,
            "Geodetic_Latitude": 8.073241377671037,
            "Geodetic_Longitude": -157.72699088737426,
            "Geodetic_Altitude": 7905139.7119802525,
            "Geocentric_Latitude": 8.049423553107445,
            "Geocentric_Longitude": -157.72699088737426
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
            "Delta_Decl": 19.0777832030284,
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
        "SRPArea": 20,
        "Geodetic_Latitude": 36.294256329151224,
        "Geodetic_Longitude": 70.97928031454174,
        "Geodetic_Altitude": 629317.9712046866,
        "Geocentric_Latitude": 36.127324241348745,
        "Geocentric_Longitude": 70.97928031454174
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
        "SRPArea": 20,
        "Geodetic_Latitude": 8.073241377671037,
        "Geodetic_Longitude": -157.72699088737426,
        "Geodetic_Altitude": 7905139.7119802525,
        "Geocentric_Latitude": 8.049423553107445,
        "Geocentric_Longitude": -157.72699088737426
      },
      "DurationSec": 3000,
      "Results": {}
    }
  ],
  "Positions": {
    "CentralBody": "Earth",
    "CzmlPositions": [
      {
        "CentralBody": "Earth",
        "interpolationAlgorithm": "LAGRANGE",
        "interpolationDegree": 7,
        "referenceFrame": "INERTIAL",
        "epoch": "2000-01-01T11:58:55.816Z",
        "interval": "2000-01-01T11:58:55.816Z/2000-01-01T12:48:55.816Z",
        "cartesian": null,
        "cartesianVelocity": [
          0,
          5586866.740895344,
          -867848.0086733671,
          4127197.578662682,
          -2789.472699878682,
          7011.776135500682,
          5250.4315083134325,
          60,
          5407938.426441389,
          -445620.6566229473,
          4433378.239382485,
          -3172.533784489593,
          7057.544693886577,
          4952.187282130582,
          123.74099999999453,
          5193187.122066476,
          4928.515579179337,
          4738386.930872329,
          -3562.444181583813,
          7073.7799607589195,
          4614.878421772486,
          160.20799999999872,
          5059357.928737431,
          262818.9788076027,
          4903030.613774657,
          -3776.062547865787,
          7068.209080904936,
          4414.001971133389,
          197.22000000000116,
          4915706.851865214,
          524155.93021642044,
          5062553.256891129,
          -3984.960955733978,
          7051.738577212381,
          4205.313640489241,
          258.2780000000057,
          4662327.836757992,
          953332.0664803884,
          5308604.523265632,
          -4310.562006787244,
          7001.571900458814,
          3852.81726290474,
          324.7890000000043,
          4364627.533518329,
          1416345.3774590897,
          5551868.329082561,
          -4636.1067342639235,
          6916.212662797105,
          3461.2602971521255,
          392.9799999999959,
          4038052.1052592397,
          1884116.6867292193,
          5774104.13392417,
          -4936.3808496183065,
          6798.402376141506,
          3056.6690943262815,
          462.73099999999977,
          3684070.1639724136,
          2353285.2297893018,
          5972915.33582944,
          -5207.361607655412,
          6649.876986683291,
          2644.639136229215,
          534.3279999999941,
          3302417.0992279695,
          2823176.4430968985,
          6147311.088085929,
          -5447.394881536897,
          6472.248359357526,
          2228.469319365031,
          607.4529999999941,
          2896301.4429893894,
          3289153.889316174,
          6295062.754594808,
          -5653.6074568481745,
          6269.180180393773,
          1814.8499093038442,
          681.0829999999987,
          2473553.0484763067,
          3742678.7705145422,
          6413816.465562954,
          -5823.2783619634165,
          6047.31734671757,
          1413.6456472873515,
          754.8889999999956,
          2038592.2883307592,
          4180385.9656606955,
          6503860.92034295,
          -5957.5779606408005,
          5811.875837077374,
          1029.5867769816768,
          828.6889999999985,
          1594990.1297454145,
          4600326.356262537,
          6566284.39779459,
          -6058.923727646174,
          5567.428837152943,
          665.5525835318231,
          902.3899999999994,
          1145636.611417234,
          5001477.312300642,
          6602584.270212635,
          -6130.33358270013,
          5317.826060000014,
          323.07108017464435,
          975.9210000000021,
          693071.3237127147,
          5383262.6833542595,
          6614434.725728494,
          -6175.009718275318,
          5066.320091910407,
          2.83871153203188,
          1049.1579999999958,
          239925.025308187,
          5745125.0533326715,
          6603613.86119387,
          -6196.137810451688,
          4815.812723087476,
          -294.82288912965964,
          1121.8450000000012,
          -210593.8800283611,
          6086190.140530478,
          6572074.066657396,
          -6196.857515069087,
          4569.138249231189,
          -569.6251584217084,
          1199.1000000000058,
          -688737.8584211741,
          6429170.105736243,
          6517485.774074217,
          -6178.4088338804995,
          4310.814022062277,
          -839.9051301256657,
          1280.796000000002,
          -1192056.976512033,
          6770381.2847409425,
          6437987.866602397,
          -6140.435843632695,
          4043.490749739777,
          -1102.3925120741349,
          1367.1159999999945,
          -1719735.069856077,
          7107501.818820454,
          6331743.729471264,
          -6082.941971981671,
          3768.894805400084,
          -1355.171790613579,
          1458.3190000000031,
          -2271129.389073007,
          7438374.561158426,
          6196941.758504834,
          -6006.162281171988,
          3488.6009587938124,
          -1596.7070439606823,
          1554.7419999999984,
          -2845755.5755776805,
          7760936.641605362,
          6031730.773234274,
          -5910.4820956089825,
          3204.019552986107,
          -1825.7820333149014,
          1656.7799999999988,
          -3443127.570892523,
          8073077.686532884,
          5834208.348528144,
          -5796.397316195402,
          2916.460418821805,
          -2041.393002024321,
          1764.8849999999948,
          -4062705.1389566883,
          8372581.627563163,
          5602403.969159063,
          -5664.47949853332,
          2627.1403057508414,
          -2242.7085905542654,
          1879.5570000000007,
          -4703788.78155689,
          8657053.201251049,
          5334293.114646279,
          -5515.361654031881,
          2337.213513499027,
          -2429.0228529696524,
          2001.3420000000042,
          -5365452.955386839,
          8923877.50284883,
          5027813.172464267,
          -5349.724836746184,
          2047.7802433894722,
          -2599.733751196956,
          2130.8329999999987,
          -6046492.063881258,
          9170192.372606868,
          4680881.970400367,
          -5168.284432229131,
          1759.886858243182,
          -2754.33037026395,
          2268.6619999999966,
          -6745316.847147806,
          9392850.542929456,
          4291450.135078542,
          -4971.791087400469,
          1474.544529458678,
          -2892.3730419680596,
          2415.5,
          -7459895.706751417,
          9588410.173483003,
          3857544.04774254,
          -4761.021403961368,
          1192.7251877887334,
          -3013.4893633243423,
          2571.9850000000006,
          -8187368.231229037,
          9753064.71037364,
          3377540.1127718217,
          -4536.870273588369,
          915.479625564454,
          -3117.3270928492525,
          2733.9320000000007,
          -8903454.798055079,
          9879639.753993997,
          2865592.619415039,
          -4307.08191439161,
          651.3648104559046,
          -3201.4341222870326,
          2900.9890000000014,
          -9603425.807152502,
          9967237.650045523,
          2325014.353240122,
          -4073.633185336099,
          400.8829324526535,
          -3267.053981165867,
          3000,
          -10000000.009369384,
          10000000.005414259,
          2000000.0008691638,
          -3937.351288448252,
          262.0492345151066,
          -3297.1463530619376
        ]
      }
    ]
  }
}
         */
    }
}