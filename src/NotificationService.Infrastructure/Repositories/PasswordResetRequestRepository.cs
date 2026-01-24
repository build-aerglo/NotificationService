using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Repositories;

public class PasswordResetRequestRepository : IPasswordResetRequestRepository
{
    private readonly string _connectionString;

    public PasswordResetRequestRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("PostgresConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<PasswordResetRequest?> GetByIdAsync(string id)
    {
        const string sql = "SELECT * FROM password_reset_requests WHERE id = @Id;";
        await using var conn = CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (result == null) return null;

        return new PasswordResetRequest
        {
            Id = result.id,
            CreatedAt = result.created_at,
            ExpiresAt = result.expires_at
        };
    }

    public async Task AddAsync(PasswordResetRequest request)
    {
        const string sql = @"
            INSERT INTO password_reset_requests (id, created_at, expires_at)
            VALUES (@Id, @CreatedAt, @ExpiresAt);";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            request.Id,
            request.CreatedAt,
            request.ExpiresAt
        });
    }

    public async Task DeleteByIdAsync(string id)
    {
        const string sql = "DELETE FROM password_reset_requests WHERE id = @Id;";
        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
