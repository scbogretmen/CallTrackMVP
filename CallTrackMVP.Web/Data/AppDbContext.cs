using Microsoft.EntityFrameworkCore;
using CallTrackMVP.Web.Models;

namespace CallTrackMVP.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<CallLog> CallLogs => Set<CallLog>();
    public DbSet<CallLogUpdate> CallLogUpdates => Set<CallLogUpdate>();
    public DbSet<CallType> CallTypes => Set<CallType>();
    public DbSet<CallLogAcknowledgment> CallLogAcknowledgments => Set<CallLogAcknowledgment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.UserName).IsUnique();
        });

        modelBuilder.Entity<CallType>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<CallLog>(e =>
        {
            e.HasIndex(c => new { c.Tarih, c.CagriNo }).IsUnique();
            e.HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CallLogs)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(c => c.Updates)
                .WithOne(u => u.CallLog)
                .HasForeignKey(u => u.CallLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CallLogAcknowledgment>(e =>
        {
            e.HasKey(a => new { a.UserId, a.CallLogId });
            e.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.CallLog).WithMany().HasForeignKey(a => a.CallLogId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
