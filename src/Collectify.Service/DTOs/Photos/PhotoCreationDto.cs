using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Collectify.Service.DTOs.Photos;

public class PhotoCreationDto
{
    public IFormFile File { get; set; }
    public long UserId { get; set; }
}