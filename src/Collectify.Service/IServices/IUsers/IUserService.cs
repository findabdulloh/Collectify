using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Users;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Responses;
using System.Linq.Expressions;
using Collectify.Domain.Entities.Users;

namespace Collectify.Service.IServices.IUsers;

public interface IUserService
{
    Task<Response<UserResultDto>> AddAsync(UserCreationDto dto);
    Task<Response<PaginatedData<UserResultDto>>> GetAllAsync(
        PaginationParams @params, 
        Expression<Func<User, bool>> expression = null,
        string searchString = null);
    Task<Response<bool>> RemoveAsync(long id);
    Task<Response<UserResultDto>> GetAsync(long id);
    Task<Response<UserResultDto>> ModifyAsync(UserUpdateDto dto);
    Task<Response<UserResultDto>> ChangeEmailAsync(UserEmailUpdateDto dto);
    Task<Response<UserResultDto>> ChangePasswordAsync(UserPasswordUpdateDto dto);
}