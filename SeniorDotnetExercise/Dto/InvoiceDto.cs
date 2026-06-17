using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Dto
{
    public class InvoiceDto : InvoiceBase
    {
        public IEnumerable<InvoiceLineItemDto>? LineItems { get; set; }

        public IEnumerable<LedgerEntryDto>? LedgerEntries { get; set; }
    }
}
