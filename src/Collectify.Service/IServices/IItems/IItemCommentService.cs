using System.Linq.Expressions;
using Collectify.Service.Responses;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Domain.Entities.Items.ItemComments;

namespace Collectify.Service.IServices.IItems;

public interface IItemCommentService
{
    Task<Response<bool>> RemoveAsync(long id);
    Task<Response<ItemCommentResultDto>> GetAsync(long id);
    Task<Response<ItemCommentResultDto>> AddAsync(ItemCommentCreationDto dto);
    Task<Response<bool>> SawAllReceivedCommentsAsync();
    Task<Response<ItemCommentResultDto>> ModifyAsync(ItemCommentUpdateDto dto);
    Task<Response<PaginatedData<ItemCommentResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemComment, bool>> expression = null,
        string searchString = null);
}