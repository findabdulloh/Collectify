using Collectify.Domain.Commons;

namespace Collectify.Domain.Entities.Users;

public class UserToken : Auditable
{
    public long UserId { get; set; }
    public string Token { get; set; }
}