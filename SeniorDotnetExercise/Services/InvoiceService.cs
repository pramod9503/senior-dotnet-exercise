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
        var query = from inv in _excerciseContext.Invoices
                    where inv.Id == invoiceId
                    join invList in _excerciseContext.InvoiceLineItems
                        on inv.Id equals invList.InvoiceId into lineItemsGroup
                    from li in lineItemsGroup.DefaultIfEmpty()
                    join led in _excerciseContext.LedgerEntries
                        on inv.Id equals led.InvoiceId into ledgerEntriesGroup
                    from le in ledgerEntriesGroup.DefaultIfEmpty()
                    select new { inv, li, le };

        var result = await query.ToListAsync();

        // Group and project to DTO
        InvoiceDto? invoice = result
            .GroupBy(x => x.inv)
            .Select(g => new InvoiceDto
            {
                Id = g.Key.Id,
                Reference = g.Key.Reference,
                CreatedAt = g.Key.CreatedAt,
                LineItems = g.Where(x => x.li != null).Select(x => new InvoiceLineItemDto
                {
                    Id = x.li.Id,
                    Description = x.li.Description,
                    Amount = x.li.Amount,
                    CreatedAt = x.li.CreatedAt,
                    DueDate = x.li.DueDate
                }).Distinct().ToList(),
                LedgerEntries = g.Where(x => x.le != null).Select(x => new LedgerEntryDto
                {
                    Id = x.le.Id,
                    Type = x.le.Type,
                    Amount = x.le.Amount,
                    CreatedAt = x.le.CreatedAt,
                    LineItemId = x.le.LineItemId
                }).Distinct().ToList()
            }).SingleOrDefault();        
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
                        TotalAmount = x.LineItems.Sum(li => li.Amount),
                        TotalPayment = x.LedgerEntries.Sum(le => le.Amount)
                    }).ToListAsync();
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
