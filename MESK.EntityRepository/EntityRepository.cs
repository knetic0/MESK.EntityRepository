using System.Linq.Expressions;
using Mapster;
using MESK.EntityRepository.Abstractions;
using MESK.EntityRepository.Abstractions.Dto;
using MESK.EntityRepository.Exceptions;
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

    public async Task<PaginationResult<TDto>> GetAllAsync<TDto>(PaginationQuery paginationQuery,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(paginationQuery.Filters.Key) &&
            !string.IsNullOrEmpty(paginationQuery.Filters.Value.Value))
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, paginationQuery.Filters.Key);
            
            var toStringCall = Expression.Call(property, "ToString", Type.EmptyTypes);
            var valueExpression = Expression.Constant(paginationQuery.Filters.Value.Value, typeof(string));
            
            Expression? filterExpression = null;
            switch (paginationQuery.Filters.Value.MatchMode)
            {
                case MatchModes.Contains:
                    filterExpression = Expression.Call(toStringCall,
                        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!, valueExpression);
                    break;
                case MatchModes.StartsWith:
                    filterExpression = Expression.Call(toStringCall,
                        typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!, valueExpression);
                    break;
                case MatchModes.EndsWith:
                    filterExpression = Expression.Call(toStringCall,
                        typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!, valueExpression);
                    break;
            }

            if (filterExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
                query = query.Where(lambda);
            }
        }
        if (!string.IsNullOrEmpty(paginationQuery.SortField))
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.PropertyOrField(parameter, paginationQuery.SortField);
            var lambda = Expression.Lambda(property, parameter);
            string methodName = paginationQuery.SortDirection == SortDirections.Desc ? "OrderByDescending" : "OrderBy";
            var orderByCall = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda));
            
            query = query.Provider.CreateQuery<T>(orderByCall);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((paginationQuery.PageNumber - 1) * paginationQuery.PageSize)
            .Take(paginationQuery.PageSize)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

        return new PaginationResult<TDto>
        {
            TotalCount = totalCount,
            Count = items.Count,
            Items = items,
            PageNumber = paginationQuery.PageNumber,
            PageSize = paginationQuery.PageSize
        };
    }
    
    public async Task<TDto> CreateAsync<TDto>(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Adapt<TDto>();
    }

    public async Task<TDto> UpdateAsync<TDto, TUpdateDto>(TKey id, TUpdateDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        
        updateDto.Adapt(entity);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity.Adapt<TDto>();
    }

    public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
            
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<T> FindByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<T>().FindAsync([id], cancellationToken);
        if (entity is null)
            throw new EntityNotFoundException(typeof(T), id);
        return entity;
    }
}