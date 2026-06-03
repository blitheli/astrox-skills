using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{
    /*
     测试 Astrogator  Follow段的Separation的终止条件 作为 自变量
        自变量: StoppingConditions.Duration
        约束: 远地点大地纬度
    
    # 飞行任务各段简要说明

        ## Entities（领航器）
        - EntityPath: LeaderSate
          - Position: AstrogatorMCS
            - InitialState（初始段）
              - CoordSystemName: "Earth Inertial"
              - Keplerian: a=6678137m, e=0, i=28.5deg, RAAN=0, AOP=0, TA=0
              - Epoch: 2018-12-01T00:00:00.000Z
            - Propagate（轨道递推段）
              - PropagatorName: "Earth_Point_Mass"
              - StopConditions:
                - Duration=86400s（1天）

        ## MainSequence（目标序列）
        - TargetSequence: Inner_Target_List
          Segments:
          1) Follow（跟随段）
             - LeaderName: "LeaderSate"
             - Joining: "Specify"
               - JoiningConditions: Duration=43200s（12小时后开始跟随/加入）
             - Separation: "Specify"
               - SeparationConditions: Duration=30s（分离条件：积分固定时长）
             - Text: "初始质量等参数采用缺省值,连接和分离均采用指定时长"

          2) ManeuverImpulsive（脉冲机动段）
             - AttitudeControl: VelocityVector
             - DeltaVMagnitude: 500 m/s

          3) Propagate（递推段）
             - PropagatorName: "Earth_Point_Mass"
             - StopConditions:
               - Apoapsis（到达远地点）
             - Results:
               - Cartographic.Latitude（输出纬度）

        ## DifferentialCorrector（微分修正器）
        - Name: "DC: Duration"
        - ControlParameters（控制变量）
          - Name: "StopConditions.Duration"
          - ParentName: "Follow"
          - InitialValue: 20.0
          - MaxStep: 600.0
          - Perturbation: 0.1
          - Tolerance: 0.001
        - Results（约束）
          - Name: "Latitude"
          - ParentName: "Propagate"
          - DesiredValue: -2
          - Tolerance: 0.1

        期望：
        - DifferentialCorrector 收敛（Converged=true）
        - Follow 段的 Duration 变量被修正到约 309.1（见断言）
        - Propagate 段的 Latitude 约束误差约为 -2（见断言）
 
        20260416    结果收敛!
    */
    [TestMethod()]
    public void StoppingConditionsDuration_260415()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "StoppingConditionsDuration_260415.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);


        var seg = output.MainSequenceResults[0] as MCSTargetSequenceResults;

        //  微分修正结果
        var dcRlt = seg.OperatorResults[0] as TargetOperatorDifferentialCorrectorResults;
        Assert.IsTrue(dcRlt.Converged);
        Assert.AreEqual(double.Parse(dcRlt.ControlParameters[0].FinalValue), 309.1, 0.1);
        Assert.AreEqual(double.Parse(dcRlt.Results[0].CurrentValue), -2, 0.001);
    }

}