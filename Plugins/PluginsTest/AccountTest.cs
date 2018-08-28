using System;
using NUnit;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest : Account
    {

        [TestCase("Ontological", 0)]
        [TestCase("Bastion", 100000000)]
        [TestCase("bastion", 100000000)]
        [TestCase("Distress",100000001 )]
        [TestCase("diamon", 100000001)]
        [TestCase("Yahoo", 100000004)]
        [TestCase("yep", 100000004)]
        public void GetSearchEngineOptionSetTest(string accountName, int expectedSearchEngine)
        {
            //Arrange

            //Act
            
            //Assert
            Assert.AreEqual(expectedSearchEngine, searchEngine);
        }

    }
}
