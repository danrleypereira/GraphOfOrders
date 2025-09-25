using GraphOfOrders.Lib.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphOfOrders.Repo.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepoServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrdersContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            // DeliverPersonRepository moved to delivery-service
            
            return services;
        }
    }
}