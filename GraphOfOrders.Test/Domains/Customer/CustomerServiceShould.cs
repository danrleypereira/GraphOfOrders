using Moq;
using AutoMapper;
using GraphOfOrders.Service;
using GraphOfOrders.Lib.DI;
using GraphOfOrders.Lib.DTOs;
using GraphOfOrders.Lib.Entities;
using GraphOfOrders.Lib.Exceptions;

public class CustomerServiceShould
{
    private readonly Mock<ICustomerRepository> _mockRepo;
    private readonly CustomerService _service;
    private readonly Mock<IMapper> _mockMapper;

    public CustomerServiceShould()
    {
        _mockRepo = new Mock<ICustomerRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockMapper.Setup(m => m.Map<Customer>(It.IsAny<CustomerInputDTO>()))
                    .Returns((CustomerInputDTO source) => new Customer
                    {
                        CustomerId = 1, // You might want to set this to a specific value for testing
                        Name = source.Name,
                        Email = source.Email
                    });

        _mockMapper.Setup(m => m.Map<CustomerDTO>(It.IsAny<Customer>()))
                    .Returns((Customer source) => new CustomerDTO
                    {
                        CustomerId = source.CustomerId,
                        Name = source.Name,
                        Email = source.Email
                    });
        _service = new CustomerService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task ReturnCustomerDTO_WhenCreatingCustomer()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, Name = "Test", Email = "test@example.com" };
        var createCustomer = new CustomerInputDTO { Name = "Test", Email = "test@example.com" };
        var expected = new CustomerDTO { CustomerId = 1, Name = "Test", Email = "test@example.com" };

        _mockRepo.Setup(repo => repo.CreateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

        // Act
        var result = await _service.CreateCustomer(createCustomer);

        // Assert
        Assert.Equal(expected.CustomerId, result.CustomerId);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Email, result.Email);
        _mockRepo.Verify(repo => repo.CreateCustomer(It.IsAny<Customer>()), Times.Once);
    }
    [Fact]
    public async Task UpdateCustomer_ShouldReturnUpdatedCustomerDTO_WhenUpdateIsSuccessful()
    {
        // Arrange
        var customerId = 1;
        var updateCustomerDto = new CustomerInputDTO { Name = "UpdatedName", Email = "updated@example.com" };
        var updatedCustomer = new Customer { CustomerId = customerId, Name = "UpdatedName", Email = "updated@example.com" };
        var expected = new CustomerDTO { CustomerId = customerId, Name = "UpdatedName", Email = "updated@example.com" };

        _mockRepo.Setup(repo => repo.UpdateCustomer(customerId, It.IsAny<Customer>()))
                 .ReturnsAsync(updatedCustomer);
        _mockMapper.Setup(mapper => mapper.Map<Customer>(updateCustomerDto)).Returns(updatedCustomer);
        _mockMapper.Setup(mapper => mapper.Map<CustomerDTO>(updatedCustomer)).Returns(expected);

        // Act
        var result = await _service.UpdateCustomer(customerId, updateCustomerDto);

        // Assert
        Assert.Equal(expected, result);
        _mockRepo.Verify(repo => repo.UpdateCustomer(customerId, It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldThrowNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = 1;
        var updateCustomerDto = new CustomerInputDTO { Name = "UpdatedName", Email = "updated@example.com" };

        _mockRepo.Setup(repo => repo.UpdateCustomer(customerId, It.IsAny<Customer>()))
                 .ReturnsAsync((Customer)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateCustomer(customerId, updateCustomerDto));
        _mockRepo.Verify(repo => repo.UpdateCustomer(customerId, It.IsAny<Customer>()), Times.Once);
    }


}
