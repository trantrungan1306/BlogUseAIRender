namespace SimpleBlog.Application.Common;

// Publishes an integration event to an external broker (e.g. Azure Service Bus).
// Only registered when a broker is configured; otherwise the worker handles events in-process.
public interface IEventPublisher
{
    Task PublishAsync(string type, string payload, CancellationToken ct = default);
}
