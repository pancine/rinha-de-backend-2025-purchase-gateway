using Microsoft.AspNetCore.Mvc;
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
var defaultPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Default);
var fallbackPaymentService = app.Services.GetRequiredKeyedService<IPaymentService>(PaymentGatewaysEnum.Fallback);

app.MapPost("/payments", async (PaymentRequest request) =>
{
    await channelWriter.WriteAsync(request);
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
{
    // TODO: Get the summary

    return new PaymentsSummaryResponse()
    {
        Default = new ProcessedRequests(),
        Fallback = new ProcessedRequests()
    };
});

app.MapGet("/purge-db", async () =>
{
    defaultPaymentService.
});

app.Run();