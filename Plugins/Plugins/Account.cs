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
                    List<string> websites = new List<string>() {
                        "https://www.google.com",
                        "https://www.bing.com",
                        "https://duckduckgo.com",
                        "https://yahoo.com"
                    };

                    account["websiteurl"] = websites.Skip(DateTime.Now.Second % 3).First();
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
    }
}
