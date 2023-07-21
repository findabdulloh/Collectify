using AutoMapper;
using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Entities.Users;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Users;
using Collectify.Service.Extensions;
using Collectify.Service.IServices;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Collectify.Service.Services.Users;

public class UserService : IUserService
{
    private IMapper mapper;
    private IPhotoService photoService;
    private IRepository<User> userRepository;
    private IRepository<Photo> photoRepository;
    private IAuthorizationService authorizationService;
    private IAuthenticationService authenticationService;

    public UserService(IAuthorizationService authorizationService, IRepository<User> userRepository, IPhotoService photoService, IMapper mapper, IRepository<Photo> photoRepository, IAuthenticationService authenticationService)
    {
        this.mapper = mapper;
        this.photoService = photoService;
        this.userRepository = userRepository;
        this.photoRepository = photoRepository;
        this.authorizationService = authorizationService;
        this.authenticationService = authenticationService;
    }

    public async Task<Response<UserResultDto>> AddAsync(UserCreationDto dto)
    {
        var userWithEmail = await this.userRepository
            .SelectAsync(u => u.Email == dto.Email);
        if (userWithEmail is not null)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Email is already registered"
            };

        var userWithUsername = await this.userRepository
            .SelectAsync(u => u.Username == dto.Username);
        if (userWithUsername is not null)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Username is already registered"
            };

        var user = mapper.Map<User>(dto);
        user.Role = UserRole.User;

        var createdUser = await this.userRepository.InsertAsync(user);

        var photo = await this.photoRepository.InsertAsync(new Photo()
        {
            UserId = createdUser.Id
        });

        await this.userRepository.SaveAsync();

        var userEntity = await this.userRepository.SelectAsync(u => u.Id == createdUser.Id);

        userEntity.ProfilePhotoId = photo.Id;

        await this.userRepository.SaveAsync();
        await this.photoRepository.SaveAsync();

        await this.authenticationService.SendVerificationMail(createdUser.Id);


        var mappedUser = (await GetAsync(user.Id)).Result;

        return new Response<UserResultDto>
        {
            Result = mappedUser
        };
    }

    public async Task<Response<PaginatedData<UserResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<User, bool>> expression,
        string searchString)
    {
        var usersQuery = this.userRepository.SelectAll();
        if (expression is not null) usersQuery = usersQuery.Where(expression);
        
        if (!string.IsNullOrEmpty(searchString))
        {
            var searchKey = searchString.ToLower();

            usersQuery = usersQuery.Where(u =>
                   u.Name.ToLower().Contains(searchKey)
                || u.Username.ToLower().Contains(searchKey));
        }

        var totalCount = await usersQuery.CountAsync();
        var users = await usersQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedUsers = new List<UserResultDto>();

        foreach (var item in users)
        {
            var mappedUser = (await GetAsync(item.Id)).Result;

            mappedUsers.Add(mappedUser);
        }

        var paginatedData = new PaginatedData<UserResultDto>
        {
            Data = mappedUsers
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<UserResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<UserResultDto>> GetAsync(long id)
    {
        var user = await this.userRepository.SelectAsync(u => u.Id == id);

        if (user is null)
            return new Response<UserResultDto>
            {
                Code = 404,
                Message = "User is not found"
            };

        var mappedUser = mapper.Map<UserResultDto>(user);
        if (user.ProfilePhotoId is not null)
            mappedUser.ProfilePhoto = (await this.photoRepository
                .SelectAsync(p => p.Id == user.ProfilePhotoId));

        return new Response<UserResultDto>
        {
            Result = mappedUser
        };
    }

    public async Task<Response<bool>> RemoveAsync(long id)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null 
            || authorizedUser.Role != UserRole.Admin 
            && authorizedUser.Id != id)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var user = await this.userRepository.SelectAsync(u => u.Id == id);
        if (user is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "User is not found"
            };

        await this.userRepository.DeleteAsync(u => u.Id == id);
        await this.userRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<UserResultDto>> ModifyAsync(UserUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<UserResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var usersWithUserName = await this.userRepository
            .SelectAll(u => u.Username == dto.Username).AsNoTracking().ToListAsync();

        if (usersWithUserName.Count > 0 && usersWithUserName[0].Id != authorizedUser.Id)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Username is taken"
            };

        var user = await this.userRepository.SelectAsync(u => u.Id == authorizedUser.Id);

        user.Name = dto.Name;
        user.Username = dto.Username;
        user.UpdatedAt = DateTime.UtcNow;

        await this.userRepository.SaveAsync();

        var mappedUser = (await GetAsync(user.Id)).Result;

        return new Response<UserResultDto>
        {
            Result = mappedUser
        };
    }

    public async Task<Response<UserResultDto>> ChangeEmailAsync(UserEmailUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<UserResultDto>
            {
                Code = 403,
                Message = "Authorization error."
            };

        if (authorizedUser.Password != dto.Password)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Wrong password."
            };

        var userWithEmail = await this.userRepository
            .SelectAsync(u => u.Email == dto.Email);
        if (userWithEmail is not null)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Email is already registered."
            };

        var user = await this.userRepository
            .SelectAsync(u => u.Id == authorizedUser.Id);

        user.Email = dto.Email;
        user.Verified = false;
        user.UpdatedAt = DateTime.UtcNow;

        await this.userRepository.SaveAsync();

        var mappedUser = (await GetAsync(user.Id)).Result;

        return new Response<UserResultDto>
        {
            Result = mappedUser
        };
    }

    public async Task<Response<UserResultDto>> ChangePasswordAsync(UserPasswordUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<UserResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        if (authorizedUser.Password != dto.OldPassword)
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Incorrect old password"
            };

        if (string.IsNullOrEmpty(dto.Password))
            return new Response<UserResultDto>
            {
                Code = 400,
                Message = "Invalid password"
            };

        var user = await this.userRepository
            .SelectAsync(u => u.Id == authorizedUser.Id);

        user.Password = dto.Password;
        user.UpdatedAt = DateTime.UtcNow;

        await this.userRepository.SaveAsync();

        var mappedUser = (await GetAsync(user.Id)).Result;

        return new Response<UserResultDto>
        {
            Result = mappedUser
        };
    }
}