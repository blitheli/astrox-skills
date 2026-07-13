using AeroSpace.MathLib;

namespace AeroSpace.Libration.Tests;

public partial class LibrationBaseTests
{
    /// <summary>
    /// 使用侯锡云 地月L2点halo轨道测试,精度较好
    /// </summary>
    [TestMethod()]
    public void HaloOrbitEigenVector()
    {

        double U = 1.215058560962404E-002;  //地月
        //  halo轨道初值(质心会合系)
        double[] X0 = new double[] { 1.18076305054483, 0, 1.176973182629384E-002, 0, -0.156655153705165, 0 };
        double TT = 3.41439840617371;   //1个轨道周期       

        int NN = 10000;
        double[][] XN, STN, UTN;
        LibrationBase.HaloOrbitEigenVector(U, X0, TT, NN, out XN, out STN, out UTN);

        //  侯锡云给出的结果
        double[] XN_last = new double[] { 1.18076304173889, 5.348830266809257E-005, 1.176973027277787E-002, 5.166108279367313E-005, -0.156655124530030, 9.101656968849100E-006 };
        double[] STN_last = new double[] { 0.416210228164521, 0.148029501257312, 9.655733227986433E-003, -0.786579880131975, -0.428983750349742, -4.503458284493498E-002 };

        double[] dx = XN_last.Sub(XN[NN - 1]);
        double[] dSn = STN_last.Sub(STN[NN - 1]);
        Console.WriteLine("dX_last:" + dx.ArrayToString());
        Console.WriteLine("dSTN_last: " + dSn.ArrayToString());
        double ebsl6 = 2e-6;
       
        Assert.AreEqual(0, dx.Mag(), ebsl6);
        Assert.AreEqual(0, dSn.Mag(), ebsl6);
        /*  最后一点的稳定流形特征向量本身较小(1e-4量级），归一化之后达到1e-7精度
         dX_last:     2.86748402800185E-12     -1.0224093707073E-12     6.59958199200616E-14     5.41106604510545E-12    -2.93648438898231E-12     3.10570710113285E-13
        dSTN_last:     -6.07433844501681E-07     1.18727187969903E-07    -1.40153534299026E-08     -6.3944230555002E-07     6.27620472093415E-07    -3.65831492182833E-08
         */
    }
}