using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PurchaseGateway.Api;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton(Channel.CreateUnbounded<PaymentRequest>(new UnboundedChannelOptions { SingleReader = true }));

builder.Services.AddMemoryCache();

builder.Services.AddKeyedSingleton<IPaymentService, DefaultPaymentService>(PaymentGatewaysEnum.Default);
builder.Services.AddKeyedSingleton<IPaymentService, FallbackPaymentServices>(PaymentGatewaysEnum.Fallback);

builder.Services.AddHostedService<HealthCheckBackgroundService>();
builder.Services.AddHostedService<PurchaseBackgroundService>();


var app = builder.Build();

var channelWriter = app.Services.GetRequiredService<Channel<PaymentRequest>>().Writer;
var cache = app.Services.GetRequiredService<IMemoryCache>();
var defaultPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Default);
var fallbackPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Fallback);

app.MapPost("/payments", async (PaymentRequest request) =>
{
    await channelWriter.WriteAsync(request);
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
{
    if (cache.TryGetValue<PaymentsSummaryResponse>(nameof(PaymentsSummaryResponse), out var response))
    {
        return response;
    }

    response = new PaymentsSummaryResponse()
    {
        Default = await defaultPaymentService.GetPaymentsSummaryAsync(),
        Fallback = await fallbackPaymentService.GetPaymentsSummaryAsync()
    };

    cache.Set(nameof(PaymentsSummaryResponse), response, DateTimeOffset.UtcNow.AddSeconds(5));

    return response;
});

app.MapGet("/purge-db", async () =>
{
    await defaultPaymentService.PurgeDatabaseAsync();
    await fallbackPaymentService.PurgeDatabaseAsync();
});

app.Run();