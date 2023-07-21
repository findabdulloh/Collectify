using Collectify.Domain.Commons;
using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Items.Fields;

public class ItemFieldUpdateDto
{
    public long Id { get; set; }
    public string Value { get; set; }
}