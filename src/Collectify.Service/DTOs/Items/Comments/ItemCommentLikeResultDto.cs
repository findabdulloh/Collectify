using Collectify.Domain.Commons;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.DTOs.Items.Comments;

public class ItemCommentLikeResultDto : Auditable
{
    public bool Seen { get; set; }

    public long UserId { get; set; }
    public UserResultDto User { get; set; }

    public long ItemCommentId { get; set; }
    public ItemCommentResultDto ItemComment { get; set; }

    public long ItemId { get; set; }
    public ItemResultDto Item { get; set; }
}