using Microsoft.AspNetCore.Http;

namespace Collectify.Service.DTOs.Photos;

public class PhotoUpdateDto
{
    public long Id { get; set; }
    public IFormFile File { get; set; }
}