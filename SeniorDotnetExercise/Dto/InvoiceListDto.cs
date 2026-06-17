using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Dto
{
    public class InvoiceListDto : InvoiceBase
    {
        public int LineItemCount { get; set; }

        public int LedgerCount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalPayment { get; set; }

        public decimal TotalOutstaing  => TotalAmount - TotalPayment;
    }
}
