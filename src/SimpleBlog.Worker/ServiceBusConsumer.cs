using Azure.Messaging.ServiceBus;
using SimpleBlog.Infrastructure.Messaging;

namespace SimpleBlog.Worker;

// Receives events from Azure Service Bus and turns them into notifications.
// Registered only when a Service Bus connection string is configured.
public class ServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly NotificationDispatcher _dispatcher;
    private readonly ILogger<ServiceBusConsumer> _logger;

    public ServiceBusConsumer(
        ServiceBusClient client,
        ServiceBusOptions options,
        NotificationDispatcher dispatcher,
        ILogger<ServiceBusConsumer> logger)
    {
        _processor = client.CreateProcessor(options.QueueName, new ServiceBusProcessorOptions());
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Service Bus consumer started.");
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var type = args.Message.Subject
            ?? (args.Message.ApplicationProperties.TryGetValue("type", out var t) ? t?.ToString() : null)
            ?? string.Empty;
        var payload = args.Message.Body.ToString();

        await _dispatcher.DispatchAsync(type, payload, args.CancellationToken);
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error from {Source}.", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
