using Microsoft.AspNetCore.Mvc;
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
    }
}
