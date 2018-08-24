using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest
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

            Mock<IOrganizationService> orgService = new Mock<IOrganizationService>();

            orgService.Setup(org => org.Execute(new RetrieveAttributeRequest()))
                .Returns(new RetrieveAttributeResponse() { Results = new ParameterCollection() });
            
            SearchEngineChecker sut = new SearchEngineChecker();

            //Act
            var searchEngine = sut.GetSearchEngineOptionSet(accountName, orgService.Object);
            //Assert
            Assert.AreEqual(expectedSearchEngine, searchEngine);
        }

    }
}
