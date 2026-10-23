using Xunit;

namespace XrcPackageDocs.Tests;

public sealed class NamesTests
{
    [Theory]
    [InlineData("m_LogEntries", "Log Entries")]
    [InlineData("m_Radius", "Radius")]
    [InlineData("_speed", "Speed")]
    [InlineData("kMaxCount", "Max Count")]
    [InlineData("radiusInMeters", "Radius In Meters")]
    [InlineData("m_UIPressInput", "UI Press Input")]
    [InlineData("XRCKey", "XRC Key")]
    [InlineData("DocumentationTestFeedback", "Documentation Test Feedback")]
    [InlineData("speed", "Speed")]
    public void NicifiesLikeTheUnityInspector(string name, string label)
    {
        Assert.Equal(label, Names.Nicify(name));
    }
}
