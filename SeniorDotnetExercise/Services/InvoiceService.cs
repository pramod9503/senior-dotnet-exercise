using SeniorDotnetExercise.Dto;
using SeniorDotnetExercise.Models;
using Microsoft.EntityFrameworkCore;
using SeniorDotnetExercise.Abstracts;

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
            .Where(x => x.Id == invoiceId)
            .Select(x => new InvoiceDto
            {
                Id = x.Id,
                Reference = x.Reference,
                CreatedAt = x.CreatedAt,
                LineItems = x.LineItems.Select(li => new InvoiceLineItemDto
                {
                    Id = li.Id,
                    Description = li.Description,
                    Amount = li.Amount,
                    CreatedAt = li.CreatedAt,
                    DueDate = li.DueDate
                }).ToList(),

                LedgerEntries = x.LedgerEntries.Select(le => new LedgerEntryDto
                {
                    Id = le.Id,
                    Type = le.Type,
                    Amount = le.Amount,
                    CreatedAt = le.CreatedAt,
                    LineItemId = le.LineItemId
                }).ToList()
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
                TotalAmount = x.LineItems.Where(y => y.InvoiceId == x.Id).Sum(li => li.Amount),
                TotalPayment = x.LedgerEntries.Where(le => le.InvoiceId == x.Id).Sum(le => le.Amount)
            })            
            .ToListAsync();
        return invoices;
    }

    public async Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt)
    {
        // 1. Record the payment received in the ledger
        var paymentEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            //LineItemId = null,
            Type = LedgerEntryType.PaymentReceived,
            Amount = paymentAmount,
            CreatedAt = receivedAt
        };
        _excerciseContext.LedgerEntries.Add(paymentEntry);

        // 2. Get all line items for the invoice, ordered oldest first
        var lineItems = await _excerciseContext.InvoiceLineItems
            .Where(li => li.InvoiceId == invoiceId)
            .OrderBy(li => li.DueDate)
            .ToListAsync();
            

        // 3. Get allocations so far for each line item
        var allocations = await _excerciseContext.LedgerEntries
            .Where(le => le.InvoiceId == invoiceId && le.Type == LedgerEntryType.Allocation && le.LineItemId != null)
            .GroupBy(le => le.LineItemId)
            .Select(g => new { LineItemId = g.Key, Allocated = g.Sum(le => le.Amount) })
            .ToDictionaryAsync(x => x.LineItemId!, x => x.Allocated);

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
                Id = Guid.NewGuid(),
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

        await _excerciseContext.SaveChangesAsync();

        // 4. Calculate new outstanding balance
        var totalCharged = lineItems.Sum(li => li.Amount);
        var totalAllocated = await _excerciseContext.LedgerEntries
            .Where(le => le.InvoiceId == invoiceId && le.Type == LedgerEntryType.Allocation)
            .SumAsync(le => le.Amount);

        var outstandingBalance = totalCharged - totalAllocated;
        return outstandingBalance;
    }
}
