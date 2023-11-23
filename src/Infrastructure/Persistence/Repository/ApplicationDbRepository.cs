using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Demo.WebApi.Application.Common.Persistence;
using Demo.WebApi.Domain.Common.Contracts;
using Demo.WebApi.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebApi.Infrastructure.Persistence.Repository;

// Inherited from Ardalis.Specification's RepositoryBase<T>
public class ApplicationDbRepository<T> : RepositoryBase<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public readonly ApplicationDbContext Context;
    public readonly DbSet<T> Entities;
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        this.Context = dbContext;
        this.Entities = dbContext.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        return Entities;
    }

    public IQueryable<TResult> GetAll<TResult>()
    {
        return Entities.ProjectToType<TResult>();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        this.Entities.UpdateRange(entities);
        await this.Context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await this.Entities.AddRangeAsync(entities);
        await this.Context.SaveChangesAsync();
    }

    public async Task AddWithoutSaveAsync(T entity)
    {
        await this.Entities.AddAsync(entity);
    }

    public void UpdateWithoutSave(T entity)
    {
        this.Entities.Update(entity);
    }

    // We override the default behavior when mapping to a dto.
    // We're using Mapster's ProjectToType here to immediately map the result from the database.
    // This is only done when no Selector is defined, so regular specifications with a selector also still work.
    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification) =>
        specification.Selector is not null
            ? base.ApplySpecification(specification)
            : ApplySpecification(specification, false)
                .ProjectToType<TResult>();
}