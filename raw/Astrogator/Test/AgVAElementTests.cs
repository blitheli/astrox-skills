using AeroSpace.OrbitCore;

namespace ASTROX.Astrogator.Tests;

[TestClass()]
public partial class AgVAElementTests
{

    //  20260204 Kepler瞬时和平均根数转换测试
    [TestMethod()]
    public void AgVAElementKeplerian_2602024()
    {
        var d2r = Math.PI / 180;
        var r2d = 180 / Math.PI;

        //var elm = new KeplerianElements(6978137, 0.02, 98.193 * d2r, 50 * d2r, 60 * d2r, 70 * d2r, OrbitBase.EarthMu);
        //  瞬时根数
        double sma = 6978137;
        double ecc = 0.02;
        double inc = 98.193;
        double argPeri = 50;
        double raan = 60;
        double trueAnom = 70;
        double mu = OrbitBase.EarthMu;

        var vaElm = new AgVAElementKeplerian(sma, ecc, inc, argPeri, raan, trueAnom, mu);

        //  转换为平均根数
        var meanKepler = vaElm.ToMeanKepler();
        Assert.AreEqual(6982.95025143555, meanKepler.SemiMajorAxis*0.001,1e-6);
        Assert.AreEqual(0.02010212002401, meanKepler.Eccentricity, 1e-10);
        Assert.AreEqual(98.1901748091543, meanKepler.Inclination, 1e-6);
        Assert.AreEqual(59.994580536652, meanKepler.RAAN, 1e-6);
        Assert.AreEqual(53.6517352336034, meanKepler.ArgOfPeriapsis, 1e-6);
        Assert.AreEqual(64.25760156026338, meanKepler.MeanAnomaly, 1e-6);

        //  转换为RV
        var rv0 = vaElm.ToRV();
        //  从RV转换回瞬时根数
        var vaElm2 = new AgVAElementKeplerian(rv0, mu);
        Assert.AreEqual(sma, vaElm2.SemiMajorAxis, 1e-6);
        Assert.AreEqual(ecc, vaElm2.Eccentricity, 1e-10);
        Assert.AreEqual(inc, vaElm2.Inclination, 1e-6);
        Assert.AreEqual(raan, vaElm2.RAAN, 1e-6);
        Assert.AreEqual(argPeri, vaElm2.ArgOfPeriapsis, 1e-6);
        Assert.AreEqual(trueAnom, vaElm2.TrueAnomaly, 1e-6);
    }

}