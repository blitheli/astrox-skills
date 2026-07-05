using System.Reflection;

using System.Text;

using System.Text.Json;



namespace ASTROX.Astrogator.Tests;



public partial class FollowTests

{



    //  Follow段测试：Joining为Specify+Epoch，Separation为Specify+Duration

    //  Leader(RH120)星历为CzmlPosition、参考系Sun Inertial；MCS中心天体为Sun，输出Sun Inertial

    //  Joining历元2028-01-01，Separation时长86400s；后续Propagate段递推864000s

    //  20260617    初次创建

    [TestMethod()]

    public void FollowSunInertial_260617()

    {

        //  输入json文件的路径

        string filePath0 = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;

        filePath0 = Path.Combine(filePath0, @"Astrogator/Follow");

        //  读取输入参数(json)

        string fp = Path.Combine(filePath0, "FollowSunInertial_260617.json");



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



        //  Follow段：Joining历元2028-01-01，Separation时长86400s

        var followRlt = output.MainSequenceResults[0];

        Assert.AreEqual("Follow", followRlt.TypeName);

        Assert.AreEqual("Sun Inertial", followRlt.InitialState.CoordSystemName);

        Assert.AreEqual("2028-01-01T00:00:00Z", followRlt.InitialState.Epoch);

        Assert.AreEqual("2028-01-02T00:00:00Z", followRlt.FinalState.Epoch);

        Assert.AreEqual(86400.0, followRlt.DurationSec, 1e-6);



        Assert.AreEqual(-59569932.8019133, followRlt.InitialState.Cartesian.X * 0.001, ebsl3);

        Assert.AreEqual(127852255.28320236, followRlt.InitialState.Cartesian.Y * 0.001, ebsl3);

        Assert.AreEqual(56947074.45773242, followRlt.InitialState.Cartesian.Z * 0.001, ebsl3);

        Assert.AreEqual(-27.601854937399607, followRlt.InitialState.Cartesian.Vx * 0.001, ebsl7);

        Assert.AreEqual(-10.281442935882977, followRlt.InitialState.Cartesian.Vy * 0.001, ebsl7);

        Assert.AreEqual(-4.293197613987327, followRlt.InitialState.Cartesian.Vz * 0.001, ebsl7);



        Assert.AreEqual(-61946239.11420142, followRlt.FinalState.Cartesian.X * 0.001, ebsl3);

        Assert.AreEqual(126945992.09282678, followRlt.FinalState.Cartesian.Y * 0.001, ebsl3);

        Assert.AreEqual(56568147.36711177, followRlt.FinalState.Cartesian.Z * 0.001, ebsl3);

        Assert.AreEqual(-27.403977618642995, followRlt.FinalState.Cartesian.Vx * 0.001, ebsl7);

        Assert.AreEqual(-10.696313270441471, followRlt.FinalState.Cartesian.Vy * 0.001, ebsl7);

        Assert.AreEqual(-4.47802752645206, followRlt.FinalState.Cartesian.Vz * 0.001, ebsl7);



        //  递推段：初始状态应与Follow段末状态一致，递推864000s至2028-01-12

        var segRlt1 = output.MainSequenceResults[1];

        Assert.AreEqual("Propagate", segRlt1.TypeName);

        Assert.AreEqual("2028-01-02T00:00:00Z", segRlt1.InitialState.Epoch);

        Assert.AreEqual("2028-01-12T00:00:00Z", segRlt1.FinalState.Epoch);

        Assert.AreEqual(864000.0, segRlt1.DurationSec, 1e-6);



        Assert.AreEqual(followRlt.FinalState.Cartesian.X, segRlt1.InitialState.Cartesian.X, 1.0);

        Assert.AreEqual(followRlt.FinalState.Cartesian.Y, segRlt1.InitialState.Cartesian.Y, 1.0);

        Assert.AreEqual(followRlt.FinalState.Cartesian.Z, segRlt1.InitialState.Cartesian.Z, 1.0);

        Assert.AreEqual(followRlt.FinalState.Cartesian.Vx, segRlt1.InitialState.Cartesian.Vx, 1e-3);

        Assert.AreEqual(followRlt.FinalState.Cartesian.Vy, segRlt1.InitialState.Cartesian.Vy, 1e-3);

        Assert.AreEqual(followRlt.FinalState.Cartesian.Vz, segRlt1.InitialState.Cartesian.Vz, 1e-3);



        Assert.AreEqual(-84646812.51028111, segRlt1.FinalState.Cartesian.X * 0.001, ebsl3);

        Assert.AreEqual(115972518.47517732, segRlt1.FinalState.Cartesian.Y * 0.001, ebsl3);

        Assert.AreEqual(51926236.74507577, segRlt1.FinalState.Cartesian.Z * 0.001, ebsl3);

        Assert.AreEqual(-25.02512315363585, segRlt1.FinalState.Cartesian.Vx * 0.001, ebsl7);

        Assert.AreEqual(-14.639260701476362, segRlt1.FinalState.Cartesian.Vy * 0.001, ebsl7);

        Assert.AreEqual(-6.239064893973839, segRlt1.FinalState.Cartesian.Vz * 0.001, ebsl7);

    }



}


