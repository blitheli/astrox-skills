using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASTROX.Astrogator.Tests;

public partial class FollowTests
{

    //  Follow段的测试，Leader的星历为多段数据CzmlPosition数据(CompositePoint类型),且为Earth Inertial
    //  本卫星的中心天体为 Moon, 输出的坐标系为Moon Inertial
    //  测试 是否能够正确 进行坐标系转换，和正确的递推，验证结果与平台的结果一致
    //  20260616    初次创建
    [TestMethod()]
    public void CzmlPositions_260616()
    {
        //  输入json文件的路径
        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");
        //  读取输入参数(json)
        string fp = Path.Combine(filePath0, "CzmlPositions_260616.json");

        //  读取json文件，并序列化为类对象
        string inputStr = File.ReadAllText(fp, Encoding.UTF8);
        var input = JsonSerializer.Deserialize<AstrogatorMCS>(inputStr);

        //  调用webApi
        var output = input.RunMCS();

        if (!output.IsSuccess)
            Assert.Fail(output.Message);

        
        string mcsRlts = JsonSerializer.Serialize(output.MainSequenceResults, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(mcsRlts);

        double ebsl3 = 1e-3;
        double ebsl7 = 1e-7;
        //  Follow段的结果，Moon Inertial
        var followRlt = output.MainSequenceResults[0];
        Assert.AreEqual(630.534112074, followRlt.InitialState.Cartesian.X * 0.001, ebsl3);
        Assert.AreEqual(-1714.944816253, followRlt.InitialState.Cartesian.Y * 0.001, ebsl3);
        Assert.AreEqual(117.649595139, followRlt.InitialState.Cartesian.Z * 0.001, ebsl3);
        Assert.AreEqual(1.449486595545, followRlt.InitialState.Cartesian.Vx * 0.001, ebsl7);
        Assert.AreEqual(0.584293910444, followRlt.InitialState.Cartesian.Vy * 0.001, ebsl7);
        Assert.AreEqual(0.421086674346, followRlt.InitialState.Cartesian.Vz * 0.001, ebsl7);

        //  递推段 (初始和结束状态), 和平台转换结果对比,平台的精度有点低
        var segRlt1 = output.MainSequenceResults[1];

        Assert.AreEqual(630.534112074, segRlt1.InitialState.Cartesian.X*0.001, ebsl3);
        Assert.AreEqual(-1714.944816253, segRlt1.InitialState.Cartesian.Y * 0.001, ebsl3);
        Assert.AreEqual(117.649595139, segRlt1.InitialState.Cartesian.Z * 0.001, ebsl3);
        Assert.AreEqual(1.449486595545, segRlt1.InitialState.Cartesian.Vx * 0.001, ebsl7);
        Assert.AreEqual(0.584293910444, segRlt1.InitialState.Cartesian.Vy * 0.001, ebsl7);
        Assert.AreEqual(0.421086674346, segRlt1.InitialState.Cartesian.Vz * 0.001, ebsl7);

        Assert.AreEqual(1369.390250191, segRlt1.FinalState.Cartesian.X * 0.001, ebsl3);
        Assert.AreEqual(-1138.475474246, segRlt1.FinalState.Cartesian.Y * 0.001, ebsl3);
        Assert.AreEqual(341.564766248, segRlt1.FinalState.Cartesian.Z * 0.001, ebsl3);
        Assert.AreEqual(0.950824103899, segRlt1.FinalState.Cartesian.Vx * 0.001, ebsl7);
        Assert.AreEqual(1.293798762523, segRlt1.FinalState.Cartesian.Vy * 0.001, ebsl7);
        Assert.AreEqual(0.306520337112, segRlt1.FinalState.Cartesian.Vz * 0.001, ebsl7);
        // kepler根数
        Assert.AreEqual(1792.161222331, segRlt1.FinalState.Keplerian.SemiMajorAxis * 0.001, ebsl3);
        Assert.AreEqual(0.025257677131, segRlt1.FinalState.Keplerian.Eccentricity, ebsl7);
        Assert.AreEqual(15.593595577803, segRlt1.FinalState.Keplerian.Inclination, ebsl7);

        //  验证Position(应该在Moon Inertial坐标系下)，和平台转换结果对比,平台的精度有点低
        double[] rvList = output.Position.cartesianVelocity;
        Assert.AreEqual(630.534112074, rvList[1] * 0.001, ebsl3);
        Assert.AreEqual(-1714.944816253, rvList[2] * 0.001, ebsl3);
        Assert.AreEqual(117.649595139, rvList[3] * 0.001, ebsl3);
        Assert.AreEqual(1.449486595545, rvList[4] * 0.001, ebsl7);
        Assert.AreEqual(0.584293910444, rvList[5] * 0.001, ebsl7);
        Assert.AreEqual(0.421086674346, rvList[6] * 0.001, ebsl7);
        /*
        平台给出的原始结果， TBD: 时间应该 09:09:59.0181s, 排查平台的转换
                目前转换精度较低，位置误差在1m 

        Leader的最后点

         UTCG(格林尼治): 2032-01-19 09:09:59.018          UTC儒略日: 2463250.88193308
        Julian Ephemeris Date: 2463250.8827338205
                       历元秒: 403799.01810254494秒       (历元零点(UTCG): 2032-01-14 17:00:00.000)

    坐标系: Earth Inertial

        参数类型:  笛卡尔
         X: 341592.284139619 km              Vx: 1.068544680399 km/s
         Y: 152335.083069190 km              Vy: 1.173763136941 km/s
         Z:  77982.385319239 km              Vz: 0.921686830041 km/s


        坐标系: Moon Inertial

        参数类型:  笛卡尔
                 X:   630.534112074 km              Vx: 1.449486595545 km/s
                 Y: -1714.944816253 km              Vy: 0.584293910444 km/s
                 Z:   117.649595139 km              Vz: 0.421086674346 km/s

        参数类型:  经典根数
               sma:   1792.162978651 km            RAAN:  276.847923880401 deg
               ecc:  0.025257229526                   w:  163.584048398013 deg
               inc: 15.593595527283 deg              TA: -149.754493358638 deg

        参数类型:  球坐标系
         Right Asc: -69.813028731912 deg       Horiz. FPA: -0.745147067365 deg
              Decl:   3.684098134067 deg          Azimuth: 74.837346458623 deg
               |R|:    1830.969802187 km              |V|: 1.618556378925 km/s

        大地坐标:
          纬度:  4.583223730352 deg
          经度: 87.958019399999 deg
          高度:     93.569802187 km

        =============================================================
            
        UTCG(格林尼治): 2032-01-19 09:19:59.018          UTC儒略日: 2463250.8888775245
            Julian Ephemeris Date: 2463250.889678265
                           历元秒: 404399.01810254494秒       (历元零点(UTCG): 2032-01-14 17:00:00.000)

            坐标系: Moon Inertial

            参数类型:  笛卡尔
                     X:  1369.390250191 km              Vx: 0.950824103899 km/s
                     Y: -1138.475474246 km              Vy: 1.293798762523 km/s
                     Z:   341.564766248 km              Vz: 0.306520337112 km/s

            参数类型:  经典根数
                   sma:   1792.161222331 km            RAAN:  276.847923478930 deg
                   ecc:  0.025257677131                   w:  163.584522277395 deg
                   inc: 15.593595577803 deg              TA: -119.098136619703 deg

            参数类型:  球坐标系
             Right Asc: -39.739255115401 deg       Horiz. FPA: -1.280021719817 deg
                  Decl:  10.857521017664 deg          Azimuth: 78.739458361476 deg
                   |R|:    1813.290531691 km              |V|: 1.634605894864 km/s

            大地坐标:
              纬度:  12.725428491927 deg
              经度: 117.797755664881 deg
              高度:      75.890531691 km
         */
    }

}
