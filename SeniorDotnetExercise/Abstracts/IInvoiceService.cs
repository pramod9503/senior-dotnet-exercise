using SeniorDotnetExercise.Dto;

namespace SeniorDotnetExercise.Abstracts
{
    public interface IInvoiceService
    {
        public Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt);

        public Task<InvoiceDto?> GetInvoice(Guid invoiceId);

        Task<IEnumerable<InvoiceListDto>> GetInvoices();
    }
}
