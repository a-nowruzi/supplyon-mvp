using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseRequirement> PurchaseRequirements => Set<PurchaseRequirement>();
    public DbSet<SupplierOffer> SupplierOffers => Set<SupplierOffer>();
    public DbSet<UserFeedback> UserFeedbacks => Set<UserFeedback>();
    public DbSet<EvidenceRecord> EvidenceRecords => Set<EvidenceRecord>();
    public DbSet<ProcurementOrder> ProcurementOrders => Set<ProcurementOrder>();
    public DbSet<SupplierPerformanceEvaluation> SupplierPerformanceEvaluations => Set<SupplierPerformanceEvaluation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<CatalogItem>().HasIndex(x => x.ItemCode).IsUnique();
        builder.Entity<PurchaseRequirement>().HasOne(x => x.Item).WithMany().HasForeignKey(x => x.CatalogItemId);
        builder.Entity<PurchaseRequirement>().Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Entity<SupplierOffer>().HasOne(x => x.Requirement).WithMany().HasForeignKey(x => x.PurchaseRequirementId);
        builder.Entity<SupplierOffer>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
        builder.Entity<SupplierOffer>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Entity<EvidenceRecord>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<EvidenceRecord>().HasOne(x => x.PurchaseRequirement).WithMany().HasForeignKey(x => x.PurchaseRequirementId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<EvidenceRecord>().HasIndex(x => x.Status);
        builder.Entity<EvidenceRecord>().HasIndex(x => x.CreatedAt);
        builder.Entity<ProcurementOrder>().HasIndex(x => x.OrderCode).IsUnique();
        builder.Entity<ProcurementOrder>().HasIndex(x => x.SupplierOfferId).IsUnique();
        builder.Entity<ProcurementOrder>().Property(x => x.OrderedQuantity).HasPrecision(18, 3);
        builder.Entity<ProcurementOrder>().Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
        builder.Entity<ProcurementOrder>().Property(x => x.AcceptedQuantity).HasPrecision(18, 3);
        builder.Entity<ProcurementOrder>().Property(x => x.RejectedQuantity).HasPrecision(18, 3);
        builder.Entity<ProcurementOrder>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Entity<ProcurementOrder>().HasOne(x => x.Requirement).WithMany().HasForeignKey(x => x.PurchaseRequirementId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ProcurementOrder>().HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.SupplierOfferId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ProcurementOrder>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<SupplierPerformanceEvaluation>().HasIndex(x => x.ProcurementOrderId).IsUnique();
        builder.Entity<SupplierPerformanceEvaluation>().HasOne(x => x.Order).WithOne(x => x.Evaluation).HasForeignKey<SupplierPerformanceEvaluation>(x => x.ProcurementOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SupplierPerformanceEvaluation>().Property(x => x.OverallScore).HasPrecision(5, 2);
        builder.Entity<SupplierPerformanceEvaluation>().Property(x => x.FulfillmentRate).HasPrecision(6, 2);
    }
}
