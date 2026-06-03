using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class AstrogatorTests
{
    /*
     测试 Astrogator 地月转移
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
            停止条件: R Magnitude              320000 (km)
                Coordinate System Name:  Earth Inertial

        @Propagate3参数
            段名称: TransferOrbit
            积分器名称: CisLunarMoonCentered
            停止条件:
                1.  True Anomaly    0.0 (deg)
                    Central Body:
                                 Moon
                    CoordSystemName:
                                 Moon Inertial
                    Mu:
                                 4.90280030555540e+012
                2.  Altitude          1 (km)
                    CentralBodyName:  Moon
    
        与STK对比时:
            1)  第三体摄动的Mu,如果为CbFile，则需要看Stk中对应中心天体的cb文件中的Mu值
            2） 数值积分器的相对控制误差要一致
            3） EOP-All-v1.1.txt数据要一致！

        STK 12.8 结果(JplDe430)
        Parameter Set Type:  Cartesian                                                                 
         X:  -277710.5489384502288885 km              Vx:       -3.0789567881934077 km/sec     
         Y:  -254911.9040210688544903 km              Vy:       -3.0989819852438147 km/sec     
         Z:   -98702.8827929350663908 km              Vz:       -1.1491487534020981 km/sec  

        20230619    liumulin, 初次创建。

        与STK对比精度: 时间0.002 (s)，位置～0.03 (m)，速度～3e-8 (m/s)

        20231212    修改json输入,不再提供Propagators,引用缺省的积分器
        20251210    由于EOP-V1.1.txt更新了，所以重新使用STK12.8计算了标准值，精度差不多
    */
    [TestMethod()]
    public void E2M_RMag_TrueAnomaly_230619()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Target");

        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "E2M_RMag_TrueAnomaly_230619.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi        
        var output = input.RunMCS();

        //  比较当前编号的t,x,y,z,Vx,Vy,Vz(m,m/s)
        int id = output.Position.cartesianVelocity.Length;
        var rv = output.Position.cartesianVelocity;
        Console.WriteLine($"{rv[id - 6]}  {rv[id - 5]}  {rv[id - 4]}");
        Console.WriteLine($"{rv[id - 3]}  {rv[id - 2]}  {rv[id - 1]}");
        Assert.AreEqual(82125.4909557879873319, output.Position.cartesianVelocity[id - 7], 0.002);
        Assert.AreEqual(-277710548.9384502288885,rv[id - 6], 0.03);
        Assert.AreEqual(-254911904.0210688544903,rv[id - 5], 0.03);
        Assert.AreEqual(-98702882.7929350663908,rv[id - 4], 0.03);
        Assert.AreEqual(-3078.9567881934077,rv[id - 3], 3e-8);
        Assert.AreEqual(-3098.9819852438147,rv[id - 2], 3e-8);
        Assert.AreEqual(-1149.1487534020981,rv[id - 1], 3e-8);

        Assert.IsTrue(output.IsSuccess);
    }

    /**
        -277710548.96234274  -254911904.04524523  -98702882.80179593
        -3078.956788170578  -3098.981985227933  -1149.148753404584
      */
}