using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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

            var organizationFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var orgService = organizationFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var account = (Entity)context.InputParameters["Target"];

                if (account.LogicalName != "account") { return; }

                try
                {
                  account["new_searchengineid"] = GetSearchEngine(orgService, account["name"].ToString()); 
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

        private EntityReference GetSearchEngine(IOrganizationService service, string accountName)
        {
            EntityReference output = null;

            var query = $@"<fetch version='1.0'  mapping='logical' distinct='true'>
  <entity name='new_searchengine'>
    <attribute name='new_searchengineid' />
    <attribute name='new_name' />  
    <order attribute='new_name' descending='false' />
    <filter type='and'>
      <condition attribute='new_name' operator='like' value = '{accountName[0]}%' />
    </filter>
  </entity>
</fetch>";

            var searchEngines = service.RetrieveMultiple(new FetchExpression(query))?.Entities;        

            var searchEngine = searchEngines
               .Where(se => se["new_name"].ToString().ToLower().Substring(0, 1) == accountName.ToLower()[0].ToString())
               .FirstOrDefault();

            if (searchEngine != null)
            {
                output = new EntityReference("new_searchengine", new Guid(searchEngine["new_searchengineid"].ToString()));
            }

            return output;


        }     
    }
}
