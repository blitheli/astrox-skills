using System.Reflection;
using System.Text;
using System.Text.Json;
using ASTROX.Celestial;
using ASTROX.Coordinates;
using ASTROX.Geometry;
using ASTROX.NumericalMethods;
using ASTROX.Propagators;
using ASTROX.SegmentPropagation;
using ASTROX.StoppingConditions;
using ASTROX.Time;

namespace ASTROX.Astrogator.Tests;

public partial class ManeuverFiniteTests
{
    /*
     测试 Astrogator
            MCS:
            >   Initial_State       地球惯性系Cartisian
            >   FiniteManeuver      有限推力机动

        @Initial State参数：
            段名称: Initial_State
            坐标系: 地球惯性系
            中心天体引力常数: 3.986004415E14,
            轨道历元: 2017-06-28T04:00:00.000Z
            坐标类型: Cartesian
            坐标元素: 6678137.0 (m)
                   0.0 (m)
                   0.0 (m)
                   0.0 (m/s)
                   6789.5303002727 (m/s)
                   3686.4141744009 (m/s)
            结构质量: 500 (kg)
            燃料质量: 500 (kg)
            拖拽系数: 2.2
            拖拽面积: 20 (m^2)
            SRP系数: 1.0
            SRP面积: 20 (m^2)

        @FiniteManeuver参数：
            段名称: ManeuverFinite
            机动类型: Finite
            姿态参数: 
                姿态控制类型: VelocityVector
                AttitudeUpdate: DuringBurn
            发动机参数: 
                名称: Constant_Thrust_Isp
                发动机类型: AgVAEngineConstant
                推力:500 (N)
                比冲: 300 (s)
            积分器名称: Two_Body_Earth
            停止条件: Duration    500
   
        20220810    初次创建，测试结果与STK结果对比，位置精度~3e-5 (m), 速度精度~3e-8 (m/s)
                    Mulin LIU 
        20230525    重新创建,liyunfei
        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20260117    修改json中StoppingConditions为StopCondtions
        20260202    增加MCSManeuverFiniteResults的测试

    */
    [TestMethod()]
    public void Finite_alongVelocityVector2_251211()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Maneuver");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Finite_alongVelocityVector2_251211.json");

       
        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        var output = input.RunMCS();
                   
        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        //  标准值为STK 的计算结果, 86400s 的数值            
        int id = output.Position.cartesianVelocity.Length;
        Assert.AreEqual(500, output.Position.cartesianVelocity[id - 7], 1e-6);
        Assert.AreEqual(5580770.3522511319534, output.Position.cartesianVelocity[id - 6], 3e-5);
        Assert.AreEqual(3262467.4981678749646, output.Position.cartesianVelocity[id - 5], 3e-5);
        Assert.AreEqual(1771375.3230153993172, output.Position.cartesianVelocity[id - 4], 3e-5);
        Assert.AreEqual(-4284.7363415342334, output.Position.cartesianVelocity[id - 3], 3e-8);
        Assert.AreEqual(5894.7633029292836, output.Position.cartesianVelocity[id - 2], 3e-8);
        Assert.AreEqual(3200.5953333449524, output.Position.cartesianVelocity[id - 1], 3e-8);
        //  推进剂质量
        Assert.AreEqual(415.0236, output.MainSequenceResults.Last().FinalState.FuelMass, 1e-3);

        //  获取机动段结果
        var segRlt = output.GetSegmentResultByName("ManeuverFinite") as MCSManeuverFiniteResults;
        Assert.AreEqual(84.9764, segRlt.ManeuverInformation.FuelUsed, 0.001);
        Assert.AreEqual(500.0, segRlt.ManeuverInformation.Duration, 0.1);
        Console.WriteLine("有限推力段开始时刻: " + segRlt.ManeuverInformation.Start);
        Console.WriteLine("有限推力段结束时刻: " + segRlt.ManeuverInformation.Stop);
    }



    // 手动创建 FiniteManeuverSegment 测试 - 基于 Finite_alongVelocityVector2_251211.json
    // 测试 推力方向 FixedAtIgnition
    // 20251211  有BUG, FixedAtIgnition 参考系未正确实现
    [TestMethod()]
    public void Finite_alongVelocityVector2_Manual_251211()
    {
        // 1. 定义中心天体和参考系
        EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
        double earthGm = 3.986004415E14;

        // 2. 创建初始条件（根据 JSON 配置）
        JulianDate epoch = new JulianDate(GregorianDate.Parse("2017-06-28T04:00:00.000Z"));
        Cartesian position = new Cartesian(6678137.0, 0.0, 0.0);
        Cartesian velocity = new Cartesian(0.0, 6789.5303002727, 3686.4141744009);
        Motion<Cartesian> initialConditions = new Motion<Cartesian>(position, velocity);

        // 3. 创建传播点
        PropagationNewtonianPoint propagationPoint = new PropagationNewtonianPoint();
        propagationPoint.Identification = "Satellite";
        propagationPoint.IntegrationFrame = earth.InertialFrame;
        propagationPoint.InitialPosition = position;
        propagationPoint.InitialVelocity = velocity;

        // 4. 创建数值传播器定义（Two Body）
        NumericalPropagatorDefinition basePropagator = new NumericalPropagatorDefinition();
        basePropagator.IntegrationElements.Add(propagationPoint);
        //basePropagator.IntegrationElements.Add(fue)
        //  RKF78 积分器
        basePropagator.Integrator = new RungeKuttaFehlberg78Integrator();
        basePropagator.Integrator.InitialStepSize = 60.0;

        // 添加二体引力
        TwoBodyGravity gravity = new TwoBodyGravity(propagationPoint.IntegrationPoint, earth, earthGm);
        propagationPoint.AppliedForces.Add(gravity);

        // 5. 配置质量参数
        double initialFuelMass = 500.0;  // kg
        double initialDryMass = 500.0;   // kg

        PropagationScalar fuelMassScalar = new PropagationScalar(initialFuelMass);
        fuelMassScalar.Identification = "Fuel Mass";
        fuelMassScalar.ScalarDerivative = new ScalarFixed(0.0);

        PropagationScalar dryMassScalar = new PropagationScalar(initialDryMass);
        dryMassScalar.Identification = "Dry Mass";
        dryMassScalar.ScalarDerivative = new ScalarFixed(0.0);

        propagationPoint.Mass = fuelMassScalar.IntegrationValue + dryMassScalar.IntegrationValue;

        basePropagator.IntegrationElements.Add(fuelMassScalar);
        basePropagator.IntegrationElements.Add(dryMassScalar);

        basePropagator.Epoch = epoch;

        // Simply the initial state.  We will need it since the differential corrector will operate 
        // on the initial values.
        NumericalInitialStateSegment initialStateSegment = new NumericalInitialStateSegment();
        initialStateSegment.Name = "Initial_State_Segment";
        initialStateSegment.PropagatorDefinition = basePropagator;

        //=====================================================================================
        // 8. 创建有限推力机动段
        FiniteManeuverSegment segment = new FiniteManeuverSegment();
        segment.PropagatorDefinition = basePropagator;
        segment.Name = "ManeuverFinite_Manual";


        // 6. 创建燃料流量（基于推力和比冲计算）
        double thrust = 500.0;  // N
        double isp = 300.0;     // s
        double g0 = 9.80665;    // m/s^2
        double fuelFlowRate = -thrust / (isp * g0);  // kg/s
        
        ScalarFixed fuelFlow = new ScalarFixed(fuelFlowRate);
        fuelMassScalar.ScalarDerivative = fuelFlow;

       //  推力所在参考轴
        Axes thrustAxes = new AxesVelocityOrbitNormal(propagationPoint.IntegrationPoint);

        //
        var axesFixedAtEngineIgnition = new AxesFixedAtJulianDate(thrustAxes, earth.InertialFrame.Axes, new TimeFromStateValueDefinition(segment.IgnitionState));
        // 创建推力矢量（沿速度方向，InertialAtIgnition 模式）
        Vector thrustVector = ContinuousThrustForceModel.CreateIspThrustVector(
            isp,
            fuelFlow,
            //thrustAxes,
            axesFixedAtEngineIgnition,
            new Cartesian(1, 0, 0)  // X 轴方向（速度方向）
        );

        // 7. 创建连续推力模型
        ContinuousThrustForceModel thrustForce = new ContinuousThrustForceModel(thrustVector, propagationPoint.IntegrationFrame.Axes);

        propagationPoint.AppliedForces.Add(thrustForce);
     
        // 9. 添加停止条件：持续时间 500 秒
        DurationStoppingCondition durationCondition = new DurationStoppingCondition(Duration.FromSeconds(500.0));
        segment.StoppingConditions.Add(durationCondition);


        // 11. 创建段列表并传播
        SegmentList segmentList = new SegmentList();
        segmentList.Segments.Add(initialStateSegment);
        segmentList.Segments.Add(segment);

        SegmentListResults results = segmentList.GetSegmentListPropagator().PropagateSegmentList();            
        FiniteManeuverSegmentResults maneuverResults = (FiniteManeuverSegmentResults)results.GetResultsOfSegment(segment);

        //maneuverResults.FinalPropagatedState.

        DateMotionCollection<Cartesian> overallEphemeris =
            results.GetDateMotionCollectionOfOverallTrajectory("Satellite",earth.InertialFrame);


        // 获取最终状态
        var finalMotion =  overallEphemeris.Motions.Last();
        var finalState = results.SegmentResults[1].FinalPropagatedState;
        var finalPosition = finalMotion.Value;
        var finalVelocity = finalMotion.FirstDerivative;

        Console.WriteLine($"最终位置: X={finalPosition.X:F6}, Y={finalPosition.Y:F6}, Z={finalPosition.Z:F6}");
        Console.WriteLine($"最终速度: Vx={finalVelocity.X:F6}, Vy={finalVelocity.Y:F6}, Vz={finalVelocity.Z:F6}");

        // 获取燃料质量
        //double finalFuelMass = finalState.GetValue(fuelMassScalar.IntegrationValue);
        //Console.WriteLine($"最终燃料质量: {finalFuelMass:F4} kg");

        // 13. 与 STK 结果对比
        Assert.AreEqual(5580770.3522511319534, finalPosition.X, 3e-5);
        Assert.AreEqual(3262467.4981678749646, finalPosition.Y, 3e-5);
        Assert.AreEqual(1771375.3230153993172, finalPosition.Z, 3e-5);
        Assert.AreEqual(-4284.7363415342334, finalVelocity.X, 3e-8);
        Assert.AreEqual(5894.7633029292836, finalVelocity.Y, 3e-8);
        Assert.AreEqual(3200.5953333449524, finalVelocity.Z, 3e-8);
        //Assert.AreEqual(415.0236, finalFuelMass, 1e-3);
    }
}