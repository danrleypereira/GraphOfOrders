using Microsoft.EntityFrameworkCore;
using GraphOfOrders.Repo;
using GraphOfOrders.Lib.Entities;

public class OrderRepositoryShould
{
    private readonly OrdersContext _context;
    private readonly OrderRepository _repo;

    public OrderRepositoryShould()
    {
        var options = new DbContextOptionsBuilder<OrdersContext>()
            .UseInMemoryDatabase(databaseName: "OrderTestDatabase")
            .Options;

        _context = new OrdersContext(options);
        _repo = new OrderRepository(_context);
    }

    [Fact]
    public void GetOrdersByBrand_ReturnsOrders()
    {
        // Arrange
        var customer = new Customer { CustomerId = 1, Name = "Test Customer", Email = "test@example.com" };
        var category = new Category { CategoryId = 1, CategoryName = "Test Category" };
        var product = new Product { ProductId = 1, ProductName = "Test Product", CategoryId = 1, Category = category };
        var brand = new Brand { BrandId = 1, BrandName = "Test Brand", ProductId = 1, Product = product };
        var order = new Order { OrderId = 1, BrandId = 1, CustomerId = 1, OrderDate = DateTime.Now, Brand = brand, Customer = customer };
        
        _context.Customers.Add(customer);
        _context.Categories.Add(category);
        _context.Products.Add(product);
        _context.Brands.Add(brand);
        _context.Orders.Add(order);
        _context.SaveChanges();

        // Act
        var result = _repo.GetOrdersByBrand(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().OrderId);
    }
}
