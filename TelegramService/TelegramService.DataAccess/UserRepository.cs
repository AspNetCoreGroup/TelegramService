using System.Text.Json;
using StackExchange.Redis;
using TelegramService.Domain.Abstractions;

namespace TelegramService.DataAccess;

public class UserRepository : IUserRepository
{
    private readonly IConnectionMultiplexer _redis;

    private const string HashUser = "hashuser";
    
    public UserRepository(
        IConnectionMultiplexer redis
        )
    {
        _redis = redis;
    }
    
    public IEnumerable<User> GetAllUsers()
    {
        var db = _redis.GetDatabase();

        var completeSet = db.HashGetAll(HashUser);

        if (completeSet.Length <= 0) 
            return Enumerable.Empty<User>();
        
        var obj = Array.ConvertAll(completeSet, val => 
            JsonSerializer.Deserialize<User>(val.Value)!).ToList();
        return obj;
    }
    
    public User? GetUserById(Guid id)
    {
        var db = _redis.GetDatabase();

        var plat = db.HashGet(HashUser, id.ToString());

        if (string.IsNullOrEmpty(plat))
            return null;
            
        return JsonSerializer.Deserialize<User>(plat);
    }

    public void CreateUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var db = _redis.GetDatabase();

        if (db.HashExists(HashUser, user.UserId.ToString()))
            throw new Exception($"User '{user.UserId}' already exist");

        var serialPlat = JsonSerializer.Serialize(user);

        db.HashSet(HashUser, new HashEntry[] 
            {new HashEntry(user.UserId.ToString(), serialPlat)});
    }

    public void DeleteUser(Guid id)
    {
        throw new NotImplementedException();
    }

    public void UpdateUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var db = _redis.GetDatabase();

        var serialPlat = JsonSerializer.Serialize(user);

        db.HashSet(HashUser, new HashEntry[] 
            {new HashEntry(user.UserId.ToString(), serialPlat)});
    }
}