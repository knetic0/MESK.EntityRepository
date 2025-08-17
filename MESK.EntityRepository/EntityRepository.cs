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
        if (paginationQuery.Filters?.Count > 0)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            Expression? combined = null;
            foreach (var filter in paginationQuery.Filters)
            {
                var property = Expression.PropertyOrField(parameter, filter.Key);
                var typedValue = Convert.ChangeType(filter.Value.Value, property.Type);
                var valueExpression = Expression.Constant(typedValue, property.Type);

                Expression? filterExpression = null;
                filterExpression = filter.Value.MatchMode switch
                {
                    MatchModes.Equals => Expression.Equal(property, valueExpression),
                    MatchModes.NotEquals => Expression.NotEqual(property, valueExpression),
                    MatchModes.GreaterThan => Expression.GreaterThan(property, valueExpression),
                    MatchModes.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, valueExpression),
                    MatchModes.LessThan => Expression.LessThan(property, valueExpression),
                    MatchModes.LessThanOrEqual => Expression.LessThanOrEqual(property, valueExpression),
                    MatchModes.Contains => Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                        valueExpression
                    ),
                    MatchModes.StartsWith => Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
                        valueExpression
                    ),
                    MatchModes.EndsWith => Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
                        valueExpression
                    ),
                    _ => null
                };
                
                if(filterExpression != null)
                    combined = combined == null 
                        ? filterExpression
                        : Expression.AndAlso(combined, filterExpression);
            }
            
            if (combined != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
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