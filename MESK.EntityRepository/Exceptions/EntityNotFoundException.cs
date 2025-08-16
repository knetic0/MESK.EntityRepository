namespace MESK.EntityRepository.Exceptions;

[Serializable]
public class EntityNotFoundException : Exception
{
    public Type? EntityType { get; set; }
    public object? Key { get; set; }
    
    public EntityNotFoundException() { }
    
    public EntityNotFoundException(string message) : base(message) { }
    
    public EntityNotFoundException(string message, Exception inner) : base(message, inner) { }

    public EntityNotFoundException(Type entityType, object? key)
        : base($"Entity of type {entityType.Name} with key {key} was not found.")
    {
        EntityType = entityType;
        Key = key;
    }
}