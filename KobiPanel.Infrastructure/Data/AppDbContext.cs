using Microsoft.EntityFrameworkCore;
using KobiPanel.Core.Entities;

namespace KobiPanel.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== GLOBAL FILTER: Soft delete =====
        // IsDeleted = true olan kayıtlar otomatik filtrelenir
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Business>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Transaction>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<TransactionCategory>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<InvoiceItem>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(x => !x.IsDeleted);

        // ===== USER =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.Role).HasMaxLength(50);
        });

        // ===== BUSINESS =====
        modelBuilder.Entity<Business>(entity =>
        {
            entity.Property(b => b.Name).HasMaxLength(200);
            entity.Property(b => b.BusinessType).HasMaxLength(100);
            entity.Property(b => b.TaxNumber).HasMaxLength(50);

            entity.HasOne(b => b.Owner)
                  .WithMany(u => u.Businesses)
                  .HasForeignKey(b => b.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== CUSTOMER =====
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.FirstName).HasMaxLength(100);
            entity.Property(c => c.LastName).HasMaxLength(100);
            entity.Property(c => c.TotalSpent).HasPrecision(18, 2);

            entity.HasOne(c => c.Business)
                  .WithMany(b => b.Customers)
                  .HasForeignKey(c => c.BusinessId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== TRANSACTION =====
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Type).HasMaxLength(50);
            entity.Property(t => t.PaymentMethod).HasMaxLength(50);

            entity.HasOne(t => t.Business)
                  .WithMany(b => b.Transactions)
                  .HasForeignKey(t => t.BusinessId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Customer)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== TRANSACTION CATEGORY =====
        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.Property(tc => tc.Name).HasMaxLength(100);
            entity.Property(tc => tc.Type).HasMaxLength(50);

            entity.HasOne(tc => tc.Business)
                  .WithMany(b => b.TransactionCategories)
                  .HasForeignKey(tc => tc.BusinessId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== INVOICE =====
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(i => i.InvoiceNumber).HasMaxLength(50);
            entity.Property(i => i.SubTotal).HasPrecision(18, 2);
            entity.Property(i => i.TaxRate).HasPrecision(5, 2);
            entity.Property(i => i.TaxAmount).HasPrecision(18, 2);
            entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
            entity.Property(i => i.Status).HasMaxLength(50);

            entity.HasOne(i => i.Business)
                  .WithMany(b => b.Invoices)
                  .HasForeignKey(i => i.BusinessId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Customer)
                  .WithMany(c => c.Invoices)
                  .HasForeignKey(i => i.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== INVOICE ITEM =====
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.Property(ii => ii.UnitPrice).HasPrecision(18, 2);
            entity.Property(ii => ii.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(ii => ii.Invoice)
                  .WithMany(i => i.Items)
                  .HasForeignKey(ii => ii.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== APPOINTMENT =====
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Title).HasMaxLength(200);
            entity.Property(a => a.Status).HasMaxLength(50);

            entity.HasOne(a => a.Business)
                  .WithMany(b => b.Appointments)
                  .HasForeignKey(a => a.BusinessId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Customer)
                  .WithMany(c => c.Appointments)
                  .HasForeignKey(a => a.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== REFRESH TOKEN =====
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // SaveChanges'da otomatik tarih güncelleme
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}