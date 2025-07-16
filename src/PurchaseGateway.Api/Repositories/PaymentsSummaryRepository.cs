using Npgsql;
using PurchaseGateway.Api.Models;
using System.Data;
using System.Text;

namespace PurchaseGateway.Api.Repositories;

public class PaymentsSummaryRepository(NpgsqlDataSource dataSource) : IPaymentsSummaryRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<bool> InsertRangeAsync(List<Purchase> payments)
    {
        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO purchase (requested_at, payment_gateway_used, amount) VALUES");

        var parameters = new List<NpgsqlParameter>();
        for (int i = 0; i < payments.Count; i++)
        {
            sql.Append($"(@requested_at_{i}, @payment_gateway_used_{i}, @amount_{i}),");

            var payment = payments[i];
            parameters.Add(new NpgsqlParameter($"@requested_at_{i}", payment.RequestedAt));
            parameters.Add(new NpgsqlParameter($"@payment_gateway_used_{i}", payment.PaymentGatewayUsed));
            parameters.Add(new NpgsqlParameter($"@amount_{i}", payment.Amount));
        }
        sql.Length--;

        Console.WriteLine(sql.ToString());

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql.ToString();
        cmd.Parameters.AddRange(parameters.ToArray());

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        return payments.Count == rowsAffected;
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
            if (reader.GetInt32("payment_gateway_used") == 0)
            {
                result.Default.TotalAmount = reader.GetDecimal("sum");
                result.Default.TotalRequests = reader.GetInt64("count");
                result.Default.TotalRequests = reader.GetInt64("count");
                continue;
            }

            result.Fallback.TotalAmount = reader.GetDecimal("sum");
            result.Fallback.TotalRequests = reader.GetInt64("count");
            result.Fallback.TotalRequests = reader.GetInt64("count");
        }

        return result;
    }

    public async Task PurgeAsync()
    {
        await using var cmd = _dataSource.CreateCommand("DELETE FROM purchase WHERE 1=1;");
        await cmd.ExecuteNonQueryAsync();
    }
}
