using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Users;

public class UserResultDto : Auditable
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool Verified { get; set; }
    public bool IsBlocked { get; set; }
    public UserRole Role { get; set; }

    public Photo ProfilePhoto { get; set; }
}