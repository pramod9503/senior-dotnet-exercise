using Microsoft.EntityFrameworkCore;

namespace SeniorDotnetExercise.Models
{
    public static class SeedInvoices
    {
        public static void EnsurePopulated(IApplicationBuilder app) 
        {
            ExerciseDbContext context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ExerciseDbContext>();
            //if (context.Database.GetPendingMigrations().Any()) 
            //{
            //    context.Database.Migrate();
            //}

            if (!context.Invoices.Any()) 
            {
                context.Invoices.AddRange( 
                        new Invoice { 
                        Reference = "INV-001", CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), 
                        LineItems = new List<InvoiceLineItem>() 
                        { 
                            new InvoiceLineItem { Description = "Care fees - week 1", Amount = 100.00m, CreatedAt= DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 7), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 2", Amount = 150.50m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 7), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 14), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 3", Amount = 200.25m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 14), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 21), DateTimeKind.Utc) }
                        }},

                        new Invoice
                        {
                            Reference = "INV-002",
                            CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 2), DateTimeKind.Utc),
                            LineItems = new List<InvoiceLineItem>()
                            {
                            new InvoiceLineItem { Description = "Care fees - week 1", Amount = 225.00m, CreatedAt= DateTime.SpecifyKind(new DateTime(2026, 2, 2), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 2, 8), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 2", Amount = 300.25m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 8), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 2, 15), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 3", Amount = 250.75m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 15), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 2, 21), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 4", Amount = 275.00m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 21), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 2, 27), DateTimeKind.Utc) }
                            }
                        });

                context.SaveChanges();
            }
        }
    }
}
