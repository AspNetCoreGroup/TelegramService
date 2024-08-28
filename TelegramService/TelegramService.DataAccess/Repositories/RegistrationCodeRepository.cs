using Microsoft.EntityFrameworkCore;
using TelegramService.Domain.Abstractions.Repositories;
using TelegramService.Domain.Entities;

namespace TelegramService.DataAccess.Repositories;

public class RegistrationCodeRepository : IRegistrationCodeRepository
{
    private readonly DataContext _dataContext;
    
    public RegistrationCodeRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    public IEnumerable<RegistrationCode> GetCodesForUser(int userId)
    {
        var codesForUser = _dataContext.RegistrationCodes
            .Where(code => code.UserId == userId)
            .ToList();
        
        return codesForUser;
    }

    public RegistrationCode? GetLastCodeForUser(int userId)
    {
        var lastCode = _dataContext.RegistrationCodes
            .Where(code => code.UserId == userId)
            .OrderByDescending(code => code.CreationDateTime)
            .First();

        return lastCode;
    }
    
    public int? GetUserIdFromCode(string code)
    {
        var userId = _dataContext.RegistrationCodes
            .FirstOrDefault(x => x.Code == code)?.UserId;

        return userId;
    }

    public void AddCode(RegistrationCode user)
    {
        var result = _dataContext.RegistrationCodes.Add(user);
        _dataContext.SaveChanges();
    }

    public void DeleteCode(long id)
    {
        var toDelete = _dataContext.RegistrationCodes.Find(id);

        if (toDelete is null)
        {
            return;
        }
        
        // _dataContext.Entry(toDelete).State = EntityState.Deleted;
        // _dataContext.SaveChanges();
        _dataContext.RegistrationCodes.Remove(toDelete);
    }

    public void UpdateCode(RegistrationCode code)
    {
        // TODO
        // _dataContext.RegistrationCodes.Update()
        if (_dataContext.RegistrationCodes.Find(code.Id) is { } codeInDb)
        {
            codeInDb.UserId = code.UserId;
            codeInDb.Code = code.Code;
            codeInDb.CreationDateTime = code.CreationDateTime;
            _dataContext.Entry(codeInDb).State = EntityState.Modified;
            _dataContext.SaveChanges();
        }
        else
        {
            throw new Exception("There is no registration code with that id");
        }
    }
}