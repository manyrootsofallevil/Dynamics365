using System;
using NUnit;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest : Account
    {
        [Test]
        public void GetSearchEngineTestGoogle()
        {
            //Arrange
            var accountName = "Guardian";
            //Act
            var searchEngine = GetSearchEngine(accountName);
            //Assert
            Assert.AreEqual("https://google.com", searchEngine);
        }

        [TestCase("Ontological", null)]
        [TestCase("Bastion", "https://bing.com")]
        [TestCase("bastion", "https://bing.com")]
        [TestCase("Distress", "https://dogpile.com")]
        [TestCase("diamon", "https://dogpile.com")]
        [TestCase("Guardian", "https://google.com")]
        [TestCase("guardian", "https://google.com")]
        public void GetSearchEngineTest(string accountName, string expectedSearchEngine)
        {
            //Arrange

            //Act
            var searchEngine = GetSearchEngine(accountName);
            //Assert
            Assert.AreEqual(expectedSearchEngine, searchEngine);
        }
    }
}
