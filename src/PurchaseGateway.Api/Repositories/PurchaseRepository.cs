using Dapper;
using Npgsql;
using PurchaseGateway.Api.Models;
using System.Text;
using Z.Dapper.Plus;

namespace PurchaseGateway.Api.Repositories;

public class PurchaseRepository(NpgsqlDataSource dataSource) : IPurchaseRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    const string INSERT_QUERY = "INSERT INTO purchase (id, requested_at, payment_gateway_used, amount) VALUES (@CorrelationId, @RequestedAt, @PaymentGatewayUsed, @Amount)";

    public async Task<bool> InsertAsync(Purchase purchase)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(INSERT_QUERY, purchase);

        return rowsAffected == 1;
    }

    public async Task InsertRangeAsync(List<Purchase> purchases)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var result = await connection.BulkInsertAsync(purchases);
    }

    public async Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var sql = new StringBuilder("SELECT payment_gateway_used, SUM(amount), COUNT(*) FROM purchase ");

        if (from.HasValue && to.HasValue)
        {
            sql.Append("WHERE requested_at > @from and requested_at < @to ");
        }
        sql.Append("GROUP BY payment_gateway_used");

        await using var connection = await _dataSource.OpenConnectionAsync();
        var queryResult = await connection.QueryAsync<(int paymentGatewayUsed, decimal totalAmount, long totalRequests)>
            (sql.ToString(), new { from, to });

        var paymentsSummary = new PaymentsSummaryResponse()
        {
            Default = new PaymentsSummary(),
            Fallback = new PaymentsSummary()
        };

        foreach (var (paymentGatewayUsed, totalAmount, totalRequests) in queryResult)
        {
            if (paymentGatewayUsed == 0)
            {
                paymentsSummary.Default.TotalAmount = totalAmount;
                paymentsSummary.Default.TotalRequests = totalRequests;
                continue;
            }

            paymentsSummary.Fallback.TotalAmount = totalAmount;
            paymentsSummary.Fallback.TotalRequests = totalRequests;
        }

        return paymentsSummary;
    }

    public async Task PurgeAsync()
    {
        await using var cmd = _dataSource.CreateCommand("DELETE FROM purchase WHERE 1=1;");
        await cmd.ExecuteNonQueryAsync();
    }
}
