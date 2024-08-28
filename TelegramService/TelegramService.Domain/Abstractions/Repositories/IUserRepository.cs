using TelegramService.Domain.Entities;

namespace TelegramService.Domain.Abstractions;

public interface IUserRepository
{
    IEnumerable<User> GetAllUsers();
    User? GetUserById(int id);
    User? GetUserByChatId(long chatId);
    void CreateUser(User user);
    void DeleteUser(int id);
    void UpdateUser(User user);
}