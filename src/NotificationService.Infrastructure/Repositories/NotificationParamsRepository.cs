using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationParamsRepository : INotificationParamsRepository
{
    private readonly string _connectionString;

    public NotificationParamsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<NotificationParams?> GetNotificationParamsAsync()
    {
        const string sql = @"
            SELECT
                id,
                smtp_host,
                smtp_port,
                smtp_user,
                smtp_password,
                from_email,
                from_name,
                enable_ssl,
                created_at,
                updated_at
            FROM notification_params
            ORDER BY id DESC
            LIMIT 1";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<NotificationParams>(sql);
    }
}
