using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Responses;
using System.Linq.Expressions;

namespace Collectify.Service.IServices.IItems;

public interface IItemLikeService
{
    Task<Response<ItemLikeResultDto>> AddAsync(long itemId);
    Task<Response<bool>> RemoveAsync(long itemId);
    Task<Response<bool>> SawAllReceivedLikesAsync();
    Task<Response<ItemLikeResultDto>> GetAsync(long userId, long itemId);
    Task<Response<PaginatedData<ItemLikeResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemLike, bool>> expression = null);
}