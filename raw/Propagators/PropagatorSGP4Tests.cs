using System.Text.Json;
using ASTROX;
using ASTROX.Celestial;
using ASTROX.Coordinates;
using ASTROX.Geometry;
using ASTROX.Propagators;
using ASTROX.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroSpace.Propagators.Tests
{
    [TestClass()]
    public class PropagatorSGP4Tests
    {
        /*  测试 由TLE计算卫星星历(ECI下的位置速度)
         *  
         *      webApi结果与本地的Sgp4计算结果对比
         *      
         *      位置精度:0.01m, 速度精度: 0.0001m/s
         *      
         *  20220126    初次编写
         *  20230504    输入文件中Step为Null
         */
        [TestMethod()]
        public void GetSgp4Test()
        {
            string inputStr = """
                {
                  "Start": "2021-05-01T00:00:00.000Z",
                  "Stop": "2021-05-02T12:00:00.000Z",
                  "SatelliteNumber": "25730",
                  "TLEs": [
                    "1 25730U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9997",
                    "2 25730  99.0559 142.6068 0014039 175.9692 333.4962 14.16181681132327"
                  ]
                }
                """;
            var input = JsonSerializer.Deserialize<Sgp4Input>(inputStr);

            var output = PropagatorSGP4.Compute(input);

            //  输出到json文件
            string outputStr = JsonSerializer.Serialize(output);
            Console.WriteLine(outputStr);

            //  首个点的位置、速度
            Cartesian r_0 = new Cartesian(output.Position.cartesianVelocity, 1);
            Cartesian v_0 = new Cartesian(output.Position.cartesianVelocity, 4);

            //---------------------------------------------------------------
            //  直接计算SGP4轨道，进行对比
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;

            TwoLineElementSet scTLE = new TwoLineElementSet(input.TLEs[0] + "\n" + input.TLEs[1]);
            var p = new Sgp4Propagator(scTLE).CreatePoint();
            //  创建卫星在Earth Inertial系下的点积分器
            var pointEvaluator = GeometryTransformer.ObservePoint(p, earth.InertialFrame);
            JulianDate T0 = new JulianDate(GregorianDate.Parse(input.Start));

            Assert.IsTrue(output.IsSuccess);

            //  首个点对比
            var rlt0 = pointEvaluator.Evaluate(T0, 1);
            Assert.AreEqual((rlt0.Value - r_0).Magnitude, 0, 0.01);
            Assert.AreEqual((rlt0.FirstDerivative - v_0).Magnitude, 0, 0.0001);

            /*
            标准输出: 
{
    "IsSuccess":true,
    "Message":"Success",
    "Position":{
        "CentralBody":"Earth",
        "interpolationAlgorithm":"LAGRANGE",
        "interpolationDegree":5,
        "referenceFrame":"INERTIAL",
        "epoch":"2021-05-01T00:00:00.000Z",
        "Interval":null,
        "cartesian":null,
        "cartesianVelocity":[
            0,-269832.68912101083,-1231571.9885748266,-7112993.727909726,-5940.285269496997,4409.545860973774,-548.8518924090486,
            60,-625514.8141626065,-964833.9586369937,-7132403.508315522,-5912.015304124423,4478.890225851514,-97.95530569741851,
            120,-978828.8208736931,-694443.3217763235,-7124743.41143838,-5861.374845055089,4531.262404569591,353.1914073456787,
            ...    
            ]
    }
}
             */
        }

        [TestMethod()]

        public void GetSgp4InFixedSys()
        {    
            JulianDate T0 = new JulianDate(GregorianDate.Parse("2024-07-18T00:00:00.000Z"));

            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            string tle_1 = "1 00005U 58002B   24197.42634405  .00000985  00000-0  12253-2 0  9998";
            string tle_2 = "2 00005  34.2548 320.1210 1842761 214.8082 131.7115 10.85475291366990";

            //  卫星的 两行根数
            TwoLineElementSet scTLE = new TwoLineElementSet(tle_1 + "\n" + tle_2);

            //  创建卫星在Earth Fixed系下的点积分器                  
            var pointEvaluator = GeometryTransformer.ObservePoint(new Sgp4Propagator(scTLE).CreatePoint(), earth.FixedFrame);

            Motion<Cartesian> rv = pointEvaluator.Evaluate(T0, 1);

            Console.WriteLine(rv.Value);
            Console.WriteLine(rv.FirstDerivative);

            /**
                9189376.599965451, 1700375.5314721237, -599200.4168826442
                408.81072676238955, 4564.958727897439, 3377.4405444202876
             */

            //  首个点对比
            Assert.AreEqual(rv.Value.X, 9189376.599965451, 1e-6);
            Assert.AreEqual(rv.Value.Y, 1700375.5314721237, 1e-6);
            Assert.AreEqual(rv.Value.Z, -599200.4168826442, 1e-6);
            Assert.AreEqual(rv.FirstDerivative.X, 408.81072676238955, 1e-9);
            Assert.AreEqual(rv.FirstDerivative.Y, 4564.958727897439, 1e-9);
            Assert.AreEqual(rv.FirstDerivative.Z, 3377.4405444202876, 1e-9);
        }
    }
}