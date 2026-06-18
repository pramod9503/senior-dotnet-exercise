using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using SeniorDotnetExercise.Services;
using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Tests
{
    public class InvoiceServiceTests
    {
        private ExerciseDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ExerciseDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ExerciseDbContext(options);
        }

        private async Task SeedInvoiceAsync(ExerciseDbContext db)
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                Reference = "INV-001",
                CreatedAt = new DateTime(2026, 1, 1),
                LineItems = new[]
                {
                new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Care fees - week 1",
                    DueDate = new DateTime(2026, 1, 7),
                    Amount = 100.00m,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Care fees - week 2",
                    DueDate = new DateTime(2026, 1, 14),
                    Amount = 150.50m,
                    CreatedAt = new DateTime(2026, 1, 7)
                },
                new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Care fees - week 3",
                    DueDate = new DateTime(2026, 1, 21),
                    Amount = 200.25m,
                    CreatedAt = new DateTime(2026, 1, 14)
                }
            }
            };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task AllocatePayment_PartialPayment_AllocatesOldestFirst()
        {
            using var db = CreateDbContext();
            await SeedInvoiceAsync(db);
            var service = new InvoiceService(db);

            var invoice = db.Invoices.Include(i => i.LineItems).First();
            var result = await service.AllocatePayment(invoice.Id, 120.00m, DateTime.UtcNow);

            Assert.Equal(330.75m, result); // Outstanding: 450.75 - 120 = 330.75

            var allocations = db.LedgerEntries.Where(le => le.Type == LedgerEntryType.Allocation).ToList();
            Assert.Equal(2, allocations.Count);
            Assert.Equal(100.00m, allocations.First(a => a.LineItemId == invoice.LineItems.ElementAt(0).Id).Amount);
            Assert.Equal(20.00m, allocations.First(a => a.LineItemId == invoice.LineItems.ElementAt(1).Id).Amount);
        }

        [Fact]
        public async Task AllocatePayment_ExactPayment_ClearsInvoice()
        {
            using var db = CreateDbContext();
            await SeedInvoiceAsync(db);
            var service = new InvoiceService(db);

            var invoice = db.Invoices.Include(i => i.LineItems).First();
            var result = await service.AllocatePayment(invoice.Id, 450.75m, DateTime.UtcNow);

            Assert.Equal(0.00m, result);

            var allocations = db.LedgerEntries.Where(le => le.Type == LedgerEntryType.Allocation).ToList();
            Assert.Equal(3, allocations.Count);
            Assert.Equal(100.00m, allocations.First(a => a.LineItemId == invoice.LineItems.ElementAt(0).Id).Amount);
            Assert.Equal(150.50m, allocations.First(a => a.LineItemId == invoice.LineItems.ElementAt(1).Id).Amount);
            Assert.Equal(200.25m, allocations.First(a => a.LineItemId == invoice.LineItems.ElementAt(2).Id).Amount);
        }

        [Fact]
        public async Task AllocatePayment_OverPayment_RecordsCredit()
        {
            using var db = CreateDbContext();
            await SeedInvoiceAsync(db);
            var service = new InvoiceService(db);

            var invoice = db.Invoices.Include(i => i.LineItems).First();
            var result = await service.AllocatePayment(invoice.Id, 500.00m, DateTime.UtcNow);

            Assert.Equal(-49.25m, result);

            var allocations = db.LedgerEntries.Where(le => le.Type == LedgerEntryType.Allocation).ToList();
            Assert.Equal(3, allocations.Count);

            var credit = db.LedgerEntries.FirstOrDefault(le => le.Type == LedgerEntryType.Credit);
            Assert.NotNull(credit);
            Assert.Equal(49.25m, credit.Amount);
        }
    }
}