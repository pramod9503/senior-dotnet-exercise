using Microsoft.EntityFrameworkCore;

namespace SeniorDotnetExercise.Models
{
    public class ExerciseDbContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }

        public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) : base(options)
        {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseNpgsql("Host=localhost;Database=your_db;Username=your_user;Password=your_password");
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Invoices table setup
            modelBuilder.Entity<Invoice>( 
                (entity) => {
                    entity.ToTable("Invoices");
                    entity.HasKey(x => x.Id).HasName("PK_InvoiceId");
                    entity.Property(x => x.Id).ValueGeneratedOnAdd();
                    entity.Property(x => x.Reference).IsRequired();

                    entity.HasMany(x => x.LineItems)
                    .WithOne(x => x.Invoice)
                    .HasForeignKey(x => x.InvoiceId)
                    .HasConstraintName("FK_Invoice_InvoiceLineItem")
                    .OnDelete(DeleteBehavior.Cascade);

                    entity.HasMany(x => x.LedgerEntries)
                    .WithOne(x => x.Invoice)
                    .HasForeignKey(x => x.InvoiceId)
                    .HasConstraintName("FK_Invoice_LedgerEntry")
                    .OnDelete(DeleteBehavior.Cascade);
                });

            // InvoiceLineItems table setup
            modelBuilder.Entity<InvoiceLineItem>(
                (entity) => {
                    entity.ToTable("InvoiceLineItems");
                    entity.HasKey(x => x.Id).HasName("PK_InvoiceLineItemId");
                    entity.Property(x => x.Id).ValueGeneratedOnAdd();
                    entity.Property(x => x.Description).IsRequired();
                    entity.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
                    
                    entity.HasMany(x => x.LedgerEntries)
                    .WithOne(x => x.LineItem)
                    .HasForeignKey(x => x.LineItemId)
                    .IsRequired()
                    .HasConstraintName("FK_InvoiceLineItem_LedgerEntry")
                    .OnDelete(DeleteBehavior.NoAction);
                });

            // LedgerEntries table setup
            modelBuilder.Entity<LedgerEntry>(
                (entity) => {
                    entity.ToTable("LedgerEntries");
                    entity.HasKey(x => x.Id).HasName("PK_LedgerEntryId");
                    entity.Property(x => x.Id).ValueGeneratedOnAdd();
                    entity.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
                    entity.Property(x => x.Type).HasConversion<string>().IsRequired();
                });                            
        }
    }
}
