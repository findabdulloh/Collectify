using Collectify.Domain.Commons;
using Collectify.Service.DTOs.Paginations;

namespace Collectify.Service.Extensions;

public static class CollectionExtensions
{
    public static IQueryable<TEntity> Paginate<TEntity>(this IQueryable<TEntity> entities, PaginationParams @params)
            where TEntity : Auditable
    {
        if (@params.PageSize <= 0 || @params.PageIndex <= 0)
        {
            @params.PageSize = 10;
            @params.PageIndex = 1;
        }

        return entities
            .OrderByDescending(e => e.Id)
            .Skip((@params.PageIndex - 1) * @params.PageSize)
            .Take(@params.PageSize);
    }
}