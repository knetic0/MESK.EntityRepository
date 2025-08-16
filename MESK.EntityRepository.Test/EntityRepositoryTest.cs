using System.ComponentModel.DataAnnotations;
using MESK.EntityRepository.Abstractions;
using MESK.EntityRepository.Abstractions.Dto;
using MESK.EntityRepository.Exceptions;
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

    class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
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
    public async Task GetAllAsync_ReturnsListOfProduct_WhenPaginationQueryGiven()
    {
        var opts = CreateInMemoryDbContextOptions();
        
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        var paginationQuery = new PaginationQuery()
        {
            PageNumber = 1,
            PageSize = 3,
            SortField = "Price",
        };

        using (var context = new TestDbContext(opts))
        {
            var productsRepository = new ProductRepository(context);
            var results = await productsRepository.GetAllAsync<ProductDto>(paginationQuery);
            
            Assert.NotNull(results);
            Assert.Equal(6, results.TotalCount);
            Assert.Equal(3, results.Count);
            
            Assert.Equal(100, results.Items[0].Price);
            Assert.Equal(150, results.Items[1].Price);
            Assert.Equal(200, results.Items[2].Price);
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedProduct_WhenUpdatedSuccess()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        using (var context = new TestDbContext(opts))
        {
            var productsRepository = new ProductRepository(context);

            var updateProductDto = new UpdateProductDto()
            {
                Name = "Updated Product",
                Description = "Updated Product",
                Price = 50,
            };

            var result = await productsRepository.UpdateAsync<ProductDto, UpdateProductDto>(1, updateProductDto);
            Assert.Equal(updateProductDto.Name, result.Name);
            Assert.Equal(updateProductDto.Description, result.Description);
            Assert.Equal(updateProductDto.Price, result.Price);
        }
    }

    [Fact]
    public async Task DeleteAsync_ReturnsVoid_WhenEntityExists()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        using (var context = new TestDbContext(opts))
        {
            var productsRepository = new ProductRepository(context);
            await productsRepository.DeleteAsync(1);
        }
    }
    
    [Fact]
    public async Task DeleteAsync_ThrowEntityNotFoundException_WhenEntityNotExists()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        using (var context = new TestDbContext(opts))
        {
            var productsRepository = new ProductRepository(context);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => productsRepository.DeleteAsync(999)
            );
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfProducts()
    {
        var opts = CreateInMemoryDbContextOptions();

        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
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
            await context.Products.AddRangeAsync(GetSeedProducts());
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
    
    private static List<Product> GetSeedProducts() => new()
    {
        new Product { Name = "Test1", Description = "Test1Description", Price = 100 },
        new Product { Name = "Test2", Description = "Test2Description", Price = 200 },
        new Product { Name = "Test3", Description = "Test3Description", Price = 300 },
        new Product { Name = "Test4", Description = "Test4Description", Price = 225 },
        new Product { Name = "Test5", Description = "Test5Description", Price = 250 },
        new Product { Name = "Test6", Description = "Test6Description", Price = 150 }
    };

}