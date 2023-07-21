using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Responses;
using static Dropbox.Api.TeamLog.TimeUnit;
using System.Linq.Expressions;
using Collectify.Service.DTOs.Collections;
using Collectify.Domain.Entities.Others;
using System.Runtime.CompilerServices;

namespace Collectify.Service.IServices;

public interface ICollectionService
{
    Task<Response<bool>> RemoveAsync(long id);
    Task<Response<CollectionResultDto>> GetAsync(long id);
    Task<Response<CollectionResultDto>> AddAsync(CollectionCreationDto dto);
    Task<Response<CollectionResultDto>> ModifyAsync(CollectionUpdateDto dto);
    Task<Response<PaginatedData<CollectionResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<Collection, bool>> expression = null,
        string searchString = null);
    Task<Response<CollectionResultDto>> AddFieldAsync(CollectionFieldAddDto dto);
    Task<Response<CollectionResultDto>> RemoveFieldAsync(CollectionFieldRemoveDto dto);
}