using Collectify.Domain.Commons;

namespace Collectify.Domain.Entities.Others;

public class Photo : Auditable
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public long UserId { get; set; }
}