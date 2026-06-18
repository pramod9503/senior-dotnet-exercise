namespace SeniorDotnetExercise.Abstracts
{
    public class InvoiceLineItemBase
    {
        public Guid Id { get; set; }        

        public string Description { get; set; } = String.Empty;

        public DateTime DueDate { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
