using Xunit;

namespace PotatoBot.Tests
{
    [CollectionDefinition("PotatoBot")]
    public class TestSetupCollection : IClassFixture<TestSetup>
    {
    }
}
