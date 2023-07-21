using Microsoft.AspNetCore.Http;

namespace Collectify.Service.IServices;

public interface ICloudService
{
    Task<string> GetUrlAsync(string fileName);
    Task<string> UploadAsync(IFormFile file);
    Task RemoveAsync(string fileName);
}