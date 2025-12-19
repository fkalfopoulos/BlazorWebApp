using EpsilonWebApp.Contracts.DTOs;
using EpsilonWebApp.Services.CustomerService;
using FluentAssertions;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EpsilonWebApp.Services.Tests.CustomerService
{
    public class CustomerServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _httpClient;
        private readonly ICustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _httpClient = _mockHttp.ToHttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost");
            _customerService = new EpsilonWebApp.Services.CustomerService.CustomerService(_httpClient);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldReturnPagedResult()
        {
            // Arrange
            var expectedResult = new PagedResult<CustomerDto>
            {
                Items = new List<CustomerDto>
                {
                    new CustomerDto { Id = Guid.NewGuid(), CompanyName = "Company1" },
                    new CustomerDto { Id = Guid.NewGuid(), CompanyName = "Company2" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockHttp.When("http://localhost/api/customers?pageNumber=1&pageSize=10")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedResult));

            var result = await _customerService.GetCustomersAsync(1, 10);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldReturnEmptyResult_WhenApiReturnsNull()
        {
            // Arrange
            _mockHttp.When("http://localhost/api/customers?pageNumber=1&pageSize=10")
                .Respond(HttpStatusCode.OK, "application/json", "null");

             var result = await _customerService.GetCustomersAsync(1, 10);

             result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 20)]
        [InlineData(5, 5)]
        public async Task GetCustomersAsync_ShouldUseCorrectQueryParameters(int pageNumber, int pageSize)
        {
            var expectedUrl = $"http://localhost/api/customers?pageNumber={pageNumber}&pageSize={pageSize}";
            _mockHttp.When(expectedUrl)
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new PagedResult<CustomerDto>()));

            await _customerService.GetCustomersAsync(pageNumber, pageSize);

            _mockHttp.GetMatchCount(_mockHttp.When(expectedUrl)).Should().Be(1);
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnCustomer_WhenFound()
        {
            var customerId = Guid.NewGuid();
            var expectedCustomer = new CustomerDto
            {
                Id = customerId,
                CompanyName = "Test Company",
                ContactName = "John Doe"
            };

            _mockHttp.When($"http://localhost/api/customers/{customerId}")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedCustomer));

            var result = await _customerService.GetCustomerAsync(customerId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(customerId);
            result.CompanyName.Should().Be("Test Company");
            result.ContactName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnNull_WhenNotFound()
        {
            var customerId = Guid.NewGuid();
            _mockHttp.When($"http://localhost/api/customers/{customerId}")
                .Respond(HttpStatusCode.NotFound);

            var result = await _customerService.GetCustomerAsync(customerId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnCreatedCustomer()
        {
            var newCustomer = new CustomerDto
            {
                CompanyName = "New Company",
                ContactName = "Jane Smith"
            };

            var createdCustomer = new CustomerDto
            {
                Id = Guid.NewGuid(),
                CompanyName = "New Company",
                ContactName = "Jane Smith"
            };

            _mockHttp.When(HttpMethod.Post, "http://localhost/api/customers")
                .Respond(HttpStatusCode.Created, "application/json", System.Text.Json.JsonSerializer.Serialize(createdCustomer));

            var result = await _customerService.CreateCustomerAsync(newCustomer);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.CompanyName.Should().Be("New Company");
            result.ContactName.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrowException_WhenCreationFails()
        {
            var newCustomer = new CustomerDto { CompanyName = "Test" };

            _mockHttp.When(HttpMethod.Post, "http://localhost/api/customers")
                .Respond(HttpStatusCode.BadRequest);

            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _customerService.CreateCustomerAsync(newCustomer));
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrowException_WhenResponseIsNull()
        {
            var newCustomer = new CustomerDto { CompanyName = "Test" };

            _mockHttp.When(HttpMethod.Post, "http://localhost/api/customers")
                .Respond(HttpStatusCode.Created, "application/json", "null");

            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _customerService.CreateCustomerAsync(newCustomer));
            
            exception.Message.Should().Be("Failed to create customer");
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldSucceed()
        {
            var customerId = Guid.NewGuid();
            var updatedCustomer = new CustomerDto
            {
                Id = customerId,
                CompanyName = "Updated Company"
            };

            _mockHttp.When(HttpMethod.Put, $"http://localhost/api/customers/{customerId}")
                .Respond(HttpStatusCode.NoContent);

            Func<Task> act = async () => await _customerService.UpdateCustomerAsync(customerId, updatedCustomer);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldThrowException_WhenUpdateFails()
        {
            var customerId = Guid.NewGuid();
            var updatedCustomer = new CustomerDto { Id = customerId };

            _mockHttp.When(HttpMethod.Put, $"http://localhost/api/customers/{customerId}")
                .Respond(HttpStatusCode.NotFound);

            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _customerService.UpdateCustomerAsync(customerId, updatedCustomer));
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldSucceed()
        {
            var customerId = Guid.NewGuid();

            _mockHttp.When(HttpMethod.Delete, $"http://localhost/api/customers/{customerId}")
                .Respond(HttpStatusCode.NoContent);

            Func<Task> act = async () => await _customerService.DeleteCustomerAsync(customerId);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldThrowException_WhenDeleteFails()
        {
            var customerId = Guid.NewGuid();

            _mockHttp.When(HttpMethod.Delete, $"http://localhost/api/customers/{customerId}")
                .Respond(HttpStatusCode.NotFound);

            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _customerService.DeleteCustomerAsync(customerId));
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldUseDefaultParameters()
        {
            _mockHttp.When("http://localhost/api/customers?pageNumber=1&pageSize=10")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new PagedResult<CustomerDto>()));

            await _customerService.GetCustomersAsync();

            _mockHttp.GetMatchCount(_mockHttp.When("http://localhost/api/customers?pageNumber=1&pageSize=10")).Should().Be(1);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldHandleApiError()
        {
            _mockHttp.When("http://localhost/api/customers?pageNumber=1&pageSize=10")
                .Respond(HttpStatusCode.InternalServerError);

            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _customerService.GetCustomersAsync(1, 10));
        }
    }
}
