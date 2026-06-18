using SeniorDotnetExercise.Dto;
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ExerciseDbContext _excerciseContext;

        public InvoiceService(ExerciseDbContext excerciseContext)
        {
            _excerciseContext = excerciseContext;
        }

        public async Task<InvoiceDto?> GetInvoice(Guid invoiceId)
        {
            InvoiceDto? invoice = await _excerciseContext.Invoices
            .Where(inv => inv.Id == invoiceId)
            .Select(inv => new InvoiceDto
            {
                Id = inv.Id,
                Reference = inv.Reference,
                CreatedAt = inv.CreatedAt,
                LineItems = inv.LineItems.Select(li => new InvoiceLineItemDto
                {
                    Id = li.Id,
                    Description = li.Description,
                    Amount = li.Amount,
                    CreatedAt = li.CreatedAt,
                    DueDate = li.DueDate
                }).ToList(),
                LedgerEntries = inv.LedgerEntries.Select(le => new LedgerEntryDto
                {
                    Id = le.Id,
                    Type = le.Type,
                    Amount = le.Amount,
                    CreatedAt = le.CreatedAt,
                    LineItemId = le.LineItemId
                }).OrderByDescending(x => x.Type).ToList()
            }).SingleOrDefaultAsync();

            return invoice;
        }

        public async Task<IEnumerable<InvoiceListDto>> GetInvoices()
        {
            var invoices = await _excerciseContext.Invoices
                        .Select(x => new InvoiceListDto
                        {
                            Id = x.Id,
                            Reference = x.Reference,
                            CreatedAt = x.CreatedAt,
                            LineItemCount = x.LineItems.Count(),
                            LedgerCount = x.LedgerEntries.Count(),
                            TotalAmount = x.LineItems.Sum(li => li.Amount),
                            TotalPayment = x.LedgerEntries.Sum(le => le.Amount)
                        }).ToListAsync();
            return invoices;
        }

        public async Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt)
        {
            // Check if the invoice exists
            var invoiceExists = await _excerciseContext.Invoices.AnyAsync(i => i.Id == invoiceId);
            if (!invoiceExists)
            {
                throw new InvalidOperationException($"Invoice with ID {invoiceId} does not exist.");
            }

            // 1. Record the payment received in the ledger (unallocated at this point)
            var paymentEntry = new LedgerEntry
            {
                //Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                LineItemId = null,
                Type = LedgerEntryType.PaymentReceived,
                Amount = paymentAmount,
                CreatedAt = receivedAt
            };
            _excerciseContext.LedgerEntries.Add(paymentEntry);

            // 2. Get all line items for the invoice, ordered oldest first (DueDate, then CreatedAt)
            var lineItems = await _excerciseContext.InvoiceLineItems
                .Where(li => li.InvoiceId == invoiceId)
                .OrderBy(li => li.DueDate)
                .ThenBy(li => li.CreatedAt)
                .ToListAsync();

            // 3. Get allocations so far for each line item
            var allocations = await _excerciseContext.LedgerEntries
                .Where(le => le.InvoiceId == invoiceId && le.Type == LedgerEntryType.Allocation && le.LineItemId != null)
                .GroupBy(le => le.LineItemId)
                .Select(g => new { LineItemId = g.Key, Allocated = g.Sum(le => le.Amount) })
                .ToDictionaryAsync(x => x.LineItemId!.Value, x => x.Allocated);

            decimal remaining = paymentAmount;

            foreach (var lineItem in lineItems)
            {
                allocations.TryGetValue(lineItem.Id, out var alreadyAllocated);
                var outstanding = lineItem.Amount - alreadyAllocated;

                if (outstanding <= 0) continue;

                var toAllocate = Math.Min(outstanding, remaining);
                if (toAllocate <= 0) break;

                var allocationEntry = new LedgerEntry
                {
                    //Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    LineItemId = lineItem.Id,
                    Type = LedgerEntryType.Allocation,
                    Amount = toAllocate,
                    CreatedAt = receivedAt
                };
                _excerciseContext.LedgerEntries.Add(allocationEntry);

                remaining -= toAllocate;
                if (remaining <= 0) break;
            }

            // 4. If there is leftover (overpayment), record as Credit
            if (remaining > 0)
            {
                var creditEntry = new LedgerEntry
                {
                    //Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    LineItemId = null,
                    Type = LedgerEntryType.Credit,
                    Amount = remaining,
                    CreatedAt = receivedAt
                };
                _excerciseContext.LedgerEntries.Add(creditEntry);
            }

            await _excerciseContext.SaveChangesAsync();

            // 5. Calculate new outstanding balance (total charged - total allocated)
            var totalCharged = lineItems.Sum(li => li.Amount);
            var totalAllocated = await _excerciseContext.LedgerEntries
                .Where(le => le.InvoiceId == invoiceId && le.Type == LedgerEntryType.Allocation)
                .SumAsync(le => le.Amount);

            var totalCredit = await _excerciseContext.LedgerEntries
                    .Where(le => le.InvoiceId == invoiceId && le.Type == LedgerEntryType.Credit)
                    .SumAsync(le => le.Amount);

            var outstandingBalance = totalCharged - totalAllocated - totalCredit;
            return outstandingBalance;
        }
    }
}