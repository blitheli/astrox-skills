using System.Text.Json;
using ASTROX.Celestial;
using ASTROX.Coordinates;
using ASTROX.Geometry;
using ASTROX.NumericalMethods;
using ASTROX.Propagators;
using ASTROX.SegmentPropagation;
using ASTROX.StoppingConditions;
using ASTROX.Time;
using ASTROX.Extended;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
            霍曼转移 Target验证，采用微分修正器，约束参数采用Result

            飞行段:
                初始状态段
                    初始状态，地心惯性坐标系
                    
                转移段
                    转移轨道，地心惯性坐标系
                    终止条件: TrueAnomaly=0

                目标序列段
                    第一次机动段
                        VNC坐标系
                    转移轨道段
                        转移轨道，地心惯性坐标系
                        终止条件: TrueAnomaly=180

                    第二次机动段
                        VNC坐标系

            微分修正器:
                控制参数: 
                    第一次机动段，VNC坐标系X轴
                    第二次机动段，VNC坐标系X轴
                目标序列段
                    约束参数: Result
                    期望值: 
                        远地点距离: 42000000.0
                        偏心率: 0.1

            20250314                    

        */
        [TestMethod()]
        public void Hohmann_Transfer1_250311()
        {

            string motionID = "Satellite";
            string fuelMassName = "Fuel_Mass";
            string dryMassName = "Dry_Mass";

            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            ReferenceFrame frame = earth.InertialFrame;
            double gravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter;
            JulianDate epoch = TimeConstants.J2000.ToTimeStandard(TimeStandard.CoordinatedUniversalTime);
            Motion<Cartesian> initialConditions = new Motion<Cartesian>(new Cartesian(8000000.0, 0.0, 0.0),
                                                                        new Cartesian(1500.0, 7500.0, 0.0));

            NumericalPropagatorDefinition numericalPropagatorDefinition = new NumericalPropagatorDefinition();
            numericalPropagatorDefinition.Epoch = epoch;

            PropagationNewtonianPoint integrationPoint =
                new PropagationNewtonianPoint(motionID,
                                              frame,
                                              initialConditions.Value,
                                              initialConditions.FirstDerivative);

            // simple gravity
            TwoBodyGravity gravity =
                new TwoBodyGravity(integrationPoint.IntegrationPoint,
                                   earth,
                                   gravitationalParameter);
            integrationPoint.AppliedForces.Add(gravity);

            // fuel mass
            PropagationScalar fuelMass = new PropagationScalar(900.0);
            fuelMass.Identification = fuelMassName;
            fuelMass.ScalarDerivative = new ScalarFixed(0.0);

            // dry mass
            PropagationScalar dryMass = new PropagationScalar(100.0);
            dryMass.Identification = dryMassName;
            dryMass.ScalarDerivative = new ScalarFixed(0.0);

            // total mass
            integrationPoint.Mass = fuelMass.IntegrationValue + dryMass.IntegrationValue;

            // configure the propagator
            RungeKuttaFehlberg78Integrator integrator = new RungeKuttaFehlberg78Integrator();
            integrator.RelativeTolerance = Constants.Epsilon13;
            integrator.MinimumStepSize = 1.0;
            integrator.MaximumStepSize = 86400.0;
            integrator.InitialStepSize = 300.0;
            integrator.StepSizeBehavior = KindOfStepSize.Relative;

            numericalPropagatorDefinition.Integrator = integrator;
            numericalPropagatorDefinition.IntegrationElements.Add(integrationPoint);
            numericalPropagatorDefinition.IntegrationElements.Add(fuelMass);
            numericalPropagatorDefinition.IntegrationElements.Add(dryMass);

            // 初始状态段
            NumericalInitialStateSegment initial = new NumericalInitialStateSegment();
            initial.Name = "Initial_State";
            initial.PropagatorDefinition = numericalPropagatorDefinition;

            // 转移段
            NumericalPropagatorSegment propagateToPerigeeSegment = new NumericalPropagatorSegment();
            propagateToPerigeeSegment.Name = "Propagate_To_Perigee";
            propagateToPerigeeSegment.PropagatorDefinition = numericalPropagatorDefinition;

            // perigee stopping condition
            ScalarStoppingCondition perStop =
                new ScalarStoppingCondition(new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                                               integrationPoint.IntegrationPoint,
                                                                               KeplerianElement.TrueAnomaly,
                                                                               frame),
                                            0.0, // threshold in radians
                                            0.0000000001, // value tolerance, radians
                                            StopType.AnyThreshold);
            perStop.AngularSetting = CircularRange.NegativePiToPi; // to avoid the branch cut
            perStop.Name = "Periapsis";

            propagateToPerigeeSegment.StoppingConditions.Add(perStop);

            TargetedSegmentList innerTargetSegment = new TargetedSegmentList();
            innerTargetSegment.Name = "Inner_Target_List";

            // 第一次机动段
            ImpulsiveManeuverSegment dv1 = new ImpulsiveManeuverSegment();
            dv1.Name = "First_Maneuver";

            // maneuver info， 比较原来的例子，没有设置fuelMassName和dryMassName
            ImpulsiveManeuverInformation dv1Info =
                new ImpulsiveManeuverInformation(motionID, new Cartesian(200.0, 0.0, 0.0));//, // first guess
                                                  //fuelMassName,
                                                 //dryMassName,
                                                 //4500.0, // exhaust velocity, meters/second
                                                 //InvalidFuelStateBehavior.ThrowException);

            dv1Info.Orientation = new AxesVelocityOrbitNormal(dv1Info.PropagationPoint, earth);

            dv1.Maneuver = dv1Info;

            // 转移轨道段(近地点->远地点)
            NumericalPropagatorSegment transferOrbit = new NumericalPropagatorSegment();
            transferOrbit.Name = "Transfer_Orbit";
            transferOrbit.PropagatorDefinition = numericalPropagatorDefinition;

            // apogee stopping condition
            ScalarStoppingCondition apoapsisStoppingCondition =
                new ScalarStoppingCondition(new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                                               integrationPoint.IntegrationPoint,
                                                                               KeplerianElement.TrueAnomaly,
                                                                               frame),
                                            Math.PI, // threshold, radians
                                            0.00000001, // value tolerance, radians
                                            StopType.AnyThreshold);
            apoapsisStoppingCondition.AngularSetting = CircularRange.ZeroToTwoPi; // let the system know to avoid the branch cut
            apoapsisStoppingCondition.Name = "Apoapsis_Stopping_Condition";

            transferOrbit.StoppingConditions.Add(apoapsisStoppingCondition);

            // 第二次机动段 ， 比较原来的例子，没有设置fuelMassName和dryMassName
            ImpulsiveManeuverSegment dv2 = new ImpulsiveManeuverSegment();
            dv2.Name = "Second_Maneuver";
            ImpulsiveManeuverInformation dv2Info =
                new ImpulsiveManeuverInformation(motionID, new Cartesian(500.0, 0.0, 0.0));
                                                 //fuelMassName,
                                                 //dryMassName,
                                                 //4500.0, // exhaust velocity, meters/second
                                                 //InvalidFuelStateBehavior.DoFullDeltaV);
            dv2.Maneuver = dv2Info;

            dv2Info.Orientation = new AxesVelocityOrbitNormal(dv2Info.PropagationPoint, earth);

            // 目标序列段: 第一次机动段->转移轨道段->第二次机动段
            innerTargetSegment.Segments.Add(dv1);
            innerTargetSegment.Segments.Add(transferOrbit);
            innerTargetSegment.Segments.Add(dv2);

            // 微分修正器
            #region 微分修正器
            TargetedSegmentListDifferentialCorrector hohmannTransferDifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
            hohmannTransferDifferentialCorrector.Name = "Solve_For_Hohmann_Transfer";

            // variable 1, dv1's x velocity
            DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> dv1VelocityXVariable =
                dv1.CreateVariable(200.0, // maximum step, meters/second
                                   0.1, // perturbation, meters/second
                                   (variable, configuration) => { configuration.Maneuver.X += variable; });
            dv1VelocityXVariable.Name = "ImpulsiveManeuver1_ThrustVector_X";

            // constraint 1, dv1's, 轨道转移段， Result： 远地点距离
            //  注意，这里使用的点是integrationPoint.IntegrationPoint
            ScalarModifiedKeplerianElement apoapsisScalar =
                new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                   integrationPoint.IntegrationPoint,
                                                   KeplerianElement.RadiusOfApoapsis,
                                                   frame);

            ScalarAtEndOfNumericalSegmentConstraint radiusOfApoapsisPostDv1Constraint =
                new ScalarAtEndOfNumericalSegmentConstraint(apoapsisScalar,
                                                            transferOrbit,
                                                            42000000.0, // desired value, meters
                                                            0.0001); // tolerance, meters

            radiusOfApoapsisPostDv1Constraint.Name = "Radius_Of_Apoapsis";

            // variable 2, dv2's x velocity
            DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> dv2VelocityXVariable =
                dv2.CreateVariable(200.0, // maximum step, meters/second
                                   0.1, // perturbation, meters/second
                                   (variable, configuration) => { configuration.Maneuver.X += variable; });
            dv2VelocityXVariable.Name = "ImpulsiveManeuver2_ThrustVector_X";

            // constraint 2,  第二次机动段， Result： 偏心率  
            ScalarModifiedKeplerianElement eccentricityScalar =
                new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                   integrationPoint.IntegrationPoint,
                                                   KeplerianElement.Eccentricity,
                                                   frame);
            ScalarAtEndOfNumericalSegmentConstraint eccentricityPostDv2Constraint =
                new ScalarAtEndOfNumericalSegmentConstraint(eccentricityScalar,
                                                            dv2,
                                                            0.1, // desired value, unitless
                                                            0.000001); // tolerance, unitless

            eccentricityPostDv2Constraint.Name = "Eccentricity";

            // add the variables
            hohmannTransferDifferentialCorrector.Variables.Add(dv1VelocityXVariable);
            hohmannTransferDifferentialCorrector.Variables.Add(dv2VelocityXVariable);

            // add the constraints
            hohmannTransferDifferentialCorrector.Constraints.Add(radiusOfApoapsisPostDv1Constraint);
            hohmannTransferDifferentialCorrector.Constraints.Add(eccentricityPostDv2Constraint);
            #endregion

            // add the corrector to the targeted segment
            innerTargetSegment.Operators.Add(hohmannTransferDifferentialCorrector);

            SegmentList overallList = new SegmentList();
            overallList.Name = "Overall_List";
            overallList.Segments.Add(initial);
            overallList.Segments.Add(propagateToPerigeeSegment);
            overallList.Segments.Add(innerTargetSegment);


            SegmentPropagator propagator = overallList.GetSegmentPropagator(new EvaluatorGroup());
            SegmentListResults propagatorResults = (SegmentListResults)propagator.Propagate();

            // 输出结果
            var targetResult = propagatorResults.SegmentResults[2] as TargetedSegmentListResults;

            var dcResult = targetResult.OperatorResults[0] as TargetedSegmentListDifferentialCorrectorResults;

            var finalIterRlt = dcResult.FunctionSolverResults.FinalIteration;
            var variableValue = finalIterRlt.FunctionResult.GetVariablesUsed();
            var constValue = finalIterRlt.FunctionResult.GetConstraintValues();
            
            Console.WriteLine("微分修正器结果:");
            Console.WriteLine("是否收敛: " + dcResult.Converged);
            Console.WriteLine("迭代次数: " + finalIterRlt.Iteration);
            Console.WriteLine("自变量: " + string.Join(", ", variableValue));
            Console.WriteLine("约束值: " + string.Join(", ", constValue));
            /*
              微分修正器结果:
                是否收敛: True
                迭代次数: 10
                自变量: 1176.339731469334, 759.1125112224238
                约束值: 42000000.000000305, 0.09999999999999748

             */
        }
    }
}