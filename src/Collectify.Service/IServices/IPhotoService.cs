using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Photos;
using Collectify.Service.DTOs.Users;
using Collectify.Service.Responses;
using Microsoft.AspNetCore.Http;

namespace Collectify.Service.IServices;

public interface IPhotoService
{
    Task<Response<bool>> RemoveAsync(long id);
    Task<Response<Photo>> ModifyAsync(PhotoUpdateDto dto);
    Task<Response<Photo>> GetAsync(long id);
}