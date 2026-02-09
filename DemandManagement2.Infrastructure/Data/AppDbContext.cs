using DemandManagement2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DemandRequest> DemandRequests => Set<DemandRequest>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();
    public DbSet<DecisionNote> DecisionNotes => Set<DecisionNote>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DemandEvent> DemandEvents => Set<DemandEvent>();
    public DbSet<DemandAttachment> DemandAttachments => Set<DemandAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DemandRequest>()
            .HasOne(d => d.Assessment)
            .WithOne(a => a.DemandRequest)
            .HasForeignKey<Assessment>(a => a.DemandRequestId);

        modelBuilder.Entity<DemandRequest>()
            .HasOne(d => d.Approval)
            .WithOne(a => a.DemandRequest)
            .HasForeignKey<ApprovalDecision>(a => a.DemandRequestId);

        modelBuilder.Entity<DemandRequest>()
            .HasMany(d => d.DecisionNotes)
            .WithOne(n => n.DemandRequest)
            .HasForeignKey(n => n.DemandRequestId);

        modelBuilder.Entity<DemandRequest>()
            .HasMany(d => d.ResourceAllocations)
            .WithOne(a => a.DemandRequest)
            .HasForeignKey(a => a.DemandRequestId);

        modelBuilder.Entity<DemandRequest>()
            .HasMany(d => d.Events)
            .WithOne(e => e.DemandRequest)
            .HasForeignKey(e => e.DemandRequestId);

        modelBuilder.Entity<DemandRequest>()
            .HasMany(d => d.Attachments)
            .WithOne(a => a.DemandRequest)
            .HasForeignKey(a => a.DemandRequestId);

        modelBuilder.Entity<Resource>()
            .HasMany(r => r.Allocations)
            .WithOne(a => a.Resource)
            .HasForeignKey(a => a.ResourceId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        base.OnModelCreating(modelBuilder);
    }
}