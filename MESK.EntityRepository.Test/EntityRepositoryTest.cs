using System.ComponentModel.DataAnnotations;
using MESK.EntityRepository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace MESK.EntityRepository.Test;


public class EntityRepositoryTest
{
    class Product : Entity<int>
    {
        [MaxLength(100)]
        public string Name { get; init; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }

    class ProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }
    
    interface IProductRepository : IEntityRepository<Product, int> { }

    class ProductRepository(DbContext context) : EntityRepository<Product, int>(context), IProductRepository { }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfProducts()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            context.Products.Add(new Product { Name = "Test1", Description = "Test1Description", Price = 100 });
            context.Products.Add(new Product { Name = "Test2", Description = "Test2Description", Price = 200 });
            context.Products.Add(new Product { Name = "Test3", Description = "Test3Description", Price = 300 });
            context.Products.Add(new Product { Name = "Test4", Description = "Test4Description", Price = 200 });
            context.Products.Add(new Product { Name = "Test5", Description = "Test5Description", Price = 100 });
            context.Products.Add(new Product { Name = "Test6", Description = "Test6Description", Price = 200 });
            await context.SaveChangesAsync();
        }

        var productsLength = 6;
        
        using (var context = new TestDbContext(opts))
        {
            var productRepository = new ProductRepository(context);

            var result = await productRepository.GetAllAsync<ProductDto>();
            
            Assert.Equal(result.Count, productsLength);
        }
    }

    [Fact]
    public async Task CreateAsync_ReturnsOkResult_WhenEntityIsCreated()
    {
        var opts = CreateInMemoryDbContextOptions();

        var product = new Product()
        {
            Name = "Test",
            Description = "Test",
            Price = 100.00m
        };

        using (var context = new TestDbContext(opts))
        {
            var productRepository = new ProductRepository(context);
            var result = await productRepository.CreateAsync<ProductDto>(product);
            
            Assert.NotNull(result);
            Assert.Equal(result.Name, product.Name);
            Assert.Equal(result.Description, product.Description);
            Assert.Equal(result.Price, product.Price);
        }
    }
    
    [Fact]
    public async Task GetAsync_ReturnsOkResult_WhenSendEntityId()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            context.Products.Add(new Product { Name = "Test1", Description = "Test1Description", Price = 100 });
            context.Products.Add(new Product { Name = "Test2", Description = "Test2Description", Price = 200 });
            context.Products.Add(new Product { Name = "Test3", Description = "Test3Description", Price = 300 });
            context.Products.Add(new Product { Name = "Test4", Description = "Test4Description", Price = 200 });
            context.Products.Add(new Product { Name = "Test5", Description = "Test5Description", Price = 100 });
            context.Products.Add(new Product { Name = "Test6", Description = "Test6Description", Price = 200 });
            await context.SaveChangesAsync();
        }

        var testId = 1;

        using (var context = new TestDbContext(opts))
        {
            var productRepository = new ProductRepository(context);

            var result = await productRepository.GetAsync<ProductDto>(testId);

            Assert.NotNull(result);
            Assert.Equal("Test1", result.Name);
            Assert.Equal("Test1Description", result.Description);
            Assert.Equal(100, result.Price);
        }
    }
    
    private DbContextOptions<TestDbContext> CreateInMemoryDbContextOptions()
        => new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
}