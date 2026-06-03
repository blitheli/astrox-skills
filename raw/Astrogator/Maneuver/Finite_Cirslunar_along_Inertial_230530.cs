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
                InertialAtIgnition
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

        Parameter Set Type:  Cartesian                                                                                   
         X:   278697.0044060199288651 km              Vx:        0.5388450144122499 km/sec                       
         Y:   264831.3688122067833319 km              Vy:       -1.1072089010057953 km/sec                       
         Z:   110251.6346483506640652 km              Vz:        1.8112205120777292 km/sec    
       
    Parameter Set Type:  Cartesian                                                                                   
     X:   278697.0030662625795230 km              Vx:        0.5388474480615082 km/sec                       
     Y:   264831.3717284072772600 km              Vy:       -1.1072102318988819 km/sec                       
     Z:   110251.6297408514510607 km              Vz:        1.8112185844124327 km/sec   
                   
        与STK结果对比精度: 位置精度～3e-3 m, 速度精度～2e-6 m/s， 10^-11的精度

        注意: InertialAtIgnition 这个暂时有BUG，待后续排查!!!
        怀疑是AxesFixedAtEpoch的缘故!!!
   
        20230525    初次创建
        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20260117    修改json中StoppingConditions为StopCondtions
    */
    [TestMethod()]
    public void Finite_Cirslunar_along_Inertial_230530()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Maneuver");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "Finite_Cirslunar_along_Inertial_230530.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();

        int id = output.Position.cartesianVelocity.Length;
        var rv = output.Position.cartesianVelocity;
        Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
        Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
        //  推进剂质量
        Assert.AreEqual(415.0236, output.MainSequenceResults.Last().FinalState.FuelMass, 1e-3);
        Assert.AreEqual(403700,rv[id - 7], 1e-6);
        Assert.AreEqual(278697004.4060199288651, rv[id-6], 3e-3);
        Assert.AreEqual(264831368.8122067833319, rv[id-5], 3e-3);
        Assert.AreEqual(110251634.6483506640652, rv[id-4], 3e-3);
        Assert.AreEqual(538.8450144122499, rv[id-3], 1e-6);
        Assert.AreEqual(-1107.2089010057953, rv[id-2], 1e-6);
        Assert.AreEqual(1811.2205120777292, rv[id-1], 1e-6);
        /*
           标准输出: 
        278691939.38772786  264833426.7625241  110256082.67578834
        508.1760727894815  -1094.143755388734  1837.1802215131333
         */

    }
}