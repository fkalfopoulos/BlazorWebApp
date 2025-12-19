using EpsilonWebApp.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace EpsilonWebApp.Contracts.Tests.DTOs
{
    public class CustomerDtoTests
    {
        [Fact]
        public void CustomerDto_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var dto = new CustomerDto();

            // Assert
            dto.Id.Should().Be(Guid.Empty);
            dto.CompanyName.Should().BeNull();
            dto.ContactName.Should().BeNull();
            dto.Address.Should().BeNull();
            dto.City.Should().BeNull();
            dto.Region.Should().BeNull();
            dto.PostalCode.Should().BeNull();
            dto.Country.Should().BeNull();
            dto.Phone.Should().BeNull();
        }

        [Fact]
        public void CustomerDto_ShouldSetAndGetAllProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new CustomerDto
            {
                Id = id,
                CompanyName = "Tech Solutions",
                ContactName = "Jane Smith",
                Address = "456 Tech Ave",
                City = "San Francisco",
                Region = "CA",
                PostalCode = "94102",
                Country = "USA",
                Phone = "+1-555-0200"
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.CompanyName.Should().Be("Tech Solutions");
            dto.ContactName.Should().Be("Jane Smith");
            dto.Address.Should().Be("456 Tech Ave");
            dto.City.Should().Be("San Francisco");
            dto.Region.Should().Be("CA");
            dto.PostalCode.Should().Be("94102");
            dto.Country.Should().Be("USA");
            dto.Phone.Should().Be("+1-555-0200");
        }
    }

    public class PagedResultTests
    {
        [Fact]
        public void PagedResult_ShouldInitializeWithEmptyList()
        {
            // Arrange & Act
            var result = new PagedResult<CustomerDto>();

            // Assert
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.PageNumber.Should().Be(0);
            result.PageSize.Should().Be(0);
        }

        [Fact]
        public void PagedResult_TotalPages_ShouldCalculateCorrectly_WhenEvenDivision()
        {
            // Arrange
            var result = new PagedResult<CustomerDto>
            {
                TotalCount = 100,
                PageSize = 10
            };

            // Act
            var totalPages = result.TotalPages;

            // Assert
            totalPages.Should().Be(10);
        }

        [Fact]
        public void PagedResult_TotalPages_ShouldRoundUp_WhenUnevenDivision()
        {
            // Arrange
            var result = new PagedResult<CustomerDto>
            {
                TotalCount = 95,
                PageSize = 10
            };

            // Act
            var totalPages = result.TotalPages;

            // Assert
            totalPages.Should().Be(10); // 95 / 10 = 9.5, rounds up to 10
        }

        [Fact]
        public void PagedResult_TotalPages_ShouldBe1_WhenFewerItemsThanPageSize()
        {
            // Arrange
            var result = new PagedResult<CustomerDto>
            {
                TotalCount = 5,
                PageSize = 10
            };

            // Act
            var totalPages = result.TotalPages;

            // Assert
            totalPages.Should().Be(1);
        }

        [Fact]
        public void PagedResult_TotalPages_ShouldBe0_WhenNoItems()
        {
            // Arrange
            var result = new PagedResult<CustomerDto>
            {
                TotalCount = 0,
                PageSize = 10
            };

            // Act
            var totalPages = result.TotalPages;

            // Assert
            totalPages.Should().Be(0);
        }

        [Fact]
        public void PagedResult_ShouldStoreItems()
        {
            // Arrange
            var items = new List<CustomerDto>
            {
                new CustomerDto { Id = Guid.NewGuid(), CompanyName = "Company1" },
                new CustomerDto { Id = Guid.NewGuid(), CompanyName = "Company2" }
            };

            // Act
            var result = new PagedResult<CustomerDto>
            {
                Items = items,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.Should().BeEquivalentTo(items);
        }

        [Theory]
        [InlineData(100, 10, 10)]
        [InlineData(50, 20, 3)]
        [InlineData(1, 10, 1)]
        [InlineData(0, 10, 0)]
        [InlineData(99, 10, 10)]
        public void PagedResult_TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedPages)
        {
            // Arrange
            var result = new PagedResult<CustomerDto>
            {
                TotalCount = totalCount,
                PageSize = pageSize
            };

            // Act & Assert
            result.TotalPages.Should().Be(expectedPages);
        }
    }

    public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_ShouldHaveDefaultEmptyStrings()
        {
            // Arrange & Act
            var request = new LoginRequest();

            // Assert
            request.Username.Should().Be(string.Empty);
            request.Password.Should().Be(string.Empty);
        }

        [Fact]
        public void LoginRequest_ShouldSetAndGetProperties()
        {
            // Arrange & Act
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "testpass123"
            };

            // Assert
            request.Username.Should().Be("testuser");
            request.Password.Should().Be("testpass123");
        }
    }

    public class LoginResponseTests
    {
        [Fact]
        public void LoginResponse_ShouldHaveDefaultEmptyStrings()
        {
            // Arrange & Act
            var response = new LoginResponse();

            // Assert
            response.Token.Should().Be(string.Empty);
            response.Username.Should().Be(string.Empty);
        }

        [Fact]
        public void LoginResponse_ShouldSetAndGetProperties()
        {
            // Arrange & Act
            var response = new LoginResponse
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                Username = "admin"
            };

            // Assert
            response.Token.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
            response.Username.Should().Be("admin");
        }
    }
}
