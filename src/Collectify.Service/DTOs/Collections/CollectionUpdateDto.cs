using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Collections;

public class CollectionUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
}