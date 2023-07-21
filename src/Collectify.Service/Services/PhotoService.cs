using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Photos;
using Collectify.Service.IServices;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Responses;

namespace Collectify.Service.Services;

public class PhotoService : IPhotoService
{
    private readonly ICloudService cloudService;
    private readonly IRepository<Photo> photoRepository;
    private readonly IAuthorizationService authorizationService;

    public PhotoService(IRepository<Photo> photoRepository, ICloudService cloudService, IAuthorizationService authorizationService)
    {
        this.cloudService = cloudService;
        this.photoRepository = photoRepository;
        this.authorizationService = authorizationService;
    }

    public async Task<Response<Photo>> GetAsync(long id)
    {   
        var photo = await this.photoRepository.SelectAsync(p => p.Id == id);

        if (photo is not null)
            return new Response<Photo>
            {
                Result = photo
            };

        return new Response<Photo>
        {
            Code = 400,
            Message = "Photo is not foud"
        };
    }

    public async Task<Response<Photo>> ModifyAsync(PhotoUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        var photo = await this.photoRepository.SelectAsync(p => p.Id == dto.Id);

        if (photo is null)
            return new Response<Photo>
            {
                Code = 400,
                Message = "Photo is not found"
            };

        if (authorizedUser is null 
            || photo.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<Photo>
            {
                Code = 405,
                Message = "Authorization error"
            };


        var oldPhotoName = photo.Name;

        photo.Name = await this.cloudService.UploadAsync(dto.File);
        photo.Url = await this.cloudService.GetUrlAsync(photo.Name);

        await this.cloudService.RemoveAsync(oldPhotoName);

        await this.photoRepository.SaveAsync();

        return new Response<Photo>
        {
            Result = photo
        };
    }

    public async Task<Response<bool>> RemoveAsync(long id)
    {
        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        var photo = await this.photoRepository.SelectAsync(p => p.Id == id);

        if (photo is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Photo is not found"
            };

        if (authorizedUser is null 
            || photo.UserId != authorizedUser.Id 
            && authorizedUser.Role != UserRole.Admin)
            return new Response<bool>
            {
                Code = 405,
                Message = "Authorization error"
            };

        await this.cloudService.RemoveAsync(photo.Name);

        photo.Name = null;
        photo.Url = null;

        await this.photoRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }
}