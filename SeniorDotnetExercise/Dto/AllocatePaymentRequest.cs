using System.ComponentModel.DataAnnotations;

namespace SeniorDotnetExercise.Dto
{
    public class AllocatePaymentRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal PaymentAmount { get; set; }

        [Required(ErrorMessage = "Payment received date-time required.")]
        public DateTime ReceivedAt { get; set; }
    }
}
