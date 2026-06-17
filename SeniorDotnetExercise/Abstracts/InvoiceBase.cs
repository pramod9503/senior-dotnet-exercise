namespace SeniorDotnetExercise.Abstracts
{
    public class InvoiceBase
    {
        public Guid Id { get; set; }

        public string Reference { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
