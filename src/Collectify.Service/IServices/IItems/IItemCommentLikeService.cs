using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Responses;
using System.Linq.Expressions;

namespace Collectify.Service.IServices.IItems;

public interface IItemCommentLikeService
{

    Task<Response<ItemCommentLikeResultDto>> AddAsync(long itemCommentId);
    Task<Response<bool>> RemoveAsync(long itemCommentId);
    Task<Response<bool>> SawAllReceivedLikesAsync();
    Task<Response<ItemCommentLikeResultDto>> GetAsync(long userId, long itemCommentId);
    Task<Response<PaginatedData<ItemCommentLikeResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemCommentLike, bool>> expression = null);
}