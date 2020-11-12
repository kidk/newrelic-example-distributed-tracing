# New Relic Distributed tracing example (NodeJS to Dotnet)

## How to run

frontend: `cd frontend && ./build-run.sh [NEWRELIC_LICENSE_KEY] "[SERVICE_BUS_ENDPOINT]"`

backend: `cd frontend && ./build-run.sh [NEWRELIC_LICENSE_KEY] "[SERVICE_BUS_ENDPOINT]"`

Where to find:
* NEWRELIC_LICENSE_KEY: https://docs.newrelic.com/docs/accounts/accounts-billing/account-setup/new-relic-license-key
* SERVICE_BUS_ENDPOINT: https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal <br />
Example: `Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=example;SharedAccessKey=abcadsfewradsfwerasd`

## Distributed tracing code

### Adding the New Relic custom payload

https://github.com/kidk/newrelic-example-distributed-tracing/blob/main/frontend/routes/todo.js#L37
```
// Call newrelic.getTransaction to retrieve a handle on the current transaction.
const transaction = newrelic.getTransaction();
var headerObject = {};
transaction.insertDistributedTraceHeaders(headerObject);

return {
payload: payload,
newrelic: headerObject,
}
```

### Accepting the New Relic custom payload

https://github.com/kidk/newrelic-example-distributed-tracing/blob/main/backend/ServiceBusReceiver.cs#L77

```
var payload = JsonConvert.DeserializeObject<NewRelicPayload>(Encoding.UTF8.GetString(message.Body));

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

// Give back context
IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
ITransaction currentTransaction = agent.CurrentTransaction;
currentTransaction.AcceptDistributedTraceHeaders<NewRelicDistributedTracingPayload>(payload.newRelic, GetHeaderValue, NewRelic.Api.Agent.TransportType.Queue);

```
