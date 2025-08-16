namespace MESK.EntityRepository.Abstractions;

/// <summary>
/// Represents the base contract for an entity with a strongly typed identifier and audit fields.
/// </summary>
/// <typeparam name="TKey">The type of the unique identifier for the entity.</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    TKey Id { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC date and time when the entity was last updated.
    /// Null if the entity has not been updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Provides a base implementation of <see cref="IEntity{TKey}"/> with default behavior.
/// </summary>
/// <typeparam name="TKey">The type of the unique identifier for the entity.</typeparam>
public abstract class Entity<TKey> : IEntity<TKey>
{
    /// <inheritdoc />
    public TKey Id { get; set; } = default!;
    
    /// <inheritdoc />
    /// <remarks>
    /// Initialized with <see cref="DateTime.UtcNow"/> when the entity instance is created.
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }
}