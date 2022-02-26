using PotatoBot.Services;
using System.Linq;
using Xunit;

namespace PotatoBot.Tests
{
    [Collection("PotatoBot")]
    public class SonarrTest
    {
        private readonly TestSetup _testSetup;
        private readonly SonarrService _sonarr;

        public SonarrTest(TestSetup testSetup)
        {
            _testSetup = testSetup;
            _sonarr = Program.ServiceManager.Sonarr.First();
        }

        [Fact]
        public void TestSearch()
        {
            var results = _sonarr.Search("Person of Interest");
            var firstResult = results.First();

            Assert.NotNull(results);
            Assert.Equal("Person of Interest", firstResult.Title);
            Assert.Equal(2011, firstResult.Year);
        }

        [Fact]
        public void TestFetch()
        {
            var item = _sonarr.GetById(252);
            Assert.NotNull(item);
            Assert.Equal("Person of Interest", item.Title);
            Assert.Equal(2011, item.Year);
        }

        [Fact]
        public void TestQueue()
        {
            var queue = _sonarr.GetQueue();
            Assert.NotNull(queue);

            if(queue.Count > 0)
            {
                Assert.NotNull(queue.First().API);
            }
        }
    }
}
