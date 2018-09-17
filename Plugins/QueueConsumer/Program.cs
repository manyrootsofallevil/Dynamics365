using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;

namespace QueueConsumer
{
    class Program
    {
        // Connection String for the namespace can be obtained from the Azure portal under the 
        // 'Shared Access policies' section.
        const string ServiceBusConnectionString ="";
        const string QueueName = "searchengines";
        static IQueueClient queueClient;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await queueClient.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false

            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            string keyRoot = "http://schemas.microsoft.com/xrm/2011/Claims/";
            string entityLogicalNameKey = "EntityLogicalName";
            string requestNameKey = "RequestName";
            object entityLogicalNameValue;
            object requestNameValue;
            message.UserProperties.TryGetValue(keyRoot + entityLogicalNameKey, out entityLogicalNameValue);
            message.UserProperties.TryGetValue(keyRoot + requestNameKey, out requestNameValue);

            if (entityLogicalNameValue != null && requestNameValue != null)
            {
                if (entityLogicalNameValue.ToString() == "new_searchengine" && requestNameValue.ToString() == "Create")
                {
                    Console.WriteLine(string.Format("Message received: Id = {0}", message.MessageId));

                    Utility.PrintMessageProperties(message.UserProperties);

                    Console.WriteLine($"Content Type:{message.ContentType}");

                    if (message.ContentType == "application/msbin1")
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(RemoteExecutionContext));

                        using (MemoryStream ms = new MemoryStream(message.Body))
                        {
                            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max);
                            RemoteExecutionContext context = (RemoteExecutionContext)serializer.ReadObject(reader);
                            Utility.Print(context);
                        }
                    }
                    else if (message.ContentType == "application/json")
                    {
                        var ctx = Activator.CreateInstance<RemoteExecutionContext>();

                        using (MemoryStream ms = new MemoryStream(message.Body))
                        {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RemoteExecutionContext));
                            ctx = (RemoteExecutionContext)serializer.ReadObject(ms);
                            Utility.Print(ctx);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException($"Content type: {message.ContentType} has not been implemented.");
                    }
                }
            }

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }

    internal static class Utility
    {

        /// <summary>
        /// Pilfered from CRM Samples, EntityReferences are not properly resolved
        /// </summary>
        /// <param name="context"></param>
        public static void Print(RemoteExecutionContext context)
        {
            if (context == null)
            {
                Console.WriteLine("Context is null.");
                return;
            }

            Console.WriteLine("UserId: {0}", context.UserId);
            Console.WriteLine("OrganizationId: {0}", context.OrganizationId);
            Console.WriteLine("OrganizationName: {0}", context.OrganizationName);
            Console.WriteLine("MessageName: {0}", context.MessageName);
            Console.WriteLine("Stage: {0}", context.Stage);
            Console.WriteLine("Mode: {0}", context.Mode);
            Console.WriteLine("PrimaryEntityName: {0}", context.PrimaryEntityName);
            Console.WriteLine("SecondaryEntityName: {0}", context.SecondaryEntityName);

            Console.WriteLine("BusinessUnitId: {0}", context.BusinessUnitId);
            Console.WriteLine("CorrelationId: {0}", context.CorrelationId);
            Console.WriteLine("Depth: {0}", context.Depth);
            Console.WriteLine("InitiatingUserId: {0}", context.InitiatingUserId);
            Console.WriteLine("IsExecutingOffline: {0}", context.IsExecutingOffline);
            Console.WriteLine("IsInTransaction: {0}", context.IsInTransaction);
            Console.WriteLine("IsolationMode: {0}", context.IsolationMode);
            Console.WriteLine("Mode: {0}", context.Mode);
            Console.WriteLine("OperationCreatedOn: {0}", context.OperationCreatedOn.ToString());
            Console.WriteLine("OperationId: {0}", context.OperationId);
            Console.WriteLine("PrimaryEntityId: {0}", context.PrimaryEntityId);
            Console.WriteLine("OwningExtension LogicalName: {0}", context.OwningExtension.LogicalName);
            Console.WriteLine("OwningExtension Name: {0}", context.OwningExtension.Name);
            Console.WriteLine("OwningExtension Id: {0}", context.OwningExtension.Id);
            Console.WriteLine("SharedVariables: {0}", (context.SharedVariables == null ? "NULL" : SerializeParameterCollection(context.SharedVariables)));
            Console.WriteLine("InputParameters: {0}", (context.InputParameters == null ? "NULL" : SerializeParameterCollection(context.InputParameters)));
            Console.WriteLine("OutputParameters: {0}", (context.OutputParameters == null ? "NULL" : SerializeParameterCollection(context.OutputParameters)));
            Console.WriteLine("PreEntityImages: {0}", (context.PreEntityImages == null ? "NULL" : SerializeEntityImageCollection(context.PreEntityImages)));
            Console.WriteLine("PostEntityImages: {0}", (context.PostEntityImages == null ? "NULL" : SerializeEntityImageCollection(context.PostEntityImages)));
        }

        #region Private methods.
        private static string SerializeEntity(Entity e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(" LogicalName: " + e.LogicalName);
            sb.Append(Environment.NewLine);
            sb.Append(" EntityId: " + e.Id);
            sb.Append(Environment.NewLine);
            sb.Append(" Attributes: [");
            foreach (KeyValuePair<string, object> parameter in e.Attributes)
            {
                sb.Append(parameter.Key + ": " + parameter.Value + "; ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string SerializeParameterCollection(ParameterCollection parameterCollection)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> parameter in parameterCollection)
            {
                if (parameter.Value != null && parameter.Value.GetType() == typeof(Entity))
                {
                    Entity e = (Entity)parameter.Value;
                    sb.Append(parameter.Key + ": " + SerializeEntity(e));
                }
                else
                {
                    sb.Append(parameter.Key + ": " + parameter.Value + "; ");
                }
            }
            return sb.ToString();
        }

        private static string SerializeEntityImageCollection(EntityImageCollection entityImageCollection)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Entity> entityImage in entityImageCollection)
            {
                sb.Append(Environment.NewLine);
                sb.Append(entityImage.Key + ": " + SerializeEntity(entityImage.Value));
            }
            return sb.ToString();
        }
        #endregion

        internal static void PrintMessageProperties(IDictionary<string, object> iDictionary)
        {
            if (iDictionary.Count == 0)
            {
                Console.WriteLine("No Message properties found.");
                return;
            }
            foreach (var item in iDictionary)
            {
                String key = (item.Key != null) ? item.Key.ToString() : "";
                String value = (item.Value != null) ? item.Value.ToString() : "";
                Console.WriteLine(key + " " + value);
            }
        }
    }

}

