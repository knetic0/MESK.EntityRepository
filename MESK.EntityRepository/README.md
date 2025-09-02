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

## Repository Methods Overview

MESK.EntityRepository provides a set of generic, asynchronous methods for managing entities. Below is a detailed explanation of each method, including parameters, return types, and usage examples.

### GetAsync<TDto>

Retrieves a single entity by its identifier and maps it to the specified DTO type.

**Signature:**
```csharp
Task<TDto?> GetAsync<TDto>(TKey id, CancellationToken cancellationToken = default);
```
**Parameters:**
- `id`: The unique identifier of the entity.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Returns:**
- The mapped DTO instance, or null if not found.

**Example:**
```csharp
var product = await _repository.GetAsync<ProductDto>(id);
```

---

### GetAllAsync<TDto>

Retrieves all entities and maps them to the specified DTO type.

**Signature:**
```csharp
Task<List<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default);
```
**Returns:**
- A list of mapped DTOs.

**Example:**
```csharp
var products = await _repository.GetAllAsync<ProductDto>();
```

---

### GetAllAsync<TDto>(PaginationQuery)

Retrieves entities with pagination, filtering, and sorting, mapping results to the specified DTO type.

**Signature:**
```csharp
Task<PaginationResult<TDto>> GetAllAsync<TDto>(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
```
**Parameters:**
- `paginationQuery`: Contains page number, page size, sorting, and filtering options.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Returns:**
- A `PaginationResult<TDto>` containing items, count, total count, page number, and page size.

**PaginationQuery Example:**
```csharp
// For single-field filtering (current implementation):
var query = new PaginationQuery
{
	PageNumber = 1,
	PageSize = 10,
	SortField = "Name",
	SortDirection = SortDirections.Asc,
	Filters = new Dictionary<string, FiltersValue>
	{
		{ "Name", new FiltersValue { MatchMode = MatchModes.Contains, Value = "phone" } },
		{ "Description", new FiltersValue { MatchMode = MatchModes.Contains, Value = "wireless" } }
	}
};

var result = await _repository.GetAllAsync<ProductDto>(query);
// result.Items: List<ProductDto>
// result.TotalCount: total number of matching records
```

---

### CreateAsync<TDto>

Creates a new entity and returns it mapped to the specified DTO type.

**Signature:**
```csharp
Task<TDto> CreateAsync<TDto>(T entity, CancellationToken cancellationToken = default);
```
**Parameters:**
- `entity`: The entity instance to create.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Returns:**
- The created entity mapped to a DTO.

**Example:**
```csharp
var product = new Product { Id = Guid.NewGuid(), Name = "New Product" };
var created = await _repository.CreateAsync<ProductDto>(product);
```

---

### UpdateAsync<TDto, TUpdateDto>

Updates an existing entity by its identifier using the provided update DTO. Returns the updated entity mapped to the specified DTO type.

**Signature:**
```csharp
Task<TDto> UpdateAsync<TDto, TUpdateDto>(TKey id, TUpdateDto dto, CancellationToken cancellationToken = default);
```
**Parameters:**
- `id`: The unique identifier of the entity to update.
- `dto`: The update DTO with new values.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Returns:**
- The updated entity mapped to a DTO.

**Example:**
```csharp
var updateDto = new ProductUpdateDto { Name = "Updated Name" };
var updated = await _repository.UpdateAsync<ProductDto, ProductUpdateDto>(id, updateDto);
```

---

### DeleteAsync

Deletes an entity by its identifier.

**Signature:**
```csharp
Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
```
**Parameters:**
- `id`: The unique identifier of the entity to delete.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Example:**
```csharp
await _repository.DeleteAsync(id);
```

---

### CreateRangeAsync<TDto>

Creates multiple entities and returns them mapped to the specified DTO type.

**Signature:**
```csharp
Task<List<TDto>> CreateRangeAsync<TDto>(IEnumerable<T> entities, CancellationToken cancellationToken = default);
```
**Parameters:**
- `entities`: The list of entity instances to create.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Returns:**
- The created entities mapped to a list of DTOs.

**Example:**
```csharp
var products = new List<Product>
{
	new Product { Id = Guid.NewGuid(), Name = "Product 1" },
	new Product { Id = Guid.NewGuid(), Name = "Product 2" }
};
var createdList = await _repository.CreateRangeAsync<ProductDto>(products);
```

---

### DeleteRangeAsync

Deletes multiple entities by their identifiers.

**Signature:**
```csharp
Task DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
```
**Parameters:**
- `ids`: The unique identifiers of the entities to delete.
- `cancellationToken`: (Optional) Token for cancelling the operation.

**Example:**
```csharp
var idsToDelete = new List<Guid> { id1, id2, id3 };
await _repository.DeleteRangeAsync(idsToDelete);
```

---

## Exception Handling

If an entity is not found, an `EntityNotFoundException` is thrown. You can catch this exception to handle not-found scenarios gracefully:

```csharp
try
{
	await _repository.DeleteAsync(id);
}
catch (EntityNotFoundException ex)
{
	// Handle not found
}
```

## Contributing

Contributions are welcome! Please open issues or submit pull requests for bug fixes, new features, or improvements. Make sure to follow the existing code style and include relevant tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
