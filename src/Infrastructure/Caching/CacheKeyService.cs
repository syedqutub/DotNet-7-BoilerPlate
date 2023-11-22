using Demo.WebApi.Application.Common.Caching;

namespace Demo.WebApi.Infrastructure.Caching;

public class CacheKeyService : ICacheKeyService
{
    public string GetCacheKey(string name, object id, bool includeTenantId = true)
    {
        return $"{name}-{id}";
    }
}