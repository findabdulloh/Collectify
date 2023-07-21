using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Users;
using Collectify.Domain.Entities.Items.Basics;
using System.Globalization;

namespace Collectify.Domain.Entities.Items.ItemComments;

public class ItemComment : Auditable
{
    public string Text { get; set; }
    public bool Seen { get; set; }

    public long UserId { get; set; }
    public long ItemId { get; set; }
    public long ReceiverId { get; set; }
}