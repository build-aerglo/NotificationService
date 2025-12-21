using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

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
        const string sql = "SELECT * FROM notification WHERE id = @Id;";
        await using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Notification>(sql, new { Id = id });
    }
    
    
    public async Task AddAsync(Notification notification)
    {
        const string sql = @"
            INSERT INTO notification 
                (id, notification_type, message_header, message_body, notification_date, notification_status, created_at, updated_at)
            VALUES 
                (@Id, @NotificationType, @MessageHeader, @MessageBody, @NotificationDate, @NotificationStatus, @CreatedAt, @UpdatedAt);";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, notification);
    }
}