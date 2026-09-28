using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Infrastructure.Persistence;

namespace SimpleBlog.Worker;

// Polls the outbox table. When Azure Service Bus is configured it relays events to the broker;
// otherwise it dispatches notifications in-process. This is the outbox pattern relay.
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventPublisher? _publisher;
    private readonly NotificationDispatcher _dispatcher;
    private readonly ILogger<OutboxProcessor> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        NotificationDispatcher dispatcher,
        ILogger<OutboxProcessor> logger,
        IEventPublisher? publisher = null)
    {
        _scopeFactory = scopeFactory;
        _dispatcher = dispatcher;
        _logger = logger;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started (mode: {Mode}).", _publisher is null ? "in-process" : "service-bus");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing the outbox.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            try
            {
                if (_publisher is not null)
                    await _publisher.PublishAsync(message.Type, message.Payload, ct);
                else
                    await _dispatcher.DispatchAsync(message.Type, message.Payload, ct);

                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                _logger.LogWarning(ex, "Failed to process outbox message {Id} (attempt {Attempt}).", message.Id, message.RetryCount);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
