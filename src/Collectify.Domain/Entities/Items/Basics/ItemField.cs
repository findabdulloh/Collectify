using Collectify.Domain.Commons;
using Collectify.Domain.Enums;

namespace Collectify.Domain.Entities.Items.Basics;

public class ItemField : Auditable
{
    public FieldType Type { get; set; }
    public string Name { get; set; }
    public string? Value { get; set; }

    public long ItemId { get; set; }
}