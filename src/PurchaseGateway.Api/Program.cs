using Microsoft.AspNetCore.Mvc;
using PurchaseGateway.Api.Extensions;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

var builder = WebApplication.CreateSlimBuilder(args);
var configuration = builder.Configuration.AddEnvironmentVariables().Build();
builder.Services.ConfigureServices(configuration);


var app = builder.Build();


var channel = app.Services.GetRequiredService<Channel<PaymentsRequest>>();
app.MapPost("/payments", async ([FromBody] PaymentsRequest paymentsRequest) =>
{
    await channel.Writer.WriteAsync(paymentsRequest);
});


var repository = app.Services.GetRequiredService<IPurchaseRepository>();
app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
{
    return await repository.GetPaymentsSummaryAsync(from, to);
});


var defaultService = app.Services.GetRequiredKeyedService<IPaymentService>(nameof(DefaultPaymentService));
var fallbackService = app.Services.GetRequiredKeyedService<IPaymentService>(nameof(FallbackPaymentService));
app.MapGet("/purge-db", async () =>
{
    await Task.WhenAll(
        defaultService.PurgeDatabaseAsync(),
        fallbackService.PurgeDatabaseAsync(),
        repository.PurgeAsync()
    );
});


app.Run();