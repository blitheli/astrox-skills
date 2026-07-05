using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASTROX.Astrogator.Tests;

//  Astrogator 测试共享 PlanetsEphemeris / Earth.OrientationParameters 等全局状态，禁止并行。
[DoNotParallelize]
public partial class PropagateTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class AstrogatorTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class AstrogatorTargetTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class ManeuverImpulsiveTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class ManeuverFiniteTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class FollowTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}

[DoNotParallelize]
public partial class AgVAElementTests
{
    [TestCleanup]
    public void ResetEarthEopAfterTest() => AstrogatorTestEnvironment.ResetEarthOrientationParameters();
}
