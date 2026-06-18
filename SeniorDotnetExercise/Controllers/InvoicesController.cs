using System.Data.Common;
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
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]        
        public async Task<ActionResult<IEnumerable<InvoiceListDto>>> GetInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetInvoices();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices.");                
                return StatusCode(500, new {error = "An error occurred while retrieving invoices." });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]        
        public async Task<ActionResult<InvoiceDto>> GetInvoice([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest(new { error = "Invoice ID is required." });
            try
            {
                var invoice = await _invoiceService.GetInvoice(id);
                if (invoice == null) return NotFound(new { error = $"Invoice with ID {id} not found." });
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving invoice with ID {id}.", id);
                return StatusCode(500, new {error = "An error occurred while retrieving the invoice." });
            }
        }

        [HttpPost("{invoiceId}/allocate-payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> AllocatePayment(Guid invoiceId, [FromBody]AllocatePaymentRequest request) 
        {
            if (invoiceId == Guid.Empty) return BadRequest(new { error = "Invoice ID is required." });            

            try
            {
                var outstanding = await _invoiceService.AllocatePayment(invoiceId, request.PaymentAmount, request.ReceivedAt);
                return Ok(new { OutstandingBalance = outstanding });
            }
            catch (InvalidOperationException ex)
            {
                // Invoice not found
                return NotFound(new { error = ex.Message });
            }
            catch (DbException ex) 
            {
                _logger.LogError(ex, $"Database error allocating payment for invoice ID {invoiceId}.", invoiceId);
                return StatusCode(503, new { error = "A database error occurred while allocating the payment. Please try again later." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error allocating payment for invoice ID {invoiceId}.", invoiceId);
                return StatusCode(500, new { error = "An error occurred while allocating the payment." });
            }
        }
    }
}
