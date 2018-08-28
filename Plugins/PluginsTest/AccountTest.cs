using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest
    {
        static object[] engineCases =
        {
            new object[] { "Ontological", null },
            new object[] { "Bastion", "Bing", Guid.NewGuid() },
        };

        [TestCase("Ontological", null)]
        [TestCase("Bastion", "Bing")]
        [TestCase("bastion", "Bing")]
        [TestCase("Giga", "Google")]
        [TestCase("giga", "Google")]
        //[TestCaseSource("engineCases")]
        public void GetCorrespondantSearchEngineTest(string accountName, string expectedSearchEngineName)
        {
            //Arrange

            var mockSearchEngineRetriever = new Mock<ISearchEngineRetriever>();
            mockSearchEngineRetriever.Setup(x => x.GetSearchEngines(accountName)).Returns(

new List<Entity>() {
                new Entity("new_searchengine",Guid.NewGuid())
                {
                    Attributes = new AttributeCollection()
                    {
                        new KeyValuePair<string, object>("new_searchengineid", Guid.NewGuid()),
                        new KeyValuePair<string, object>("new_name", "Bing")
                    }
                },
                new Entity("new_searchengine", Guid.NewGuid())
                {
                    Attributes = new AttributeCollection()
                    {
                        new KeyValuePair<string, object>("new_searchengineid", Guid.NewGuid()),
                        new KeyValuePair<string, object>("new_name", "Google")
                    }
                }

            });

            var searchEngineFinder = new SearchEngineFinder(mockSearchEngineRetriever.Object);

            //Act
            var searchEngine = searchEngineFinder.GetCorrespondantSearchEngine(accountName);

            var searchEngineName = mockSearchEngineRetriever.Object.GetSearchEngines(accountName)
                .Where(se => se["new_searchengineid"].Equals(searchEngine?.Id))
                .SingleOrDefault()?["new_name"].ToString();

            //Assert
            Assert.AreEqual(expectedSearchEngineName, searchEngineName);
        }

    }
}
