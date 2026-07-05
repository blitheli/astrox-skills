using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests
{
    public partial class TargetTests
    {
        /*
         测试 Astrogator 地月转移 (微分修正，将末端状态修正到月球附近）
            
            微分修正：   2次微分修正，第2次微分修正在第1次的基础上进行！
                -   B平面 
                -   轨道倾角，高度  

           MCS:
            Target  地月转移   
                >   InitialState       地球惯性系Cartisian
                >   递推32万km         CisLunar积分器(RMagnitude=32万km)
                >   近月点             CisLunar积分器(3种停止条件)

            Target  近月制动
                >   DV1                 近月制动

            Propagate   递推1天        moon hpop


            @InitialState参数：
                坐标系: 地球惯性系
                中心天体引力常数: 3.986004415E14,
                轨道历元: 2022-06-20T04:00:00.000Z
                坐标类型: Spherical,    初始值参考了E2M_DeltaDecRA_250328的计算结果
            
                结构质量: 100 (kg)
                燃料质量: 900 (kg)
                拖拽系数: 2.2
                拖拽面积: 20 (m^2)
                SRP系数: 2.0
                SRP面积: 20 (m^2)
 
            @Prop32wkm参数：
                积分器名称: CisLunar
                停止条件: 
                    -   R Magnitude        320000000 (m)
                        
            @Prop2Moon参数
                积分器名称: CisLunar
                停止条件:
                    -   Duration            345600 (s)
                    -   Altitude(Moon)      0 (m)
                    -   Periapsis(Moon)    

            Target  地月转移    (2个微分修正，仅最后一个有效，且已为优化值，本例中不优化
                       
                微分修正1   B平面：
                    自变量：
                        -   InitialState.Spherical.Right_Asc
                        -   InitialState.Spherical.Decl
                        -   InitialState.Spherical.VMag   
                    约束：
                        -   Prop2Moon.BDotR (5500 km)
                        -   Prop2Moon.BDotT (0 km)
                        -   Prop2Moon.Epoch (116 hour)
        
                微分修正1   轨道倾角，高度：
                    自变量：
                        -   InitialState.Spherical.Right_Asc
                        -   InitialState.Spherical.Decl
                        -   InitialState.Spherical.VMag   
                    约束：
                        -   Prop2Moon.Altitude (100 km)
                        -   Prop2Moon.Inclination (90 deg)
                        -   Prop2Moon.Epoch (116 hour)
            Target  近月制动
                微分修正1 
                    自变量:
                        -   ImpulsiveMnvr.Cartesian.X
                    约束:
                        -   DV1.偏心率（0.001）

        与STK对比
            STK 微分修正的结果，自变量，赤经赤纬 角度相差约0.0001°

            但是本程序的微分修正结果是自洽的，约束满足

        20250616    初次创建
        20260201    增加 对获取段的测试
        20260611    修复最后一段(环月飞行)的坐标系问题，现在段初始状态正确了.
        */
        [TestMethod()]
        public void E2M_MoonBrake_250616()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "E2M_MoonBrake_250616.json");

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
             *  STK 初始状态：（2次微分修正优化后）
Parameter Set Type:  Cartesian                                                                 
         X:    -4349.1216989589929653 km              Vx:        6.0415149059974862 km/sec     
         Y:    -4722.2825443344709129 km              Vy:       -7.5004698102795029 km/sec     
         Z:    -1839.2122797238630483 km              Vz:        4.9717230511889916 km/sec     
                                                                                               
Parameter Set Type:  Keplerian                                                                 
       sma:   209815.9435125593736302 km            RAAN:         254.2514624869092 deg        
       ecc:        0.9681714511862143                  w:         329.0193009289385 deg        
       inc:         32.34616635346974 deg             TA:                         0 deg        
                                                                                               
Parameter Set Type:  Spherical                                                                 
 Right Asc:         227.3555883261171 deg     Horiz. FPA:    -2.162777183395191e-13 deg        
      Decl:        -15.98632909654429 deg        Azimuth:                      61.5 deg        
       |R|:     6678.1370000000033542 km             |V|:       10.8385875386061290 km/sec    
             */
            //=================================================================
            //  第1个Target段
            var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;
            var seg0 = seg.SegmentResults[0];
            //  初始段
            Assert.AreEqual(227.3555883261171, seg0.InitialState.Spherical.RightAscension, 0.0001);
            Assert.AreEqual(-15.98632909654429, seg0.InitialState.Spherical.Declination, 0.0001);
            Assert.AreEqual(10838.587538606129, seg0.InitialState.Spherical.VelocityMagnitude, 0.1);

            // Target 收敛     
            Assert.IsTrue((seg.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults).Converged);
            Assert.IsTrue((seg.OperatorResults[1] as TargetOperatorDifferentialCorrectorResults).Converged);
            // 近月点 
            var seg3 = seg.SegmentResults[2];
            Assert.AreEqual(100, (double)seg3.Results["高度"] * 0.001, 0.2);
            Assert.AreEqual(90, (double)seg3.Results["轨道倾角"], 0.01);

            //=================================================================
            //  近月制动
            var segBrake = output.MainSequenceResults[1] as MCSTargetSequenceResults;
            var segBrake0 = segBrake.SegmentResults[0];

            Assert.AreEqual(0, (double)segBrake0.Results["偏心率"], 0.001);

            // Target 收敛
            Assert.IsTrue((segBrake.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults).Converged);

            //  默认地心惯性系下的位置速度
            var rv1 = segBrake0.FinalState.Cartesian;
            //=================================================================
            //  环月轨道递推
            var segMoon = output.MainSequenceResults[2] as MCSPropagateResults;

            var rv2 = segMoon.InitialState.Cartesian;

            //  这段的初始RV应该和近月制动的末端RV一致
            Assert.AreEqual(rv1.X, rv2.X, 0.001);
            Assert.AreEqual(rv1.Y, rv2.Y, 0.001);
            Assert.AreEqual(rv1.Z, rv2.Z, 0.001);
            Assert.AreEqual(rv1.Vx, rv2.Vx, 0.0001);
            Assert.AreEqual(rv1.Vy, rv2.Vy, 0.0001);
            Assert.AreEqual(rv1.Vz, rv2.Vz, 0.0001);

            Assert.IsTrue(output.Positions.CzmlPositions[1].cartesianVelocity.Length > 1000);

            //  获取段
            //=================================================================
            var seg32w = input.GetSegmentByName("递推32万km") as AgVAMCSPropagate;
            var stop = seg32w.StopConditions[0] as AgVAScalarStoppingCondition;
            Assert.AreEqual(320000000, stop.Trip, 1);

            var seg32w2 = input.GetSegmentByName("地月转移.递推32万km") as AgVAMCSPropagate;
            Assert.AreSame(seg32w, seg32w2);

            var segRlt = output.GetSegmentResultByName("DV1");
            var segRlt2 = output.GetSegmentResultByName("近月制动.DV1");
            Assert.AreSame(segRlt, segRlt2);
            Assert.AreEqual(15.997485220312349, segRlt.FinalState.Geodetic_Latitude, 0.001);

            var segNull = output.GetSegmentResultByName("不存在的段");
            Assert.IsNull(segNull);

        }

        //  上一个例子相同,但没有Target，测试最后一段(环月飞行)的坐标系
        [TestMethod()]
        public void E2M_MoonBrake_250616_2()
        {
            //  输入json文件的路径
            string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

            //  读取输入参数(json)
            string fp = Path.Combine(filePath0, "E2M_MoonBrake_250616-2.json");

            //  读取json文件，并序列化为类对象
            string inputStr = File.ReadAllText(fp, Encoding.UTF8);
            var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

            //  调用webApi            
            var output = input.RunMCS();

            if (!output.IsSuccess)
                Assert.Fail(output.Message);
        }

    }
}
    