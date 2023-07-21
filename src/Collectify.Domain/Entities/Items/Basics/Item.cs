using Collectify.Domain.Commons;

namespace Collectify.Domain.Entities.Items.Basics;

public class Item : Auditable
{
    public string Name { get; set; }
    public string Tags { get; set; }

    public long UserId { get; set; }
    public long CollectionId { get; set; }
    public long? PhotoId { get; set; }
}