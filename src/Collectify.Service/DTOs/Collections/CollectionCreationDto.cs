using Collectify.Domain.Enums;

namespace Collectify.Service.DTOs.Collections;

public class CollectionCreationDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public Dictionary<string, FieldType> Fields { get; set; }
    public List<CollectionField> FieldsList { get; set; }

    public CollectionCreationDto() 
    {
        FieldsList = new List<CollectionField>();
        Fields = new Dictionary<string, FieldType>();
    }
}