using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Models
{
    public class LedgerEntry : LedgerEntryBase
    {        
        public Invoice Invoice { get; set; }

        public InvoiceLineItem LineItem { get; set; }
    }    
}
