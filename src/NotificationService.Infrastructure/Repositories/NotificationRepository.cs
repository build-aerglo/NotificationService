using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using System.Text.Json;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("PostgresConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);


    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM notifications WHERE id = @Id;";
        await using var conn = CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (result == null) return null;

        return new Notification
        {
            Id = result.id,
            Template = result.template,
            Channel = result.channel,
            RetryCount = result.retry_count,
            Recipient = result.recipient,
            Payload = result.payload != null ? JsonDocument.Parse(result.payload) : null,
            RequestedAt = result.requested_at,
            DeliveredAt = result.delivered_at,
            Status = result.status
        };
    }


    public async Task AddAsync(Notification notification)
    {
        const string sql = @"
            INSERT INTO notifications
                (id, template, channel, retry_count, recipient, payload, requested_at, delivered_at, status)
            VALUES
                (@Id, @Template, @Channel, @RetryCount, @Recipient, @Payload::jsonb, @RequestedAt, @DeliveredAt, @Status);";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            notification.Id,
            notification.Template,
            notification.Channel,
            notification.RetryCount,
            notification.Recipient,
            Payload = notification.Payload?.RootElement.ToString(),
            notification.RequestedAt,
            notification.DeliveredAt,
            notification.Status
        });
    }

    public async Task UpdateStatusAsync(Guid id, string status, DateTime? deliveredAt = null)
    {
        const string sql = @"
            UPDATE notifications
            SET status = @Status, delivered_at = @DeliveredAt
            WHERE id = @Id;";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id, Status = status, DeliveredAt = deliveredAt });
    }
}