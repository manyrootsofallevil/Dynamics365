using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
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

            var orgFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var orgService = orgFactory.CreateOrganizationService(context.UserId);

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity)) { return; }            

            var account = (Entity)context.InputParameters["Target"];

            if (account.LogicalName != "account") { return; }

            try
            {
                var searchEngines = GetSearchEngines(orgService);

                account["new_searchengine"] =
                    new OptionSetValue(GetSearchEngineOptionSet(account["name"].ToString(), searchEngines));
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

        protected Dictionary<int, string> GetSearchEngines(IOrganizationService orgService)
        {
            var options = new Dictionary<int, string>();

            var attributeRequest = new RetrieveAttributeRequest()
            {
                EntityLogicalName = "account",
                LogicalName = "new_searchengine",
                RetrieveAsIfPublished = true
            };

            var attributeResponse = (RetrieveAttributeResponse)orgService.Execute(attributeRequest);
            var attributeMetadata = (EnumAttributeMetadata)attributeResponse.AttributeMetadata;

            options = attributeMetadata.OptionSet.Options
               .Select(option => new
               {
                   Value = option.Value.HasValue ? (int)option.Value : 0,
                   Text = option.Label.UserLocalizedLabel.Label
               })
               .ToDictionary(opt => opt.Value, opt => opt.Text);

            return options;
        }

        protected int GetSearchEngineOptionSet(string accountName, Dictionary<int, string> searchEngines)
        {
            int output = 0;

            //Let's see if it there is a first letter match with any of our approved search engines.
            var searchEngine = searchEngines
                .Where(se => se.Value.ToLower().Substring(0, 1) == accountName.ToLower()[0].ToString())
                .FirstOrDefault();

            if (!searchEngine.Equals(default(KeyValuePair<int, string>)))
            {
                output = searchEngine.Key;
            }

            return output;
        }
    }
}
