using TelegramService.Domain.Entities;

namespace TelegramService.Domain.Abstractions.Repositories;

public interface IRegistrationCodeRepository
{
    IEnumerable<RegistrationCode> GetCodesForUser(int userId);
    RegistrationCode? GetLastCodeForUser(int userId);
    int? GetUserIdFromCode(string code);
    void AddCode(RegistrationCode user);
    void DeleteCode(long id);
    void UpdateCode(RegistrationCode user);
}