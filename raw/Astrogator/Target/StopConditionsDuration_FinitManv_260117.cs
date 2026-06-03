using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTargetTests
{
    /*
     测试 Astrogator 
        Target-微分修正：有限推力(VNC姿态),远地点点火满足半长轴
        自变量: StopConditions.Duration
        约束: 点火结束时半长轴
    
    # 飞行任务各段简要说明

    ## 主序列飞行段

    1. **Initial_State（初始状态）**
      - 初始化航天器在地心惯性坐标系中的位置和速度
      - 初始位置：(6678137, 0, 0) 米
      - 初始速度：(0, 6789.5303, 3686.4141) 米/秒
      - 设置航天器物理参数：干质量500kg，燃料500kg
      - 历元：2026-01-17T04:00:00.000Z

    ## 目标序列段 (Inner_Target_List)
    
    1. **dv1（有限推力机动）**
      - 使用VNC姿态控制，姿态更新方式：DuringBurn
      - 发动机：Constant_Thrust_Isp (推力500N, 比冲300s)
      - 停止条件：积分时长500s

    2. **Propagate（滑行段）**
      - 使用地球点质量模型传播
      - 停止条件：远地点(Apoapsis)

    3. **dv2（有限推力机动）**
      - 使用VNC姿态控制，姿态更新方式：DuringBurn
      - 发动机：Constant_Thrust_Isp (推力500N, 比冲300s)
      - 停止条件：积分时长(初值100s)
      - Results: TrueAnomaly, SemimajorAxis
                 
    ## 目标求解配置

    **DC:sma_finiteManv（微分校正器）**
    输出优化过程
    - 控制变量：
      - StopConditions.Duration (dv2段, 初值100s, 最大步长600s)
    - 约束条件：
      - SemimajorAxis = 7650000m (容差1m)

    结果:
        - 微分修正收敛后，dv2段时长约430.528s
        - 最终半长轴达到7650km

    20260117    初次编写
    */
    [TestMethod()]
    public void StopConditionsDuration_FinitManv_260117()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "StopConditionsDuration_FinitManv_260117.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        string outStr = JsonSerializer.Serialize(output, new JsonSerializerOptions() { WriteIndented = true });
        //  保存输出结果
        Console.WriteLine(outStr);

        var seg = output.MainSequenceResults[1] as MCSTargetSequenceResults;
        var seg2 = seg.SegmentResults[2];
        //  dv2段,开始
        /*  STK
        //Parameter Set Type:  Keplerian                                                                                   
        sma: 7170.8909662108544580 km RAAN:     1.288124356675265e-14 deg
        ecc:        0.0678567369475486                  w: 16.71863874295464 deg
        inc:         28.50000000000002 deg TA:         179.9999999989581 deg
        */
        Assert.AreEqual(7170.890966210854458, seg2.InitialState.Keplerian.SemiMajorAxis * 0.001, 1e-6);
        Assert.AreEqual(0.0678567369475486, seg2.InitialState.Keplerian.Eccentricity, 1e-9);
        Assert.AreEqual(16.71863874295464, seg2.InitialState.Keplerian.ArgOfPeriapsis, 1e-9);

        //  dv2段,结束
        Assert.AreEqual(430.528, seg2.DurationSec, 0.001);
        Assert.AreEqual(7650.000, seg2.FinalState.Keplerian.SemiMajorAxis * 0.001, 1e-3);
        Assert.AreEqual(0.0142728094156357, seg2.FinalState.Keplerian.Eccentricity, 1e-7);
    }

}