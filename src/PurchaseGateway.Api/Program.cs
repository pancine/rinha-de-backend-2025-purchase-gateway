using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using PurchaseGateway.Api;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton(Channel.CreateUnbounded<PaymentRequest>(new UnboundedChannelOptions { SingleReader = true }));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(new NpgsqlSlimDataSourceBuilder(builder.Configuration.GetConnectionString("Postgres")!).Build());
builder.Services.AddScoped<IPaymentsSummaryRepository, PaymentsSummaryRepository>();

builder.Services.AddKeyedSingleton<IPaymentService, DefaultPaymentService>(PaymentGatewaysEnum.Default);
builder.Services.AddKeyedSingleton<IPaymentService, FallbackPaymentServices>(PaymentGatewaysEnum.Fallback);

builder.Services.AddHostedService<HealthCheckBackgroundService>();
builder.Services.AddHostedService<PurchaseBackgroundService>();


var app = builder.Build();

var purchaseChannelWriter = app.Services.GetRequiredService<Channel<PaymentRequest>>().Writer;
app.MapPost("/payments", async (PaymentRequest request) =>
{
    await purchaseChannelWriter.WriteAsync(request);
});

var cache = app.Services.GetRequiredService<IMemoryCache>();
var paymentsSummaryRepository = app.Services.GetRequiredService<IPaymentsSummaryRepository>();
app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
{
    return await paymentsSummaryRepository.GetPaymentsSummaryAsync(from, to);
});

var defaultPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Default);
var fallbackPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Fallback);
app.MapGet("/purge-db", async () =>
{
    await defaultPaymentService.PurgeDatabaseAsync();
    await fallbackPaymentService.PurgeDatabaseAsync();
    await paymentsSummaryRepository.PurgeAsync();
});

app.Run();