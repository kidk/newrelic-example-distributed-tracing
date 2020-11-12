using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using NewRelic.Api.Agent;
using System.Collections.Generic;

namespace ServiceBusMessaging
{
    public interface IServiceBusConsumer
    {
        void RegisterOnMessageHandlerAndReceiveMessages();
        Task CloseQueueAsync();
    }

    public interface IProcessData
    {
        void Process(MyPayload myPayload);
    }

    public struct NewRelicPayload
    {
        public MyPayload payload;
        public NewRelicDistributedTracingPayload newRelic;
    }

    public struct NewRelicDistributedTracingPayload
    {
        public string traceparent;
        public string tracestate;
        public string newrelic;
    }

    public struct MyPayload
    {
        public string action;
    }

    public class ServiceBusConsumer : IServiceBusConsumer
    {
        private readonly IProcessData _processData;
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queueClient;
        private const string QUEUE_NAME = "todoqueue";
        private readonly ILogger _logger;

        public ServiceBusConsumer(IProcessData processData,
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger)
        {
            _processData = processData;
            _configuration = configuration;
            _logger = logger;
            _queueClient = new QueueClient(Environment.GetEnvironmentVariable("SERVICE_BUS"), QUEUE_NAME);
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        [Transaction]
        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var payload = JsonConvert.DeserializeObject<NewRelicPayload>(Encoding.UTF8.GetString(message.Body));

            // Give back context
            IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            ITransaction currentTransaction = agent.CurrentTransaction;
            currentTransaction.AcceptDistributedTraceHeaders<NewRelicDistributedTracingPayload>(payload.newRelic, GetHeaderValue, NewRelic.Api.Agent.TransportType.Queue);

            // Send through data
            _processData.Process(payload.payload);
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Define local function to extract New Relic header
            IEnumerable<string> GetHeaderValue(
                NewRelicDistributedTracingPayload carrier,
                string key)
            {
                var headerValues = new List<string>();
                switch(key.ToLower()) {
                    case "traceparent":
                        headerValues.Add(carrier.traceparent);
                    break;
                    case "tracestate":
                        headerValues.Add(carrier.tracestate);
                    break;
                    case "newrelic":
                        headerValues.Add(carrier.newrelic);
                    break;
                }
                return headerValues;
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _queueClient.CloseAsync();
        }
    }
}
