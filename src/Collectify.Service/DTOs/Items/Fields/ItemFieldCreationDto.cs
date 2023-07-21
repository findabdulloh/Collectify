using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Items.Fields;

public class ItemFieldCreationDto
{
    public FieldType Type { get; set; }
    public string Value { get; set; }
}