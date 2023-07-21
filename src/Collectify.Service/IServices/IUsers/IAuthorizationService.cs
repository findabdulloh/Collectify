using Collectify.Domain.Entities.Users;
using Collectify.Domain.Enums;

namespace Collectify.Service.IServices.IUsers;

public interface IAuthorizationService
{
    Task<User> AuthorizeAsync(UserRole[] roles = null);
    Task<User> GetUserAsync();
}