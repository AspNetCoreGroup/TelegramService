using TelegramService.Domain.Abstractions;

namespace TelegramService.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DataContext _dataContext;

    public UserRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    public IEnumerable<User> GetAllUsers()
    {
        var allUsers = _dataContext.Users.ToList();
        
        return allUsers;
    }

    public User? GetUserById(Guid id)
    {
        var user = _dataContext.Users.Find(id);

        return user;
    }

    public User? GetUserByChatId(long chatId)
    {
        var user = _dataContext.Users.FirstOrDefault(user => user.ChatId == chatId);

        return user;
    }

    public void CreateUser(User user)
    {
        var result = _dataContext.Users.Add(user);
        _dataContext.SaveChanges();
    }

    public void DeleteUser(Guid id)
    {
        var personToDelete = _dataContext.Users.Find(id);

        if (personToDelete is null)
        {
            return;
        }

        _dataContext.Users.Remove(personToDelete);
    }

    public void UpdateUser(User user)
    {
        _dataContext.Users.Update(user);
    }
}