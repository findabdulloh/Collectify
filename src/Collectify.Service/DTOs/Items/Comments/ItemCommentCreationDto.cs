using Collectify.Domain.Entities.Items;
using Collectify.Domain.Entities;

namespace Collectify.Service.DTOs.Items.Comments;

public class ItemCommentCreationDto
{
    public string Text { get; set; }
    public long ItemId { get; set; }
}