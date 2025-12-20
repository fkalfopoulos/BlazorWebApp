using EpsilonWebApp.Infrastructure.Repositories.EFCore.Configurations;
using EpsilonWebApp.Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpsilonWebApp.Controllers
{
    /// <summary>
    /// API controller for managing customer operations
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        /// <summary>
        /// Initializes a new instance of the CustomersController
        /// </summary>
        /// <param name="context">Database context for customer operations</param>
        /// <param name="logger">Logger instance for logging operations and errors</param>
        public CustomersController(CustomersDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of customers
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (default: 1)</param>
        /// <param name="pageSize">The number of items per page (default: 10)</param>
        /// <returns>A paginated result containing customers and pagination metadata</returns>
        /// <response code="200">Returns the paginated list of customers</response>
        /// <response code="500">An error occurred while retrieving customers</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<Customer>>> GetCustomers(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting customers - Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

                var totalCount = await _context.Customers.CountAsync();
                var customers = await _context.Customers
                    .OrderBy(c => c.CompanyName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} customers out of {Total}", customers.Count, totalCount);

                return new PagedResult<Customer>
                {
                    Items = customers,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex) when (ex is not DbUpdateException && ex is not DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "Error occurred while getting customers - Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                return StatusCode(500, "An error occurred while retrieving customers");
            }
        }

        /// <summary>
        /// Retrieves a specific customer by ID
        /// </summary>
        /// <param name="id">The unique identifier of the customer</param>
        /// <returns>The customer with the specified ID</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">An error occurred while retrieving the customer</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> GetCustomer(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved customer: {CompanyName}", customer.CompanyName);
                return customer;
            }
            catch (Exception ex) when (ex is not DbUpdateException && ex is not DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "Error occurred while getting customer with ID: {CustomerId}", id);
                return StatusCode(500, "An error occurred while retrieving the customer");
            }
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <param name="customer">The customer object to create</param>
        /// <returns>The newly created customer</returns>
        /// <response code="201">Customer created successfully</response>
        /// <response code="500">An error occurred while creating the customer</response>
        [HttpPost]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                _logger.LogInformation("Creating new customer: {CompanyName}", customer.CompanyName);

                customer.Id = Guid.NewGuid();
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created customer with ID: {CustomerId}, CompanyName: {CompanyName}", customer.Id, customer.CompanyName);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (Exception ex) when (ex is not DbUpdateException && ex is not DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "Error occurred while creating customer: {CompanyName}", customer.CompanyName);
                return StatusCode(500, "An error occurred while creating the customer");
            }
        }

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        /// <param name="id">The unique identifier of the customer to update</param>
        /// <param name="customer">The updated customer object</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Customer updated successfully</response>
        /// <response code="400">Customer ID mismatch</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">An error occurred while updating the customer</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCustomer(Guid id, Customer customer)
        {
            try
            {
                if (id != customer.Id)
                {
                    _logger.LogWarning("Update failed - ID mismatch. Route ID: {RouteId}, Customer ID: {CustomerId}", id, customer.Id);
                    return BadRequest("Customer ID mismatch");
                }

                _logger.LogInformation("Updating customer with ID: {CustomerId}, CompanyName: {CompanyName}", id, customer.CompanyName);

                _context.Entry(customer).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Customers.AnyAsync(c => c.Id == id))
                    {
                        _logger.LogWarning("Update failed - Customer with ID: {CustomerId} not found", id);
                        return NotFound();
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex) when (ex is not DbUpdateException && ex is not DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerId}", id);
                return StatusCode(500, "An error occurred while updating the customer");
            }
        }

        /// <summary>
        /// Deletes a customer
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Customer deleted successfully</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">An error occurred while deleting the customer</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    _logger.LogWarning("Delete failed - Customer with ID: {CustomerId} not found", id);
                    return NotFound();
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted customer with ID: {CustomerId}, CompanyName: {CompanyName}", id, customer.CompanyName);
                return NoContent();
            }
            catch (Exception ex) when (ex is not DbUpdateException && ex is not DbUpdateConcurrencyException)
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID: {CustomerId}", id);
                return StatusCode(500, "An error occurred while deleting the customer");
            }
        }
    }

    /// <summary>
    /// Represents a paginated result set
    /// </summary>
    /// <typeparam name="T">The type of items in the result</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the list of items in the current page
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}