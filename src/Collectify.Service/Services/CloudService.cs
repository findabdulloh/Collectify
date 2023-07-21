using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;
using Collectify.Service.IServices;
using Microsoft.Extensions.Configuration;

namespace Collectify.Service.Services;

public class CloudService : ICloudService
{
    private readonly IConfiguration configurations;
    private string dropBoxKey;

    public CloudService(IConfiguration configurations)
    {
        this.configurations = configurations;
        this.dropBoxKey = configurations["DropBoxKey"];
    }

    public async Task RemoveAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return;

        var dbx = new DropboxClient(dropBoxKey);

        var path = "/Photos/" + fileName;

        await dbx.Files.DeleteV2Async(fileName);
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        string fileExtension = Path.GetExtension(file.FileName);

        var dbx = new DropboxClient(dropBoxKey);

        string fileNewName = Guid.NewGuid().ToString("N") + fileExtension;

        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        var uploadPath = $"/Photos/" + fileNewName;

        using (var mem = new MemoryStream(fileBytes))
        {
            var updated = await dbx.Files.UploadAsync(uploadPath, WriteMode.Overwrite.Instance, body: mem);
        }

        return fileNewName;
    }

    public async Task<string> GetUrlAsync(string fileName)
    {
        var dbx = new DropboxClient(dropBoxKey);
        var filePath = "/Photos/" + fileName;

        var sharedLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(filePath);

        return sharedLink.Url;
    }
}