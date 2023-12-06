using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Demo.WebApi.Application.Common.Interfaces;
using Demo.WebApi.Application.Common.Models;
using Demo.WebApi.Application.Common.Specification;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebApi.Infrastructure.Common.Extensions;

public static class PaginationResponseExtensions
{
    public static async Task<PaginationResponse<TDestination>> PaginatedListAsync<T, TDestination>(
        this IReadRepositoryBase<T> repository, ISpecification<T, TDestination> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        where T : class
        where TDestination : class, IDto
    {
        var list = await repository.ListAsync(spec, cancellationToken);
        int count = await repository.CountAsync(spec, cancellationToken);

        return new PaginationResponse<TDestination>(list, count, pageNumber, pageSize);
    }

    public static async Task<PaginationResponse<T>> PaginatedListAsync<T>(
       this IQueryable<T> query, PaginationFilter filter, CancellationToken cancellationToken = default)
       where T : class
    {
        var spec = new EntitiesByPaginationFilterSpec<T>(filter);
        var list = await query.WithSpecification(spec).ToListAsync(cancellationToken);
        var spec2 = new EntitiesByBaseFilterSpec<T>(filter);
        int count = await query.WithSpecification(spec2).CountAsync(cancellationToken);

        return new PaginationResponse<T>(list, count, filter.PageNumber, filter.PageSize);
    }
}