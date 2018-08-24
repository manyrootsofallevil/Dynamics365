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
                    account["new_searchengine"] = new OptionSetValue(GetSearchEngineOptionSet(account["name"].ToString()));
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
     

        protected int GetSearchEngineOptionSet(string accountName)
        {
            int output = 0;

            Dictionary<int, string> searchEngines = new Dictionary<int, string>();
            searchEngines.Add(100000000, "bing.com");
            searchEngines.Add(100000001, "dogpile.com");
            searchEngines.Add(100000002, "duckduckgo.com");
            searchEngines.Add(100000003, "google.com");
            searchEngines.Add(100000004, "yippy.com");

            //Let's see if it there is a first letter match with any of our approved search engines.
            var searchEngine = searchEngines
                .Where(x => x.Value.ToLower().Substring(0, 1) == accountName.ToLower()[0].ToString())
                .FirstOrDefault();

            if (!searchEngine.Equals(default(KeyValuePair<int,string>)))
            {
                output = searchEngine.Key;
            }

            return output;
        }
    }
}
