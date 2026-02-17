using Microsoft.EntityFrameworkCore;
using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TelegramBotToken).HasMaxLength(300).IsRequired();
            entity.Property(x => x.OwnerTelegramChatId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SystemPrompt).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Question).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Answer).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.Store).WithMany(x => x.Faqs).HasForeignKey(x => x.StoreId);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TelegramUserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TelegramUsername).HasMaxLength(100);
            entity.Property(x => x.PendingLeadName).HasMaxLength(200);
            entity.HasIndex(x => new { x.StoreId, x.TelegramUserId }).IsUnique();
            entity.HasOne(x => x.Store).WithMany(x => x.Conversations).HasForeignKey(x => x.StoreId);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(4000).IsRequired();
            entity.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TelegramUserId).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Store).WithMany(x => x.Leads).HasForeignKey(x => x.StoreId);
            entity.HasOne(x => x.Conversation).WithMany().HasForeignKey(x => x.ConversationId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
