using Microsoft.AspNetCore.Mvc;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Abstractions.Repositories;
using TelegramService.Domain.Entities;

namespace TelegramService.Api.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IRegistrationCodeRepository _registrationCodeRepository;

    public UserController(
        ILogger<UserController> logger,
        IRegistrationCodeRepository registrationCodeRepository)
    {
        _logger = logger;
        _registrationCodeRepository = registrationCodeRepository;
    }
    
    [HttpPost("{userId:guid}/code/{code}")]
    public async Task<ActionResult> AddRegisterCodeToUser(
        Guid userId, string code)
    {
        var registrationCode = new RegistrationCode(){ UserId = userId, Code = code, CreationDateTime = DateTime.Now.ToUniversalTime()};
        _registrationCodeRepository.AddCode(registrationCode);
        
        return Ok();
    }
}