using Dapper;
using Npgsql;
using PurchaseGateway.Api.Models;
using System.Text;

namespace PurchaseGateway.Api.Repositories;

public class PurchaseRepository(NpgsqlDataSource dataSource) : IPurchaseRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    const string INSERT_QUERY = "INSERT INTO purchase (id, requested_at, payment_gateway_used, amount) VALUES (@CorrelationId, @RequestedAt, @PaymentGatewayUsed, @Amount)";

    [DapperAot]
    public async Task<bool> InsertAsync(Purchase purchase)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(INSERT_QUERY, purchase);

        return rowsAffected == 1;
    }

    [DapperAot]
    public async Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var sql = new StringBuilder("SELECT payment_gateway_used as PaymentGatewayUsed, SUM(amount) as TotalAmount, COUNT(*) as TotalRequests FROM purchase ");

        if (from.HasValue && to.HasValue)
        {
            sql.Append("WHERE requested_at > @from and requested_at < @to ");
        }
        sql.Append("GROUP BY payment_gateway_used");

        await using var connection = await _dataSource.OpenConnectionAsync();
        var queryResult = await connection.QueryAsync<PaymentsSummaryAggregationResult>(sql.ToString(), new { from, to });

        var paymentsSummary = new PaymentsSummaryResponse()
        {
            Default = new PaymentsSummary(),
            Fallback = new PaymentsSummary()
        };

        foreach (var aggregationResult in queryResult)
        {
            if (aggregationResult.PaymentGatewayUsed == 0)
            {
                paymentsSummary.Default.TotalAmount = aggregationResult.TotalAmount;
                paymentsSummary.Default.TotalRequests = aggregationResult.TotalRequests;
                continue;
            }

            paymentsSummary.Fallback.TotalAmount = aggregationResult.TotalAmount;
            paymentsSummary.Fallback.TotalRequests = aggregationResult.TotalRequests;
        }

        return paymentsSummary;
    }

    [DapperAot]
    public async Task PurgeAsync()
    {
        await using var cmd = _dataSource.CreateCommand("DELETE FROM purchase WHERE 1=1;");
        await cmd.ExecuteNonQueryAsync();
    }
}
