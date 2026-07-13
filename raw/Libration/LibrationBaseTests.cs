namespace AeroSpace.Libration.Tests
{
    [TestClass()]
    public partial class LibrationBaseTests
    {
        //  日-地系 共线平动点 位置
        //  20220531    从原来的位置移植到单独测试

        [TestMethod()]
        public void LibrationPosintPostion_SunEarth()
        {
            double mu = 3.003143144634591e-6;
            Console.WriteLine("共线平动点，mu={0}", mu);

            //# 日-地系
            //# mu = 3.003143144634591e-6
            //# L1平动点位置
            //# L1_x = 0.990026966366713

            double[] pos = LibrationBase.LibrationPointPosition(mu);

            Console.WriteLine("L1点的位置(0.990026966366713)：{0}", pos[3]);
            Console.WriteLine("L2点的位置(1.01003373960387)：{0}", pos[4]);
            Console.WriteLine("L3点的位置：{0}", pos[5]);

            Assert.AreEqual(0.990026966366713, pos[3], 1e-10);
            Assert.AreEqual(1.01003373960387, pos[4], 1e-10);

            Assert.AreEqual(0.0099700304901426673, pos[0], 1e-10);
            Assert.AreEqual(0.010036742747016074, pos[1], 1e-10);
            Assert.AreEqual(0.99999824816649885, pos[2], 1e-10);

            Console.WriteLine("计算完毕！");
        }

        //  地-月系 共线平动点 位置
        //  20220531    初次创建

        [TestMethod()]
        public void LibrationPosintPostion_EarthMoon()
        {
            double mu = 0.01215058560962404;
            Console.WriteLine("共线平动点，mu={0}", mu);

            double[] pos = LibrationBase.LibrationPointPosition(mu);

            //  L1,L2,L3距离附近天体的无量纲距离
            Assert.AreEqual(0.15093428861801883, pos[0], 1e-10);
            Assert.AreEqual(0.16783275105450815, pos[1], 1e-10);
            Assert.AreEqual(0.99291206020065381, pos[2], 1e-10);

            Console.WriteLine("计算完毕！");
        }

        [TestMethod()]
        public void EMLibrationUnit_260710()
        {
            double gm1 = 3.986004418e14; // 地球引力参数(m^3/s^2)
            double gm2 = 4.9048695e12; // 月球引力参数(m^3/s^2)

            double r12 = 3.844e8; // 地月距离(m)

            double mu = gm2 / (gm1 + gm2);
            var emUnit = LibrationBase.GetLibrationUnit(gm1, gm2, r12);
            Assert.AreEqual(mu, emUnit.U, 1e-10);
            Assert.AreEqual(r12, emUnit.UnitL, 1e-10);
            //  约4.34天
            Assert.AreEqual(375189.29688375752, emUnit.UnitT, 1e-6);
            Assert.AreEqual(1024.5494826018351, emUnit.UnitV, 1e-6);
        }
    }
}