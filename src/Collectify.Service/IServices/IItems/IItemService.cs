using System.Linq.Expressions;
using Collectify.Service.Responses;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Domain.Entities.Items.Basics;

namespace Collectify.Service.IServices.IItems;

public interface IItemService
{
    Task<Response<bool>> RemoveAsync(long id);
    Task<Response<ItemResultDto>> GetAsync(long id);
    Task<Response<ItemResultDto>> AddAsync(ItemCreationDto dto);
    Task<Response<ItemResultDto>> ModifyAsync(ItemUpdateDto dto);
    Task<Response<PaginatedData<ItemResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<Item, bool>> expression = null,
        string searchString = null);
}