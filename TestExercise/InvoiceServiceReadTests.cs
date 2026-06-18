using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using SeniorDotnetExercise.Dto;
using System.Collections.Generic;
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using SeniorDotnetExercise.Services;

namespace TestExercise
{
    public class InvoiceServiceReadTests
    {
        private ExerciseDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ExerciseDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ExerciseDbContext(options);
        }

        private async Task<Guid> SeedInvoiceAsync(ExerciseDbContext db)
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                Reference = "INV-001",
                CreatedAt = new DateTime(2026, 1, 1),
                LineItems = new List<InvoiceLineItem>
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
                }
            }
            };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
            return invoice.Id;
        }

        [Fact]
        public async Task GetInvoices_ReturnsAllInvoicesWithTotals()
        {
            using var db = CreateDbContext();
            var invoiceId = await SeedInvoiceAsync(db);
            var service = new InvoiceService(db);

            var result = await service.GetInvoices();
            var list = result.ToList();

            Assert.Single(list);
            var dto = list[0];
            Assert.Equal("INV-001", dto.Reference);
            Assert.Equal(2, dto.LineItemCount); // If you map this property in your projection
            Assert.Equal(250.50m, dto.TotalAmount);
        }

        [Fact]
        public async Task GetInvoice_ReturnsInvoiceWithLineItemsAndLedgerEntries()
        {
            using var db = CreateDbContext();
            var invoiceId = await SeedInvoiceAsync(db);

            // Add a ledger entry for testing
            var lineItem = db.InvoiceLineItems.First();
            db.LedgerEntries.Add(new LedgerEntry
            {
                InvoiceId = invoiceId,
                LineItemId = lineItem.Id,
                Type = LedgerEntryType.Allocation,
                Amount = 50.00m,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);
            var dto = await service.GetInvoice(invoiceId);

            Assert.NotNull(dto);
            Assert.Equal("INV-001", dto.Reference);
            Assert.Equal(2, dto.LineItems.Count());
            Assert.Single(dto.LedgerEntries);
            Assert.Equal(50.00m, dto.LedgerEntries.First().Amount);
        }

        [Fact]
        public async Task GetInvoice_ReturnsNullIfNotFound()
        {
            using var db = CreateDbContext();
            var service = new InvoiceService(db);

            var dto = await service.GetInvoice(Guid.NewGuid());
            Assert.Null(dto);
        }
    }
}
