using Npgsql;
using PurchaseGateway.Api.BackgroundServices;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace PurchaseGateway.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        services.AddSingleton(NpgsqlDataSource.Create(configuration.GetConnectionString("Postgres")!));

        services.AddKeyedScoped<IPaymentService, DefaultPaymentService>(nameof(DefaultPaymentService));
        services.AddKeyedScoped<IPaymentService, FallbackPaymentService>(nameof(FallbackPaymentService));

        services.AddSingleton(Channel.CreateUnbounded<PaymentsRequest>(new UnboundedChannelOptions { SingleWriter = true }));

        services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        var workers = configuration.GetValue<int>("PURCHASE_WORKERS");
        for (int i = 0; i < workers; i++)
        {
            services.AddSingleton<IHostedService, PurchaseBackgroundService>();
        }

        return services;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PaymentsRequest))]
[JsonSerializable(typeof(PaymentsSummaryResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{ }
