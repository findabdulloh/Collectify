using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Collections;

public class CollectionField
{
    public string Name { get; set; }
    public FieldType Type { get; set; }
}