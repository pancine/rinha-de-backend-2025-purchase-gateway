using Npgsql;
using PurchaseGateway.Api.BackgroundServices;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using StackExchange.Redis;
using System.Threading.Channels;

namespace PurchaseGateway.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddSingleton(NpgsqlDataSource.Create(configuration.GetConnectionString("Postgres")!));

        services.AddKeyedScoped<IPaymentService, DefaultPaymentService>(nameof(DefaultPaymentService));
        services.AddKeyedScoped<IPaymentService, FallbackPaymentService>(nameof(FallbackPaymentService));

        services.AddSingleton(Channel.CreateUnbounded<PaymentsRequest>(new UnboundedChannelOptions { SingleWriter = true }));

        services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        services.AddSingleton<IHostedService, HealthCheckBackgroundService>();

        var workers = configuration.GetValue<int>("PURCHASE_WORKERS");
        for (int i = 0; i < workers; i++)
        {
            services.AddSingleton<IHostedService, PurchaseBackgroundService>();
        }

        return services;
    }
}