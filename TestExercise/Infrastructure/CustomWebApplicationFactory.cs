using SeniorDotnetExercise;
using Microsoft.AspNetCore.Hosting;
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
namespace TestExercise.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Removed the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ExerciseDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // DbContext using InMemory provider
                services.AddDbContext<ExerciseDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build the service provider and seed data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ExerciseDbContext>();
                db.Database.EnsureCreated();

                // Seed test data 
                db.Invoices.Add(new Invoice
                {
                    //Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), //Guid.NewGuid(),
                    Reference = "INV-002",
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
                    LineItems = new List<InvoiceLineItem>()
                        {
                            new InvoiceLineItem { Description = "Care fees - week 1", Amount = 100.00m, CreatedAt= DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 7), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 2", Amount = 150.50m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 7), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 14), DateTimeKind.Utc) },
                            new InvoiceLineItem { Description = "Care fees - week 3", Amount = 200.25m, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 14), DateTimeKind.Utc), DueDate = DateTime.SpecifyKind(new DateTime(2026, 1, 21), DateTimeKind.Utc) }
                        }
                });
                db.SaveChanges();
            });
        }
    }
}
