using Collectify.Domain.Commons;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.DTOs.Items.Comments;

public class ItemCommentResultDto : Auditable
{
    public string Text { get; set; }
    public long ItemId { get; set; }
    public bool Seen { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByUser { get; set; }
    
    public long UserId { get; set; }
    public UserResultDto User { get; set; }
}