# MESK.EntityRepository

MESK.EntityRepository is a lightweight, extensible, and easy-to-use repository abstraction library for .NET projects. It is designed to simplify data access patterns, especially in WebApi projects, by providing generic repository interfaces and base implementations for managing your entities.

## Features

- Generic repository pattern for entity management
- Abstractions for custom implementations
- Pagination, filtering, and sorting support
- Exception handling for common data access scenarios
- Designed for extensibility and testability

## Installation

Install MESK.EntityRepository via [NuGet](https://www.nuget.org/):

```shell
dotnet add package MESK.EntityRepository
```

## Usage in ASP.NET Core WebApi

### 1. Define Your Entity

```csharp
public class Product : Entity<Guid>
{
	public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
```

### 2. Define Your Repository Interface

```csharp
public interface IProductRepository : IEntityRepository<Product, Guid> { }
```

### 3. Define Your Repository Class

```csharp
public class ProductRepository : EntityRepository<Product, Guid>,  IProductRepository { }
```

### 4. Register Repository in Dependency Injection
In your `Startup.cs` or `Program.cs`:

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### 5. Use Repository in Your Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
	private readonly IProduct _repository;

	public ProductsController(IProduct repository)
	{
		_repository = repository;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var products = await _repository.GetAllAsync<ProductDto>();
		return Ok(products);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> Get(Guid id)
	{
		var product = await _repository.GetAsync<ProductDto>(id);
		if (product == null) return NotFound();
		return Ok(product);
	}

	[HttpPost]
	public async Task<IActionResult> Create(ProductCreateDto dto)
	{
		var product = new Product { Id = Guid.NewGuid(), Name = dto.Name };
		var created = await _repository.CreateAsync<ProductDto>(product);
		return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
	}

	// ... other actions (Update, Delete, Pagination, etc.)
}
```

### 6. DTO Example

```csharp
public class ProductDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
}

public class ProductCreateDto
{
	public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
```

## Exception Handling

MESK.EntityRepository provides custom exceptions such as `EntityNotFoundException` to help you handle common data access errors gracefully.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for bug fixes, new features, or improvements. Make sure to follow the existing code style and include relevant tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
