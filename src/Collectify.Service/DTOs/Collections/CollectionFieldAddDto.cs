using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Collections;

public class CollectionFieldAddDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public FieldType Type { get; set; }
}