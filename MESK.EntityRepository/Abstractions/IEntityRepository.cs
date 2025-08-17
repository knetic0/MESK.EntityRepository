using MESK.EntityRepository.Abstractions.Dto;

namespace MESK.EntityRepository.Abstractions;

/// <summary>
/// Generic repository interface for entities implementing <see cref="IEntity{TKey}"/>.
/// Provides CRUD operations, projection to DTOs, and pagination support.
/// </summary>
public interface IEntityRepository<in T, in TKey> where T : IEntity<TKey>
{
    /// <summary>
    /// Retrieves a single entity by its identifier and maps it to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map the entity to.</typeparam>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>The mapped DTO instance, or null if the entity is not found.</returns>
    Task<TDto?> GetAsync<TDto>(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all entities and maps them to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map entities to.</typeparam>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>A list of mapped DTOs.</returns>
    Task<List<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves entities with pagination, filtering, and sorting options.
    /// Maps the results to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map entities to.</typeparam>
    /// <param name="paginationQuery">Pagination, filtering, and sorting options.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>A paginated result containing the mapped DTOs and total count.</returns>
    Task<PaginationResult<TDto>> GetAllAsync<TDto>(PaginationQuery  paginationQuery, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new entity and returns it mapped to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map the created entity to.</typeparam>
    /// <param name="entity">The entity instance to create.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>The created entity mapped to a DTO.</returns>
    Task<TDto> CreateAsync<TDto>(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new entities and returns it mapped to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map the created entity to.</typeparam>
    /// <param name="entities">The entity instance list to create.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>The created entity list mapped to a DTO.</returns>
    Task<List<TDto>> CreateRangeAsync<TDto>(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing entity by its identifier using the provided update DTO.
    /// Returns the updated entity mapped to the specified DTO type.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to return after update.</typeparam>
    /// <typeparam name="TUpdateDto">The DTO type containing updated values.</typeparam>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="dto">The update DTO with new values.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    /// <returns>The updated entity mapped to a DTO.</returns>
    Task<TDto> UpdateAsync<TDto, TUpdateDto>(TKey id, TUpdateDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an entities by its identifiers.
    /// </summary>
    /// <param name="ids">The unique identifiers of the entities to delete.</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation.</param>
    Task DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
}