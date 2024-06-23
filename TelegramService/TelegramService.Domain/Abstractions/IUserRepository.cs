using TelegramService.DataAccess;

namespace TelegramService.Domain.Abstractions;

public interface IUserRepository
{
    IEnumerable<User> GetAllUsers();
    User? GetUserById(Guid id);
    void CreateUser(User user);
    void DeleteUser(Guid id);
    void UpdateUser(User user);
}