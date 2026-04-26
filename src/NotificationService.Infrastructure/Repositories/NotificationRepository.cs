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

    public async Task<(IEnumerable<Notification> Items, int Total)> GetInAppByRecipientAsync(
        string recipientId, int page, int pageSize)
    {
        const string countSql = @"
            SELECT COUNT(*)
            FROM notifications
            WHERE recipient = @RecipientId
              AND channel = 'in-app'
              AND status != 'closed';";

        const string itemsSql = @"
            SELECT id, template, payload, requested_at
            FROM notifications
            WHERE recipient = @RecipientId
              AND channel = 'in-app'
              AND status != 'closed'
            ORDER BY requested_at DESC
            LIMIT @PageSize OFFSET @Offset;";

        var offset = (page - 1) * pageSize;

        await using var conn = CreateConnection();
        var total = await conn.ExecuteScalarAsync<int>(countSql, new { RecipientId = recipientId });
        var rows = await conn.QueryAsync<dynamic>(itemsSql, new { RecipientId = recipientId, PageSize = pageSize, Offset = offset });

        var items = rows.Select(r => new Notification
        {
            Id = r.id,
            Template = r.template,
            Payload = r.payload != null ? JsonDocument.Parse(r.payload) : null,
            RequestedAt = r.requested_at
        });

        return (items, total);
    }

    public async Task<bool> CloseInAppNotificationAsync(Guid id, string recipientId)
    {
        const string sql = @"
            UPDATE notifications
            SET status = 'closed'
            WHERE id = @Id
              AND recipient = @RecipientId
              AND channel = 'in-app';";

        await using var conn = CreateConnection();
        var affected = await conn.ExecuteAsync(sql, new { Id = id, RecipientId = recipientId });
        return affected > 0;
    }

    public async Task ClearInAppNotificationsByRecipientAsync(string recipientId)
    {
        const string sql = @"
            UPDATE notifications
            SET status = 'closed'
            WHERE recipient = @RecipientId
              AND channel = 'in-app'
              AND status != 'closed';";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { RecipientId = recipientId });
    }
}