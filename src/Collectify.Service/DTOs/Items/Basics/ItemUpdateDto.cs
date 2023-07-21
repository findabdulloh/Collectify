namespace Collectify.Service.DTOs.Items.Basics;

public class ItemUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public long CollectionId { get; set; }
}