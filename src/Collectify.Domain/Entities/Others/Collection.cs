using Collectify.Domain.Commons;
using Collectify.Domain.Entities.Users;
using Collectify.Domain.Enums;

namespace Collectify.Domain.Entities.Others;

public class Collection : Auditable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public string FieldsJson { get; set; }

    public long UserId { get; set; }
    public long? PhotoId { get; set; }
}