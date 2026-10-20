using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("users");
            user.HasKey(u => u.Id);

            user.Property(u => u.Id).HasColumnName("id");
            user.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            user.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            user.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            user.HasIndex(u => u.Email).IsUnique();
        });
    }
}
