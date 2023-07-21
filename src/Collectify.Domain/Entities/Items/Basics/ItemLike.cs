using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Users;

namespace Collectify.Domain.Entities.Items.Basics;

public class ItemLike : Auditable
{
    public bool Seen { get; set; }

    public long ItemId { get; set; }
    public long UserId { get; set; }
    public long ReceiverId { get; set; }
}