using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Items.Fields;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.DTOs.Items.Basics;

public class ItemResultDto : Auditable
{
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public List<ItemField> Fields { get; set; }
    public int CommentNumber { get; set; }
    public int LikeNumber { get; set; }
    public bool LikedByUser { get; set; }

    public Photo Photo { get; set; }

    public long UserId { get; set; }
    public UserResultDto User { get; set; }

    public long CollectionId { get; set; }
    public CollectionResultDto Collection { get; set; }
}