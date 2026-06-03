using ASTROX.Celestial;
using ASTROX.Coordinates;
using ASTROX.Geometry;
using ASTROX.NumericalMethods;
using ASTROX.Propagators;
using ASTROX.SegmentPropagation;
using ASTROX.StoppingConditions;
using ASTROX.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests
{
    public partial class AstrogatorTests
    {
        /*
       

        */
        [TestMethod()]
        public void TargetSample_Hohmann_Transfer_250311()
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
            AuxiliaryStateScalar dryMass = new AuxiliaryStateScalar(100.0);
            dryMass.Identification = dryMassName;

            // total mass
            integrationPoint.Mass = fuelMass.IntegrationValue + dryMass.AuxiliaryScalar;

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
            numericalPropagatorDefinition.AuxiliaryElements.Add(dryMass);

            NumericalInitialStateSegment initial = new NumericalInitialStateSegment();
            initial.Name = "Initial_State_Segment";
            initial.PropagatorDefinition = numericalPropagatorDefinition;

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

            ImpulsiveManeuverSegment dv1 = new ImpulsiveManeuverSegment();
            dv1.Name = "First_Maneuver";

            // maneuver info
            ImpulsiveManeuverInformation dv1Info =
                new ImpulsiveManeuverInformation(motionID,
                                                 new Cartesian(200.0, 0.0, 0.0), // first guess
                                                 fuelMassName,
                                                 dryMassName,
                                                 4500.0, // exhaust velocity, meters/second
                                                 InvalidFuelStateBehavior.ThrowException);

            dv1Info.Orientation = new AxesVelocityOrbitNormal(dv1Info.PropagationPoint, earth);

            dv1.Maneuver = dv1Info;

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

            ImpulsiveManeuverSegment dv2 = new ImpulsiveManeuverSegment();
            dv2.Name = "Second_Maneuver";
            ImpulsiveManeuverInformation dv2Info =
                new ImpulsiveManeuverInformation(motionID,
                                                 new Cartesian(500.0, 0.0, 0.0),
                                                 fuelMassName,
                                                 dryMassName,
                                                 4500.0, // exhaust velocity, meters/second
                                                 InvalidFuelStateBehavior.DoFullDeltaV);
            dv2.Maneuver = dv2Info;

            dv2Info.Orientation = new AxesVelocityOrbitNormal(dv2Info.PropagationPoint, earth);

            innerTargetSegment.Segments.Add(dv1);
            innerTargetSegment.Segments.Add(transferOrbit);
            innerTargetSegment.Segments.Add(dv2);

            TargetedSegmentListDifferentialCorrector hohmannTransferDifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
            hohmannTransferDifferentialCorrector.Name = "Solve_For_Hohmann_Transfer";

            // variable 1, dv1's x velocity
            DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> dv1VelocityXVariable =
                dv1.CreateVariable(200.0, // maximum step, meters/second
                                   0.1, // perturbation, meters/second
                                   (variable, configuration) => { configuration.Maneuver.X += variable; });
            dv1VelocityXVariable.Name = "ImpulsiveManeuver1_ThrustVector_X";

            // constraint 1, dv1's
            ScalarModifiedKeplerianElement apoapsisScalar =
                new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                   integrationPoint.IntegrationPoint,
                                                   KeplerianElement.RadiusOfApoapsis,
                                                   frame);

            ScalarAtEndOfNumericalSegmentConstraint radiusOfApoapsisPostDv1Constraint =
                new ScalarAtEndOfNumericalSegmentConstraint(apoapsisScalar,
                                                            transferOrbit,
                                                            0, // desired value, meters, leaving as null since a outer variable will set this
                                                            0.0001); // tolerance, meters

            radiusOfApoapsisPostDv1Constraint.Scalar = apoapsisScalar;
            radiusOfApoapsisPostDv1Constraint.Name = "Radius_Of_Apoapsis";

            // variable 2, dv2's x velocity
            DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> dv2VelocityXVariable =
                dv2.CreateVariable(200.0, // maximum step, meters/second
                                   0.1, // perturbation, meters/second
                                   (variable, configuration) => { configuration.Maneuver.X += variable; });
            dv2VelocityXVariable.Name = "ImpulsiveManeuver2_ThrustVector_X";

            // constraint 2, eccentricity constraint after the second maneuver
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

            eccentricityPostDv2Constraint.Scalar = eccentricityScalar;
            eccentricityPostDv2Constraint.Name = "Eccentricity";

            // add the variables
            hohmannTransferDifferentialCorrector.Variables.Add(dv1VelocityXVariable);
            hohmannTransferDifferentialCorrector.Variables.Add(dv2VelocityXVariable);

            // add the constraints
            hohmannTransferDifferentialCorrector.Constraints.Add(radiusOfApoapsisPostDv1Constraint);
            hohmannTransferDifferentialCorrector.Constraints.Add(eccentricityPostDv2Constraint);

            // add the corrector to the targeted segment
            innerTargetSegment.Operators.Add(hohmannTransferDifferentialCorrector);


            TargetedSegmentList outerTargetedSegmentList = new TargetedSegmentList();
            outerTargetedSegmentList.Name = "Outer_Target_List";

            TargetedSegmentListDifferentialCorrector solverFor24HourPeriod = new TargetedSegmentListDifferentialCorrector();
            solverFor24HourPeriod.Name = "Solving_For_24_Hour_Period";

            // 24 hour period constraint
            ScalarModifiedKeplerianElement periodScalar =
                new ScalarModifiedKeplerianElement(gravitationalParameter,
                                                   integrationPoint.IntegrationPoint,
                                                   KeplerianElement.Period,
                                                   frame);
            ScalarAtEndOfNumericalSegmentConstraint periodConstraint =
                new ScalarAtEndOfNumericalSegmentConstraint(periodScalar,
                                                            dv2,
                                                            TimeConstants.SecondsPerDay, // desired value, seconds
                                                            0.0001); // tolerance, seconds

            periodConstraint.Scalar = periodScalar;
            periodConstraint.Name = "Orbital_Period";

            // vary the desired value of the nested Radius of Apoapsis
            ParameterizedDoubleVariable nestedDesiredRadiusOfApoapsisVariable =
                new ParameterizedDoubleVariable(42000000, // initial value, meters
                                                5000000.0, // maximum step, meters
                                                500000.0, // perturbation, meters
                                                transferOrbit);
            nestedDesiredRadiusOfApoapsisVariable.Name = "Desired_Radius_Of_Apoapsis";
            radiusOfApoapsisPostDv1Constraint.DesiredValue = nestedDesiredRadiusOfApoapsisVariable.Value;

            // add the variable and constraint
            solverFor24HourPeriod.Constraints.Add(periodConstraint);
            solverFor24HourPeriod.Variables.Add(nestedDesiredRadiusOfApoapsisVariable);

            // add the differentialCorrector to the segment
            outerTargetedSegmentList.Operators.Add(solverFor24HourPeriod);


            NumericalPropagatorSegment finalPropagator = new NumericalPropagatorSegment();
            finalPropagator.Name = "Final_Orbit";
            finalPropagator.PropagatorDefinition = numericalPropagatorDefinition;
            finalPropagator.StoppingConditions.Add(new DurationStoppingCondition(Duration.FromDays(1.0)));
            finalPropagator.StoppingConditions[0].Name = "Duration";

            SegmentList overallList = new SegmentList();
            overallList.Name = "Overall_List";
            overallList.Segments.Add(initial);
            overallList.Segments.Add(propagateToPerigeeSegment);

            // add the inner targeted segment list to the outer targeted segment list
            outerTargetedSegmentList.Segments.Add(innerTargetSegment);

            // and add the outer list
            overallList.Segments.Add(outerTargetedSegmentList);
            overallList.Segments.Add(finalPropagator);

            SegmentPropagator propagator = overallList.GetSegmentPropagator(new EvaluatorGroup());
            SegmentListResults propagatorResults = (SegmentListResults)propagator.Propagate();
        }
    }
}