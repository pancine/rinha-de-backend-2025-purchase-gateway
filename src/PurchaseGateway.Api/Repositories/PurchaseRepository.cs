using Npgsql;
using PurchaseGateway.Api.Models;
using System.Text;
using Z.Dapper.Plus;

namespace PurchaseGateway.Api.Repositories;

public class PurchaseRepository(NpgsqlDataSource dataSource) : IPurchaseRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<bool> InsertAsync(Purchase purchase)
    {
        var sql = "INSERT INTO purchase (id, requested_at, payment_gateway_used, amount) VALUES (@id, @requested_at, @payment_gateway_used, @amount)";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql.ToString();
        cmd.Parameters.Add(new NpgsqlParameter("id", Guid.Parse(purchase.CorrelationId)));
        cmd.Parameters.Add(new NpgsqlParameter("requested_at", purchase.RequestedAt));
        cmd.Parameters.Add(new NpgsqlParameter("payment_gateway_used", purchase.PaymentGatewayUsed));
        cmd.Parameters.Add(new NpgsqlParameter("amount", purchase.Amount));

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        return rowsAffected == 1;
    }

    public async Task InsertRangeAsync(List<Purchase> purchases)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        var result = await connection.BulkInsertAsync(purchases);
    }

    public async Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var sql = new StringBuilder("SELECT payment_gateway_used, SUM(amount), COUNT(1) FROM purchase ");

        var parameters = new List<NpgsqlParameter>(2);
        if (from.HasValue && to.HasValue)
        {
            sql.Append("WHERE requested_at > @from and requested_at < @to ");
            parameters.Add(new NpgsqlParameter("from", from.Value));
            parameters.Add(new NpgsqlParameter("to", to.Value));
        }
        sql.Append("GROUP BY payment_gateway_used");

        await using var cmd = _dataSource.CreateCommand(sql.ToString());
        cmd.Parameters.AddRange(parameters.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new PaymentsSummaryResponse()
        {
            Default = new PaymentsSummary(),
            Fallback = new PaymentsSummary()
        };

        while (await reader.ReadAsync())
        {
            if (reader.GetInt32(reader.GetOrdinal("payment_gateway_used")) == 0)
            {
                result.Default.TotalAmount = reader.GetDecimal(reader.GetOrdinal("sum"));
                result.Default.TotalRequests = reader.GetInt64(reader.GetOrdinal("count"));
                continue;
            }

            result.Fallback.TotalAmount = reader.GetDecimal(reader.GetOrdinal("sum"));
            result.Fallback.TotalRequests = reader.GetInt64(reader.GetOrdinal("count"));
        }

        return result;
    }

    public async Task PurgeAsync()
    {
        await using var cmd = _dataSource.CreateCommand("DELETE FROM purchase WHERE 1=1;");
        await cmd.ExecuteNonQueryAsync();
    }
}
