using Microsoft.EntityFrameworkCore;
using TelegramService.Domain.Entities;

namespace TelegramService.DataAccess;

public sealed class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RegistrationCode> RegistrationCodes { get; set; }

    public DataContext()
    {
        Database.EnsureCreated();
    }
        
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(s => s.UserId);

        modelBuilder.Entity<RegistrationCode>()
            .HasKey(s => s.Id);
    }
}