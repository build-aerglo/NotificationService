using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly string _connectionString;

    public OtpRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("PostgresConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Otp?> GetByIdAndCodeAsync(string id, string code)
    {
        const string sql = "SELECT * FROM otp WHERE id = @Id AND code = @Code;";
        await using var conn = CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id, Code = code });

        if (result == null) return null;

        return new Otp
        {
            Id = result.id,
            Code = result.code,
            CreatedAt = result.created_at,
            ExpiresAt = result.expires_at
        };
    }

    public async Task AddAsync(Otp otp)
    {
        const string sql = @"
            INSERT INTO otp (id, code, created_at, expires_at)
            VALUES (@Id, @Code, @CreatedAt, @ExpiresAt);";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            otp.Id,
            otp.Code,
            otp.CreatedAt,
            otp.ExpiresAt
        });
    }

    public async Task DeleteByIdAsync(string id)
    {
        const string sql = "DELETE FROM otp WHERE id = @Id;";
        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task DeleteByIdAndCodeAsync(string id, string code)
    {
        const string sql = "DELETE FROM otp WHERE id = @Id AND code = @Code;";
        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id, Code = code });
    }

    public async Task DeleteManyByIdsAsync(IEnumerable<string> ids)
    {
        const string sql = "DELETE FROM otp WHERE id = ANY(@Ids);";
        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Ids = ids.ToArray() });
    }
}
