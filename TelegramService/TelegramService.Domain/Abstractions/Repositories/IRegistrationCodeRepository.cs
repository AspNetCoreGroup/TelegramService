using TelegramService.DataAccess;
using TelegramService.Domain.Entities;

namespace TelegramService.Domain.Abstractions.Repositories;

public interface IRegistrationCodeRepository
{
    IEnumerable<RegistrationCode> GetCodesForUser(Guid userId);
    RegistrationCode? GetLastCodeForUser(Guid userId);
    Guid? GetUserIdFromCode(string code);
    void AddCode(RegistrationCode user);
    void DeleteCode(long id);
    void UpdateCode(RegistrationCode user);
}