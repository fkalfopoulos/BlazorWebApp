using System.Net.Http.Json;
using EpsilonWebApp.Contracts.DTOs;

namespace EpsilonWebApp.Services.CustomerService
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerDto>> GetCustomersAsync(int pageNumber = 1, int pageSize = 10);
        Task<CustomerDto?> GetCustomerAsync(Guid id);
        Task<CustomerDto> CreateCustomerAsync(CustomerDto customer);
        Task UpdateCustomerAsync(Guid id, CustomerDto customer);
        Task DeleteCustomerAsync(Guid id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;

        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<CustomerDto>> GetCustomersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var response = await _httpClient.GetFromJsonAsync<PagedResult<CustomerDto>>(
                $"api/customers?pageNumber={pageNumber}&pageSize={pageSize}");
            return response ?? new PagedResult<CustomerDto>();
        }

        public async Task<CustomerDto?> GetCustomerAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
        {
            var response = await _httpClient.PostAsJsonAsync("api/customers", customer);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerDto>() 
                ?? throw new Exception("Failed to create customer");
        }

        public async Task UpdateCustomerAsync(Guid id, CustomerDto customer)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/customers/{id}", customer);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCustomerAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/customers/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
