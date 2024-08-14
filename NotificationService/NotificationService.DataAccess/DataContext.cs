using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.DataAccess;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<NotificationGroup> NotificationGroups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<MessageTracking> MessageTrackings { get; set; }
    
    public DbSet<Priority> Priorities { get; set; }

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

        modelBuilder.Entity<NotificationGroup>()
            .HasKey(ng => ng.Id);

        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        modelBuilder.Entity<MessageTracking>()
            .HasKey(mt => mt.MessageId);
        
        modelBuilder.Entity<SubscriptionType>()
            .HasKey(st => st.Id);
        
        modelBuilder.Entity<Priority>()
            .HasKey(p => p.Id);
        
        modelBuilder.Entity<UserSubscription>()
            .HasKey(us => new { us.UserId, us.SubscriptionTypeId });
        
        modelBuilder.Entity<MessageDeliveryStatus>()
            .HasKey(mds => mds.Id);

        // Определение внешних ключей и связей между таблицами
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.NotificationGroup)
            .WithMany(ng => ng.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        modelBuilder.Entity<MessageTracking>()
            .HasOne(mt => mt.User)
            .WithMany(u => u.MessageTrackings)
            .HasForeignKey(mt => mt.UserId);
        modelBuilder.Entity<MessageTracking>()
            .HasOne(mt => mt.Priority)
            .WithMany(u => u.MessageTrackings)
            .HasForeignKey(mt => mt.PriorityId);
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
    }
}