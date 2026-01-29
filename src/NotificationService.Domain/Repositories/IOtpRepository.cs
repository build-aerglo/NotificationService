using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface IOtpRepository
{
    Task<Otp?> GetByIdAndCodeAsync(string id, string code);
    Task AddAsync(Otp otp);
    Task DeleteByIdAsync(string id);
    Task DeleteByIdAndCodeAsync(string id, string code);
    Task DeleteManyByIdsAsync(IEnumerable<string> ids);
}
