using System.Collections.Generic;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest : Account
    {

        [TestCase("Ontological", 0)]
        [TestCase("Bastion", 100000000)]
        [TestCase("bastion", 100000000)]
        [TestCase("Distress", 100000001)]
        [TestCase("diamon", 100000001)]
        [TestCase("Yahoo", 100000004)]
        [TestCase("yep", 100000004)]
        public void GetSearchEngineOptionSetTest(string accountName, int expectedSearchEngine)
        {
            //Arrange
            var searchEngines = new Dictionary<int, string>()
            {
                {100000000, "bing.com"},
                {100000001, "dogpile.com"},
                {100000002, "duckduckgo.com"},
                {100000003, "google.com"},
                {100000004, "yippy.com"},
            };

            //Act
            var searchEngine = GetSearchEngineOptionSet(accountName, searchEngines);
            //Assert
            Assert.AreEqual(expectedSearchEngine, searchEngine);
        }

    }
}
