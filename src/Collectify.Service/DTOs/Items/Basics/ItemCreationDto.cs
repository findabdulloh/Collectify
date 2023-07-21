using Collectify.Service.DTOs.Items.Fields;
using Collectify.Service.DTOs.Photos;

namespace Collectify.Service.DTOs.Items.Basics;

public class ItemCreationDto
{
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public long CollectionId { get; set; }
    public List<ItemFieldCreationDto> Fields { get; set; }
}