namespace TelegramService.Domain.Entities;

public class RegistrationCode
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreationDateTime { get; set; }
}