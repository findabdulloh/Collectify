using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Photos;
using Microsoft.AspNetCore.Http;

namespace Collectify.Service.DTOs.Users;

public class UserCreationDto
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}