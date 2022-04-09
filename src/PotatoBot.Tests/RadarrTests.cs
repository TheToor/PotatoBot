using PotatoBot.Services;
using System.Linq;
using Xunit;

namespace PotatoBot.Tests
{
    [Collection("PotatoBot")]
    public class RadarrTests
    {
        private readonly RadarrService _radarr;

        public RadarrTests(TestSetup testSetup)
        {
            _radarr = testSetup.ServiceManager.Radarr.First();
        }

        [Fact]
        public void TestSearch()
        {
            var results = _radarr.Search("Inception");
            var firstResult = results.First();

            Assert.NotNull(results);
            Assert.Equal("Inception", firstResult.Title);
            Assert.Equal(2010, firstResult.Year);
        }

        [Fact]
        public void TestFetch()
        {
            var item = _radarr.GetById(3879);
            Assert.NotNull(item);
            Assert.Equal("Inception", item.Title);
            Assert.Equal(2010, item.Year);
        }

        [Fact]
        public void TestQueue()
        {
            var queue = _radarr.GetQueue();
            Assert.NotNull(queue);

            if(queue.Count > 0)
            {
                Assert.NotNull(queue.First().API);
            }
        }
    }
}