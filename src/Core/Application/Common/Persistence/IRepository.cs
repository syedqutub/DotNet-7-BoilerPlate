namespace Demo.WebApi.Application.Common.Persistence;

// The Repository for the Application Db
// I(Read)RepositoryBase<T> is from Ardalis.Specification

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T>
    where T : class, IAggregateRoot
{
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task AddWithoutSaveAsync(T entity);
    void UpdateWithoutSave(T entity);
}

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{
    IQueryable<T> GetAll();
    IQueryable<TResult> GetAll<TResult>();
}

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IRepositoryWithEvents<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
}