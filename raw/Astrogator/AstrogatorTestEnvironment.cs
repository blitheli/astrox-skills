using AeroSpace.Celestial;
using AeroSpace.IO;
using ASTROX.Celestial;

namespace ASTROX.Astrogator.Tests;

/// <summary>
/// 批量运行 Astrogator 测试时恢复地球 EOP，避免 HPOP / Follow 等算例污染后续测试。
/// </summary>
internal static class AstrogatorTestEnvironment
{
    internal static void ResetEarthOrientationParameters()
    {
        CentralBodiesFacet.GetFromContext().Earth.OrientationParameters = new EarthOrientationParameters();
    }
}
