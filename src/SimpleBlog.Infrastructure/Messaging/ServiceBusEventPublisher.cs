using Azure.Messaging.ServiceBus;
using SimpleBlog.Application.Common;

namespace SimpleBlog.Infrastructure.Messaging;

public class ServiceBusOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = "post-events";
}

public class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public ServiceBusEventPublisher(ServiceBusClient client, ServiceBusOptions options)
    {
        _sender = client.CreateSender(options.QueueName);
    }

    public async Task PublishAsync(string type, string payload, CancellationToken ct = default)
    {
        var message = new ServiceBusMessage(payload)
        {
            ContentType = "application/json",
            Subject = type
        };
        message.ApplicationProperties["type"] = type;
        await _sender.SendMessageAsync(message, ct);
    }

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
