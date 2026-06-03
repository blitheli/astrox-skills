using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class ManeuverFiniteTests
{
    /*
        测试 Astrogator CirLunar
            MCS:
            >   InitialState    地球惯性系 Cartesian参数
            >   Propagate       CisLunar,大椭圆
            >   ManeuverFinite  有限推力, along velocity vector

        地月转移轨道，从近地到近月, 充分验证月球的摄动效果

        @InitialState参数：
            段名称: Initial_State
            坐标系: 地球惯性系
            中心天体引力常数: 3.986004415E14,
            轨道历元: 2022-06-20T04:00:00.000Z
            坐标类型: Cartesian
            坐标元素: 
                "$type": "Cartesian",
                "X": -4348504.5285496607903,
                "Y": -4721813.3054333820837,
                "Z": -1835916.3669064525948,
                "Vx": 6043.9238667905633,
                "Vy": -7499.6612192427978,
                "Vz": 4972.9769780850104
            结构质量: 500 (kg)
            燃料质量: 500 (kg)
            拖拽系数: 2.2
            拖拽面积: 20 (m^2)
            SRP系数: 1.0
            SRP面积: 20 (m^2)

        @Propagate参数：
            段名称: 轨道递推段
            积分器名称: CisLunar
            停止条件: 
                "$type": "Duration",
                "Trip": 403200

    
        @ManeuverFinite参数：
            段名称: ManeuverFinite
            机动类型: Finite
            姿态参数: 
                姿态控制类型: Along Velocity Vector
                Update During Burn
            发动机参数: 
                名称: Constant_Thrust_Isp
                发动机类型: AgVAEngineConstant
                推力:500 (N)
                比冲: 300 (s)
            积分器名称: CisLunar
            停止条件: 
                "$type": "Duration",
                "Trip": 500

        积分器: CisLunar
            中心天体：     地球
            非球形引力位：
                degree：   8
                order      8
                引力文件： WGS84.grv
                EOP文件：  EOP-v1.1.txt 
                固体潮：   None

            第三体引力：                   
                月球
                太阳
                星表：     JplDE430
                采用输入的引力常数(取自Stk 12.2, .cb文件)
                "Mu": [ 4.90280030555540e+012, 1.3271244004193938E20 ],
                   
        STK 12.8 结果(Jplde430)
    Parameter Set Type:  Cartesian                                                                                   
     X:   278691.9393887486075982 km              Vx:        0.5081760710545217 km/sec                       
     Y:   264833.4267608236987144 km              Vy:       -1.0941437556996361 km/sec                       
     Z:   110256.0826777512847912 km              Vz:        1.8371802222132545 km/sec     

        与STK结果对比精度: 位置精度～3e-3 m, 速度精度～2e-6 m/s， 10^-11的精度
   
        20230525    初次创建
        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20251210    由于EOP-V1.1.txt更新了，所以重新使用STK12.8计算了标准值，精度不变
        20260117    修改json中StoppingConditions为StopCondtions
    */
    [TestMethod()]
    public void Finite_Cirslunar_alongVelocityVector_230525()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Maneuver");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Finite_Cirslunar_alongVelocityVector_230525.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        //  标准值为STK 的计算结果, 86400s 的数值
        //  第多个数值，(与STK中的时间一致)
        int id = output.Position.cartesianVelocity.Length;
        var rv = output.Position.cartesianVelocity;
        Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
        Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
        //  推进剂质量
        Assert.AreEqual(415.0236, output.MainSequenceResults.Last().FinalState.FuelMass, 1e-3);
        Assert.AreEqual(403700, output.Position.cartesianVelocity[id - 7], 1e-6);
        Assert.AreEqual(278691939.3887486075982, rv[id - 6], 3e-3);
        Assert.AreEqual(264833426.7608236987144, rv[id - 5], 3e-3);
        Assert.AreEqual(110256082.6777512847912, rv[id - 4], 3e-3);
        Assert.AreEqual(508.1760710545217, rv[id - 3], 2e-6);
        Assert.AreEqual(-1094.1437556996361, rv[id - 2], 2e-6);
        Assert.AreEqual(1837.1802222132545, rv[id - 1], 2e-6);
        /*
           标准输出: 
        278691939.38772786  264833426.7625241  110256082.67578834
        508.1760727894815  -1094.143755388734  1837.1802215131333
         */
    }
}