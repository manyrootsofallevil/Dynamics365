using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{
    public class Account : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var account = (Entity)context.InputParameters["Target"];

                if (account.LogicalName != "account") { return; }

                try
                {
                    account["websiteurl"] = GetSearchEngine(account["name"].ToString());

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in Account Plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Account: {0}", ex);
                    throw new InvalidPluginExecutionException("An error occurred in Account Plug-in.", ex);
                }
            }

        }

        protected string GetSearchEngine(string accountName)
        {
            var searchEngine = string.Empty;

            List<string> searchEngines = new List<string>() {
                        "bing.com",
                        "dogpile.com",
                        "duckduckgo.com",
                        "google.com",
                        "yippy.com"
                    };

            //Let's see if it there is a first letter match with any of our approved search engines.
            searchEngine = searchEngines.Where(x => x.Substring(0, 1) == accountName[0].ToString()).FirstOrDefault();

            if (searchEngine != null)
            {
                searchEngine = $"https://{searchEngine}";
            }

            return searchEngine;
        }
    }
}
