using Microsoft.AspNetCore.Mvc;
using PurchaseGateway;
using PurchaseGateway.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<PurchaseBackgroundService>();

var app = builder.Build();

app.MapPost("/payments", async (PaymentsRequest request) =>
{
    // Mandar pra fila
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
{
    // pegar os resultados

    return new PaymentsSummaryResponse()
    {
        Default = new ProcessedRequests(),
        Fallback = new ProcessedRequests()
    };
});

app.Run();