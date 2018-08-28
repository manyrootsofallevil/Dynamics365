using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using Plugins;

namespace PluginsTest
{
    public class AccountTest : Account
    {

        [TestCase("Ontological", null)]
        [TestCase("Bastion", "Bing")]
        [TestCase("bastion", "Bing")]
        [TestCase("Giga", "Google")]
        [TestCase("giga", "Google")]        
        public void GetCorrespondantSearchEngineTest(string accountName, string expectedSearchEngineName)
        {
            //Arrange

            var searchEngineCollection = new List<Entity>() {
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

            };

            //Act

            var searchEngine = GetCorrespondantSearchEngine(searchEngineCollection, accountName);

            var searchEngineName = searchEngineCollection
                .Where(se => se["new_searchengineid"].Equals(searchEngine?.Id))
                .SingleOrDefault()?["new_name"].ToString();

            //Assert
            Assert.AreEqual(expectedSearchEngineName, searchEngineName);
        }

    }
}
