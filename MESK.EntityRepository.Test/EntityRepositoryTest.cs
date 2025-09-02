using System.ComponentModel.DataAnnotations;
using MESK.EntityRepository.Abstractions;
using MESK.EntityRepository.Abstractions.Dto;
using MESK.EntityRepository.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MESK.EntityRepository.Test;

public class Product : Entity<int>
{
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
    
public interface IProductRepository : IEntityRepository<Product, int> { }

public class ProductRepository(DbContext context) : EntityRepository<Product, int>(context), IProductRepository { }

public class EntityRepositoryTest
{
    [Fact]
    public async Task GetAllAsync_ReturnsListOfProduct_WhenPaginationQueryGiven()
    {
        var opts = CreateInMemoryDbContextOptions();

        var seedProducts = GetSeedProducts();
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(seedProducts);
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
            Assert.Equal(seedProducts.Count, results.TotalCount);
            Assert.Equal(3, results.Count);
        }
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsListOfProduct_WhenPaginationQueryGivenWithDescriptionFilter()
    {
        var opts = CreateInMemoryDbContextOptions();
        
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        var paginationQuery = new PaginationQuery
        {
            PageNumber = 1,
            PageSize = 3,
            SortField = "Price",
            Filters = new Dictionary<string, FiltersValue>
            {
                ["Description"] = new FiltersValue { MatchMode = MatchModes.Contains, Value = "Software" }
            }
        };

        using (var context = new TestDbContext(opts))
        {
            var repo = new ProductRepository(context);
            var results = await repo.GetAllAsync<ProductDto>(paginationQuery);
            
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfProduct_WhenPaginationQueryGivenWithPriceFilter()
    {
        var opts = CreateInMemoryDbContextOptions();
        
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        var paginationQuery = new PaginationQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortField = "Price",
            Filters = new Dictionary<string, FiltersValue>
            {
                ["Price"] = new FiltersValue { MatchMode = MatchModes.LessThan, Value = 100 }
            }
        };

        using (var context = new TestDbContext(opts))
        {
            var repo = new ProductRepository(context);
            var results = await repo.GetAllAsync<ProductDto>(paginationQuery);
            
            Assert.NotNull(results);
            Assert.Equal(9, results.Count);
            var product = results.Items.First();
            Assert.Equal("T-Shirt Cotton", product.Name);
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
    public async Task DeleteRangeAsync_ThrowEntityNotFoundException_WhenEntityNotExists()
    {
        var opts = CreateInMemoryDbContextOptions();

        var products = GetSeedProducts();
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        using (var context = new TestDbContext(opts))
        {
            var productsRepository = new ProductRepository(context);

            var ids = new List<int>() { 1, 5 };
            await productsRepository.DeleteRangeAsync(ids);

            var productsList = await productsRepository.GetAllAsync<ProductDto>();
            var length = productsList.Count();
            Assert.Equal(products.Count - ids.Count, length);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfProducts()
    {
        var opts = CreateInMemoryDbContextOptions();
        
        var seedProducts = GetSeedProducts();
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(seedProducts);
            await context.SaveChangesAsync();
        }

        var productsLength = seedProducts.Count;
        
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
    public async Task CreateRangeAsync_ReturnsOkResult_WhenEntitiesIsCreated()
    {
        var opts = CreateInMemoryDbContextOptions();

        var product1 = new Product()
        {
            Name = "Test",
            Description = "Test",
            Price = 100.00m
        };

        var product2 = new Product()
        {
            Name = "Test2",
            Description = "Test2",
            Price = 100.00m
        };

        var product3 = new Product()
        {
            Name = "Test3",
            Description = "Test3",
            Price = 100.00m
        };
        
        var products =  new List<Product>() { product1, product2, product3 };

        using (var context = new TestDbContext(opts))
        {
            var productRepository = new ProductRepository(context);
            var result = await productRepository.CreateRangeAsync<ProductDto>(products);
            
            Assert.NotNull(result);
            Assert.Equal(products.Count, result.Count);
            Assert.Equal(products[0].Name, result.FirstOrDefault()?.Name);
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
            Assert.Equal("Laptop Dell XPS", result.Name);
            Assert.Equal("13-inch ultrabook, Intel i7, 16GB RAM", result.Description);
            Assert.Equal(1850, result.Price);
        }
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldSetHasMore_WhenMoreItemsExist()
    {
        // Arrange
        var opts = CreateInMemoryDbContextOptions();
        var products = GetSeedProducts();
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(GetSeedProducts());
            await context.SaveChangesAsync();
        }

        var paginationQuery = new PaginationQuery
        {
            PageNumber = 1,
            PageSize = 3
        };

        // Act
        using (var context = new TestDbContext(opts))
        {
            var repo = new ProductRepository(context);
            var result = await repo.GetAllAsync<ProductDto>(paginationQuery);

            // Assert
            Assert.Equal(products.Count, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
            Assert.True(result.HasNextPage);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldSetHasMoreFalse_WhenNoMoreItemsExist()
    {
        // Arrange
        var opts = CreateInMemoryDbContextOptions();
        var products = GetSeedProducts();
        using (var context = new TestDbContext(opts))
        {
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        var paginationQuery = new PaginationQuery
        {
            PageNumber = 4,
            PageSize = 5
        };

        // Act
        using (var context = new TestDbContext(opts))
        {
            var repo = new ProductRepository(context);
            var result = await repo.GetAllAsync<ProductDto>(paginationQuery);

            // Assert
            Assert.Equal(products.Count, result.TotalCount);
            Assert.Equal(5, result.Items.Count);
            Assert.False(result.HasNextPage);
        }
    }
    
    private DbContextOptions<TestDbContext> CreateInMemoryDbContextOptions()
        => new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    
    private static List<Product> GetSeedProducts() => new()
    {
        new Product { Name = "Laptop Dell XPS", Description = "13-inch ultrabook, Intel i7, 16GB RAM", Price = 1850 },
        new Product { Name = "Smartphone Samsung Galaxy", Description = "Android phone with AMOLED display", Price = 1200 },
        new Product { Name = "Wireless Headphones", Description = "Noise cancelling over-ear headphones", Price = 350 },
        new Product { Name = "Gaming Mouse", Description = "Ergonomic mouse with RGB lighting", Price = 75 },
        new Product { Name = "Mechanical Keyboard", Description = "Backlit keyboard with blue switches", Price = 150 },

        new Product { Name = "T-Shirt Cotton", Description = "100% cotton, size M", Price = 25 },
        new Product { Name = "Jeans Slim Fit", Description = "Dark blue slim fit jeans", Price = 60 },
        new Product { Name = "Sneakers Nike Air", Description = "Comfortable running shoes", Price = 110 },
        new Product { Name = "Winter Jacket", Description = "Waterproof insulated jacket", Price = 200 },
        new Product { Name = "Wool Scarf", Description = "Handmade wool scarf", Price = 35 },

        new Product { Name = "C# in Depth", Description = "Programming book by Jon Skeet", Price = 55 },
        new Product { Name = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship", Price = 45 },
        new Product { Name = "The Pragmatic Programmer", Description = "Journey to Mastery", Price = 50 },
        new Product { Name = "Design Patterns", Description = "Elements of Reusable Object-Oriented Software", Price = 65 },
        new Product { Name = "Introduction to Algorithms", Description = "CLRS textbook", Price = 90 },

        new Product { Name = "Office Chair", Description = "Ergonomic chair with adjustable height", Price = 180 },
        new Product { Name = "Dining Table", Description = "Wooden table for 6 people", Price = 700 },
        new Product { Name = "Bookshelf", Description = "5-tier wooden bookshelf", Price = 120 },
        new Product { Name = "Bed Frame", Description = "Queen size wooden bed frame", Price = 850 },
        new Product { Name = "Sofa", Description = "3-seater fabric sofa", Price = 950 },
    };
}