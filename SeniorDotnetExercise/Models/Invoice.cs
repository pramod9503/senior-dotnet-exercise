using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Models
{
    public class Invoice : InvoiceBase
    {
        //public Guid Id { get; set; }

        //public string Reference { get; set; } = string.Empty;

        //public DateTime CreatedAt { get; set; }

        public ICollection<InvoiceLineItem> LineItems { get; set; }

        public ICollection<LedgerEntry> LedgerEntries { get; set; }
    }           
}
