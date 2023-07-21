using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Users;

namespace Collectify.Domain.Entities.Items.ItemComments;

public class ItemCommentLike : Auditable
{
    public bool Seen { get; set; }

    public long UserId { get; set; }
    public long ItemCommentId { get; set; }
    public long ItemId { get; set; }
    public long ReceiverId { get; set; }
}