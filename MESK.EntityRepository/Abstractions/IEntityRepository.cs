using MESK.EntityRepository.Abstractions.Dto;

namespace MESK.EntityRepository.Abstractions;

public interface IEntityRepository<in T, in TKey> where T : IEntity<TKey>
{
    Task<TDto?> GetAsync<TDto>(TKey id, CancellationToken cancellationToken = default);
    Task<List<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default);
    Task<PaginationResult<TDto>> GetAllAsync<TDto>(PaginationQuery  paginationQuery, CancellationToken cancellationToken = default);
    Task<TDto> CreateAsync<TDto>(T entity, CancellationToken cancellationToken = default);
}