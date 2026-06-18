using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Models
{
    public class InvoiceLineItem : InvoiceLineItemBase
    {
        public Guid InvoiceId { get; set; }

        public Invoice Invoice { get; set; }

        public ICollection<LedgerEntry> LedgerEntries { get; set; }
    }
}
