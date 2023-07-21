using Collectify.Service.DTOs.Users;
using Collectify.Service.Responses;

namespace Collectify.Service.IServices.IUsers;

public interface IAuthenticationService
{
    Task<Response<bool>> CheckSentVerificationMailAsync();
    Task<Response<bool>> VerificateAsync(string token);
    Task<Response<bool>> SendVerificationMail(long userId);
    Task<Response<string>> AuthenticateAsync(UserLoginDto dto);
}