using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class ManeuverFiniteTests
{
    /*
     测试 Astrogator
            MCS:
            >   Initial_State       地球惯性系Cartisian
            >   FiniteManeuver      有限推力机动(VNC(Earth))

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
                姿态控制类型: ThrustVector
                AttitudeUpdate: DuringBurn
                "CoordType": "Cartesian",
                "X": 0.6330222216,
                "Y": 0.7544065067,
                "Z": 0.1736481777
            发动机参数: 
                名称: Constant_Thrust_Isp
                发动机类型: AgVAEngineConstant
                推力:500 (N)
                比冲: 300 (s)
            积分器名称: Two_Body_Earth
            停止条件: Duration    500
   
        20220810    初次创建，测试结果与STK结果对比，位置精度~3e-5 (m), 速度精度~3e-8 (m/s)
                    Mulin LIU 
        20230529    重新创建,liyunfei
        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20260117    修改json中StoppingConditions为StopCondtions
    */
    [TestMethod()]
    public void Finite_ThrustVector_230529()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Maneuver");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Finite_ThrustVector_230529.json");

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
        Assert.AreEqual(5596312.6745884192133, output.Position.cartesianVelocity[id - 6], 3e-5);
        Assert.AreEqual(3221764.2009287937981, output.Position.cartesianVelocity[id - 5], 3e-5);
        Assert.AreEqual(1803404.3292714816289, output.Position.cartesianVelocity[id - 4], 3e-5);
        Assert.AreEqual(-4214.6589407234281, output.Position.cartesianVelocity[id - 3], 3e-8);
        Assert.AreEqual(5739.2830727560522, output.Position.cartesianVelocity[id - 2], 3e-8);
        Assert.AreEqual(3330.8563393850541, output.Position.cartesianVelocity[id - 1], 3e-8);
        //  推进剂质量
        Assert.AreEqual(415.0236, output.MainSequenceResults.Last().FinalState.FuelMass, 1e-3);
    }
}