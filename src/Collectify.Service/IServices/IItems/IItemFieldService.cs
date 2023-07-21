using Collectify.Domain.Entities.Items.Basics;
using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Items.Fields;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Responses;
using System.Linq.Expressions;

namespace Collectify.Service.IServices.IItems;

public interface IItemFieldService
{
    Task<Response<ItemField>> GetAsync(long id);
    Task<Response<bool>> AddAllAsync(List<ItemFieldCreationDto> fields, long itemId);
    Task<Response<ItemField>> ModifyAsync(ItemFieldUpdateDto dto);
    Task<Response<PaginatedData<ItemField>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemField, bool>> expression = null);
}