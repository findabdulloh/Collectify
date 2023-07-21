using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.DTOs.Collections;

public class CollectionResultDto : Auditable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public Dictionary<string, FieldType> Fields { get; set; }
    public int ItemCount { get; set; }

    public long UserId { get; set; }
    public UserResultDto User { get; set; }

    public Photo Photo { get; set; }
}