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
    }
}
