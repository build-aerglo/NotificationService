namespace NotificationService.Domain.Repositories;

public interface IBusinessVerificationRepository
{
    Task<Guid?> GetBusinessIdByEmailAsync(string email);
    Task<Guid?> GetBusinessIdByPhoneAsync(string phone);
    Task SetEmailVerifiedAsync(Guid businessId);
    Task SetPhoneVerifiedAsync(Guid businessId);
}
