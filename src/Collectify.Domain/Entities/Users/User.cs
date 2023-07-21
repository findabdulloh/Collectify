using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;

namespace Collectify.Domain.Entities.Users;

public class User : Auditable
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool Verified { get; set; }
    public long? ProfilePhotoId { get; set; }
    public bool IsBlocked { get; set; }
}