using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("users");
            user.HasKey(u => u.Id);

            user.Property(u => u.Id).HasColumnName("id");
            user.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            user.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            user.Property(u => u.Balance).HasColumnName("balance").HasPrecision(18, 2).ValueGeneratedOnAdd();
            user.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Transfer>(transfer =>
        {
            transfer.ToTable("transfers");
            transfer.HasKey(t => t.Id);

            transfer.Property(t => t.Id).HasColumnName("id");
            transfer.Property(t => t.SenderId).HasColumnName("sender_id");
            transfer.Property(t => t.RecipientId).HasColumnName("recipient_id");
            transfer.Property(t => t.Amount).HasColumnName("amount").HasPrecision(18, 2);
            transfer.Property(t => t.Notes).HasColumnName("notes").HasMaxLength(200);
            transfer.Property(t => t.IdempotencyKey).HasColumnName("idempotency_key");
            transfer.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            transfer.HasOne(t => t.Sender).WithMany().HasForeignKey(t => t.SenderId);
            transfer.HasOne(t => t.Recipient).WithMany().HasForeignKey(t => t.RecipientId);
            transfer.HasIndex(t => new { t.SenderId, t.IdempotencyKey }).IsUnique();
        });
    }
}
