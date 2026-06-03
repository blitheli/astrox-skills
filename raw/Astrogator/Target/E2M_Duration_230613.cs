using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
     测试 Astrogator 地月转移，多个飞行段，两个积分器模型
            MCS:
            >   Initial_State       地球惯性系Cartisian
            >   Propagate1          CisLunar积分器(中心天体为地球)
            >   Maneuver1           脉冲机动
            >   Propagate2          CisLunar积分器
            >   Propagate3          CisLunarMoonCentered(User Defined)（中心天体为月球）
                
        @Initial State参数：
            段名称: Initial_State
            坐标系: 地球惯性系
            中心天体引力常数: 3.986004415E14,
            轨道历元: 2009-07-01T15:00:00.000Z
            坐标类型: Cartesian
            坐标元素: -3943860.9934800892916 (m)
                     -5140058.1250612303847 (m)
                     -1603813.3403060423916 (m)
                         5989.0302360225034 (m/s)
                        -4827.1079836186574 (m/s)
                          743.0494598292248 (m/s)
            结构质量: 500 (kg)
            燃料质量: 500 (kg)
            拖拽系数: 2.2
            拖拽面积: 20 (m^2)
            SRP系数: 2.0
            SRP面积: 20 (m^2)

        @Propagate1参数：
            段名称: ParkingOrbit
            积分器名称: CisLunar
            停止条件: Duration    3200 (s)

        @Maneuver1参数：
            段名称: Maneuver
            机动类型: Impulsive
            姿态参数: 
                参考轴:    VNC
                姿态控制类型: Thrust Vector(Cartesian)
                            [4000.0, 0.0, 0.0]
        @Propagate2参数
            段名称: TransferOrbitToMoonSOI
            积分器名称: CisLunar
            停止条件: Duration    63545 (s)

        @Propagate3参数
            段名称: TransferOrbit
            积分器名称: CisLunarMoonCentered
            停止条件: Duration    15380 (s)            
    
        与STK对比时:
            1)  第三体摄动的Mu,如果为CbFile，则需要看Stk中对应中心天体的cb文件中的Mu值
            2） 数值积分器的相对控制误差要一致
            3） EOP-All-v1.1.txt数据要一致！

        20230608    liumuin,初次创建。将所有积分段的终止条件调整为时间，用于测试单独积分器造成的误差。
        20230613    liyunfei，修改
  
        与STK对比精度: 位置～2e-4 (m)，速度～1e-8 (m/s)
        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20251210    由于EOP-V1.1.txt更新了,重新计算，与STK12.8比较,精度不变

        STK 12.8 结果（地月转移终点）（Jplde430)
            Parameter Set Type:  Cartesian                                                                 
             X:  -277709.0370493304799311 km              Vx:       -3.0789579231869708 km/sec     
             Y:  -254910.3822988028696273 km              Vy:       -3.0989828694579860 km/sec     
             Z:   -98702.3185158189007780 km              Vz:       -1.1491481377409589 km/sec     
    */
    [TestMethod()]
    public void E2M_Duration_230613()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "E2M_Duration_230613.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi            
        var output = input.RunMCS();

        int id = output.Position.cartesianVelocity.Length;
        var rv = output.Position.cartesianVelocity;
        Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
        Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
        //  与STK结果对比
        Assert.AreEqual(-277709037.0493304799311, rv[id - 6], 2e-4);
        Assert.AreEqual(-254910382.2988028696273, rv[id - 5], 2e-4);
        Assert.AreEqual(-98702318.5158189007780, rv[id - 4], 2e-4);
        Assert.AreEqual(-3078.9579231869708, rv[id - 3], 1e-8);
        Assert.AreEqual(-3098.9828694579860, rv[id - 2], 1e-8);
        Assert.AreEqual(-1149.1481377409589, rv[id - 1], 1e-8);
    }

    /**
        -277709037.04918015  -254910382.29883283  -98702318.51576984
        -3078.957923184082  -3098.982869457509  -1149.1481377357936
      */
}