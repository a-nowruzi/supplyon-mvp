using Microsoft.EntityFrameworkCore;
using O2Ban.ValidationWorkspace.Models;
namespace O2Ban.ValidationWorkspace.Data;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseRequirement> Requirements => Set<PurchaseRequirement>();
    public DbSet<Rfq> Rfqs => Set<Rfq>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Evidence> Evidence => Set<Evidence>();
    public DbSet<Feedback> Feedback => Set<Feedback>();
    public DbSet<LookupOption> LookupOptions => Set<LookupOption>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasCharSet("utf8mb4").UseCollation("utf8mb4_unicode_ci");
        b.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        b.Entity<LookupOption>().HasIndex(x => new { x.Group, x.Value }).IsUnique();
        b.Entity<LookupOption>().Property(x => x.Group).HasMaxLength(60).IsRequired();
        b.Entity<LookupOption>().Property(x => x.Value).HasMaxLength(120).IsRequired();
        b.Entity<Supplier>().Property(x => x.Name).HasMaxLength(180).IsRequired();
        b.Entity<Supplier>().Property(x => x.NationalId).HasMaxLength(11);
        b.Entity<Supplier>().HasIndex(x => x.NationalId).IsUnique();
        b.Entity<Supplier>().Property(x => x.Capabilities).HasMaxLength(2000).IsRequired();
        b.Entity<Supplier>().Property(x => x.City).HasMaxLength(100);
        b.Entity<Supplier>().Property(x => x.ContactName).HasMaxLength(120);
        b.Entity<Supplier>().Property(x => x.EvidenceStatus).HasMaxLength(60).IsRequired();
        b.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Rfq>().HasIndex(x => x.Code).IsUnique();
        b.Entity<PurchaseRequirement>().Property(x => x.Quantity).HasPrecision(18, 2);
        b.Entity<Offer>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Entity<Offer>().Property(x => x.Quantity).HasPrecision(18, 2);
        b.Entity<Company>().Property(x => x.Name).HasMaxLength(180).IsRequired();
    }
}
