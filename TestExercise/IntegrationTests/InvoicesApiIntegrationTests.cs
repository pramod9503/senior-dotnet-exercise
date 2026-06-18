using Xunit;
using System.Net.Http.Json;
using SeniorDotnetExercise.Dto;
using TestExercise.Infrastructure;

namespace SeniorDotnetExercise.IntegrationTests
{
    public class InvoicesApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {        
        private readonly CustomWebApplicationFactory _factory;

        public InvoicesApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }        

        [Fact]
        public async Task GetInvoices_ReturnsOkAndInvoices()
        {
            var client = _factory.CreateClient();            

            var response = await client.GetAsync("/api/v1/invoices");
            response.EnsureSuccessStatusCode();

            var invoices = await response.Content.ReadFromJsonAsync<InvoiceListDto[]>();
            Assert.NotNull(invoices);            
        }

        [Fact]
        public async Task AllocatePayment_ReturnsOutstandingBalance()
        {
            var client = _factory.CreateClient();

            // Get ID of seeded invoice
             var invoiceId = await SeedInvoiceAndGetIdAsync(client);
            
            var request = new AllocatePaymentRequest
            {
                PaymentAmount = 100.00m,
                ReceivedAt = DateTime.UtcNow
            };

            var response = await client.PostAsJsonAsync($"/api/v1/invoices/{invoiceId}/allocate-payment", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);            
        }

        private async Task<Guid> SeedInvoiceAndGetIdAsync(HttpClient client)
        {
            // Retrieve the invoice (e.g., by unique property)
            var invoices = await client.GetFromJsonAsync<InvoiceListDto[]>("/api/v1/invoices");
            var invoice = invoices.First(i => i.Reference == "INV-002");
            return invoice.Id;
        }
    }
}