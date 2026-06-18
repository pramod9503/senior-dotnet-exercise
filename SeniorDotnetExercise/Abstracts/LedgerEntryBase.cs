using SeniorDotnetExercise.Models;

namespace SeniorDotnetExercise.Abstracts
{
    public class LedgerEntryBase
    {
        public Guid Id { get; set; }

        public Guid InvoiceId { get; set; }

        public Guid? LineItemId { get; set; }

        public LedgerEntryType Type { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
