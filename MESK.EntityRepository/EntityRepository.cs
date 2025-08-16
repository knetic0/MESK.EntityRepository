using Mapster;
using MESK.EntityRepository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MESK.EntityRepository;

public class EntityRepository<T, TKey>(DbContext context) : IEntityRepository<T, TKey>
    where T : class, IEntity<TKey>
{
    private readonly DbContext _context = context;

    public async Task<TDto?> GetAsync<TDto>(TKey id, CancellationToken cancellationToken = default)
        => await _context.Set<T>()
            .AsNoTracking()
            .Where(e => EqualityComparer<TKey>.Default.Equals(e.Id, id))
            .ProjectToType<TDto>()
            .FirstOrDefaultAsync(cancellationToken);    

    public async Task<List<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default)
        => await _context.Set<T>()
            .AsNoTracking()
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<TDto> CreateAsync<TDto>(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Adapt<TDto>();
    }
}