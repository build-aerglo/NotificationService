using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface IPasswordResetRequestRepository
{
    Task<PasswordResetRequest?> GetByIdAsync(string id);
    Task AddAsync(PasswordResetRequest request);
    Task DeleteByIdAsync(string id);
}
