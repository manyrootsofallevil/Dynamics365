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

                    var searchEngineRetriever = new SearchEngineRetriever(orgService);
                    var searchEngineFinder = new SearchEngineFinder(searchEngineRetriever);
                    account["new_searchengineid"] = searchEngineFinder.GetCorrespondantSearchEngine(account["name"].ToString());
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

        protected EntityReference GetCorrespondantSearchEngine(List<Entity> searchEngines, string accountName)
        {
            EntityReference output = null;

            var searchEngine = searchEngines
               .Where(se => se["new_name"].ToString().ToLower().Substring(0, 1) == accountName.ToLower()[0].ToString())
               .FirstOrDefault();

            if (searchEngine != null)
            {
                output = new EntityReference("new_searchengine", new Guid(searchEngine["new_searchengineid"].ToString()));
            }

            return output;
        }

        private List<Entity> GetSearchEngines(IOrganizationService service, string accountName)
        {
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

            return service.RetrieveMultiple(new FetchExpression(query))?.Entities.ToList();
        }
    }


    public interface ISearchEngineRetriever
    {
        List<Entity> GetSearchEngines(string accountName);
    }

    public class SearchEngineRetriever : ISearchEngineRetriever
    {
        public IOrganizationService OrganizationService { get; set; }

        public SearchEngineRetriever( IOrganizationService organizationService)
        {           
            OrganizationService = organizationService;
        }

        public List<Entity> GetSearchEngines(string accountName)
        {
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

            return OrganizationService.RetrieveMultiple(new FetchExpression(query))?.Entities.ToList();
        }
    }

    public class SearchEngineFinder
    {
        ISearchEngineRetriever EngineRetriever { get; set; }

        public SearchEngineFinder(ISearchEngineRetriever searchEngineRetriever) => EngineRetriever = searchEngineRetriever;

        public EntityReference GetCorrespondantSearchEngine(string accountName)
        {
            EntityReference output = null;

            var searchEngines = EngineRetriever.GetSearchEngines( accountName);

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
