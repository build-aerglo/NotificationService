using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Repositories;

public class BusinessVerificationRepository : IBusinessVerificationRepository
{
    private readonly string _connectionString;

    public BusinessVerificationRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("PostgresConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Guid?> GetBusinessIdByEmailAsync(string email)
    {
        const string sql = "SELECT id FROM businesses WHERE business_email = @Email;";
        await using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { Email = email });
    }

    public async Task<Guid?> GetBusinessIdByPhoneAsync(string phone)
    {
        const string sql = "SELECT id FROM businesses WHERE business_phone_number = @Phone;";
        await using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { Phone = phone });
    }

    public async Task SetEmailVerifiedAsync(Guid businessId)
    {
        const string sql = @"
            UPDATE business_verifications
            SET email_verified = true
            WHERE business_id = @BusinessId;";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { BusinessId = businessId });
    }

    public async Task SetPhoneVerifiedAsync(Guid businessId)
    {
        const string sql = @"
            UPDATE business_verifications
            SET phone_verified = true
            WHERE business_id = @BusinessId;";

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { BusinessId = businessId });
    }
}
