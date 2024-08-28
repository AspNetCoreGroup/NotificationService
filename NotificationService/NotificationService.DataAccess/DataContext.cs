using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.DataAccess;

public class DataContext : DbContext
{
    public DbSet<User?> Users { get; set; }
    public DbSet<MessageTracking> MessageTrackings { get; set; }
    public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<MessageDeliveryStatus> MessageDeliveryStatuses { get; set; }
    

    public DataContext()
    {
        
    }
        
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Определение первичных ключей
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<MessageTracking>()
            .HasKey(mt => mt.MessageId);
        
        modelBuilder.Entity<SubscriptionType>()
            .HasKey(st => st.Id);
        
        modelBuilder.Entity<UserSubscription>()
            .HasKey(us => new { us.UserId, us.SubscriptionTypeId });
        
        modelBuilder.Entity<MessageDeliveryStatus>()
            .HasKey(mds => mds.Id);

        // Определение внешних ключей и связей между таблицам
        modelBuilder.Entity<MessageTracking>()
            .HasOne(mt => mt.User)
            .WithMany(u => u.MessageTrackings)
            .HasForeignKey(mt => mt.UserId);
        modelBuilder.Entity<MessageTracking>()
            .HasOne(mt => mt.MessageDeliveryStatus)
            .WithMany(u => u.MessageTrackings)
            .HasForeignKey(mt => mt.MessageDeliveryStatusId);
        modelBuilder.Entity<MessageTracking>()
            .HasOne(mt => mt.SubscriptionType)
            .WithMany(u => u.MessageTrackings)
            .HasForeignKey(mt => mt.SubscriptionTypeId);
        
        modelBuilder.Entity<UserSubscription>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserSubscriptions)
            .HasForeignKey(ug => ug.UserId);
        modelBuilder.Entity<UserSubscription>()
            .HasOne(ug => ug.SubscriptionType)
            .WithMany(ng => ng.UserSubscriptions)
            .HasForeignKey(ug => ug.SubscriptionTypeId);
        
        // Начальные данные
        modelBuilder.Entity<SubscriptionType>().HasData(
            new SubscriptionType { Id = 1, Code = "TELEGRAM", Description = "Telegram" },
            new SubscriptionType { Id = 2, Code = "EMAIL", Description = "Email" }
        );

        modelBuilder.Entity<MessageDeliveryStatus>().HasData(
            new MessageDeliveryStatus { Id = 1, Code = "PENDING", Description = "Pending" },
            new MessageDeliveryStatus { Id = 2, Code = "SEND", Description = "Sent" },
            new MessageDeliveryStatus { Id = 3, Code = "FAIL", Description = "Failed" }
        );
    }
}