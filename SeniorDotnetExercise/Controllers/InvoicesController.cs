using Microsoft.AspNetCore.Mvc;
using SeniorDotnetExercise.Dto;
using SeniorDotnetExercise.Abstracts;

namespace SeniorDotnetExercise.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _invoiceService.GetInvoices();
            return Ok(invoices);

        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInvoice(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invoice ID is required.");
            var invoice = await _invoiceService.GetInvoice(id);
            if (invoice == null) return NotFound($"Invoice with ID {id} not found.");
            return Ok(invoice);
        }

        [HttpPost("{invoiceId}/allocate-payment")]
        public async Task<IActionResult> AllocatePayment(Guid invoiceId, [FromBody]AllocatePaymentRequest request) 
        {
            if (invoiceId == Guid.Empty) return BadRequest("Invoice ID is required.");
            if (request.PaymentAmount <= 0) return BadRequest("Payment amount must be greater than zero.");
            if (request.ReceivedAt == default) return BadRequest("Received date is required.");

            try
            {
                var outstanding = await _invoiceService.AllocatePayment(invoiceId, request.PaymentAmount, request.ReceivedAt);
                return Ok(new { OutstandingBalance = outstanding });
            }
            catch (InvalidOperationException ex)
            {
                // Invoice not found
                return NotFound(ex.Message);
            }            
        }
    }
}
