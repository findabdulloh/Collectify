using Collectify.Domain.Commons;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.DTOs.Items.Basics;

public class ItemLikeResultDto : Auditable
{
    public bool Seen { get; set; }

    public long ItemId { get; set; }
    public ItemResultDto Item { get; set; }

    public long UserId { get; set; }
    public UserResultDto User { get; set; }
}