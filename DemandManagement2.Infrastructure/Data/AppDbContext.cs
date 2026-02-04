using DemandManagement2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DemandRequest> DemandRequests => Set<DemandRequest>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();

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

        base.OnModelCreating(modelBuilder);
    }
}