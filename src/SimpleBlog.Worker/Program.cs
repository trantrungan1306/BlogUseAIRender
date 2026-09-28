using SimpleBlog.Infrastructure;
using SimpleBlog.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructureWorker(builder.Configuration);
builder.Services.AddSingleton<NotificationDispatcher>();
builder.Services.AddHostedService<OutboxProcessor>();

// Consume from Azure Service Bus only when it's configured.
var serviceBusConn = builder.Configuration.GetConnectionString("ServiceBus")
    ?? builder.Configuration["ServiceBus:ConnectionString"];
if (!string.IsNullOrWhiteSpace(serviceBusConn))
    builder.Services.AddHostedService<ServiceBusConsumer>();

var host = builder.Build();
host.Run();
