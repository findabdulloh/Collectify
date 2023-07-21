using AutoMapper;
using Collectify.Domain.Enums;
using System.Linq.Expressions;
using Collectify.Service.IServices;
using Collectify.Service.Responses;
using Collectify.Data.IRepositories;
using Collectify.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using Collectify.Domain.Entities.Others;
using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.IServices.IUsers;
using Collectify.Domain.Entities.Items.Basics;
using System.Collections;
using System.Text.Json;

namespace Collectify.Service.Services;

public class CollectionService : ICollectionService
{
    private readonly IMapper mapper;
    private readonly IPhotoService photoService;
    private readonly IRepository<Item> itemRepository;
    private readonly IRepository<Photo> photoRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<Collection> collectionRepository;

    public CollectionService(IRepository<Collection> collectionRepository, IAuthorizationService authorizationService, IPhotoService photoService, IMapper mapper, IRepository<Photo> photoRepository, IRepository<Item> itemRepository)
    {
        this.mapper = mapper;
        this.photoService = photoService;
        this.itemRepository = itemRepository;
        this.photoRepository = photoRepository;
        this.authorizationService = authorizationService;
        this.collectionRepository = collectionRepository;
    }

    public async Task<Response<CollectionResultDto>> AddAsync(CollectionCreationDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<CollectionResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var entity = mapper.Map<Collection>(dto);

        var photo = await this.photoRepository.InsertAsync(new Photo
        {
            UserId = authorizedUser.Id
        });

        await this.photoRepository.SaveAsync();

        entity.UserId = authorizedUser.Id;
        entity.PhotoId = photo.Id;
        entity.FieldsJson = GetJson(dto.Fields);

        var createdEntity = await this.collectionRepository.InsertAsync(entity);


        await this.collectionRepository.SaveAsync();

        var mappedEntity = (await GetAsync(createdEntity.Id)).Result;

        return new Response<CollectionResultDto>
        {
            Result = mappedEntity
        };
    }

    public async Task<Response<bool>> RemoveAsync(long id)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var collection = await this.collectionRepository.SelectAsync(u => u.Id == id);
        if (collection is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Not found"
            };

        if (authorizedUser is null
            || authorizedUser.Role != UserRole.Admin
            && authorizedUser.Id != collection.UserId)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };


        await this.collectionRepository.DeleteAsync(u => u.Id == id);
        await this.collectionRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<PaginatedData<CollectionResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<Collection, bool>> expression = null,
        string searchString = null)
    {
        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        var collectionsQuery = this.collectionRepository.SelectAll();

        if (expression is not null) collectionsQuery = collectionsQuery.Where(expression);

        if (!string.IsNullOrEmpty(searchString))
        {
            var searchKey = searchString.ToLower();

            collectionsQuery = collectionsQuery.Where(u =>
                   u.Name.ToLower().Contains(searchKey)
                || u.Description.ToLower().Contains(searchKey)
                || u.Category.ToString().ToLower().Contains(searchKey));
        }

        var totalCount = await collectionsQuery.CountAsync();
        var collections = await collectionsQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedCollections = new List<CollectionResultDto>();

        foreach (var item in collections)
        {
            var mappedCollection = (await GetAsync(item.Id)).Result;

            mappedCollections.Add(mappedCollection);
        }

        var paginatedData = new PaginatedData<CollectionResultDto>
        {
            Data = mappedCollections
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<CollectionResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<CollectionResultDto>> GetAsync(long id)
    {
        var collection = await this.collectionRepository.SelectAsync(u => u.Id == id);

        if (collection is null)
            return new Response<CollectionResultDto>
            {
                Code = 404,
                Message = "Not found"
            };

        var mappedCollection = mapper.Map<CollectionResultDto>(collection);
        if (collection.PhotoId is not null)
            mappedCollection.Photo = (await this.photoService
                .GetAsync((long)collection.PhotoId))
                .Result;

        mappedCollection.ItemCount = await this.itemRepository
            .SelectAll(i => i.CollectionId == collection.Id).CountAsync();

        mappedCollection.Fields = GetDictionary(collection.FieldsJson);

        return new Response<CollectionResultDto>
        {
            Result = mappedCollection
        };
    }

    public async Task<Response<CollectionResultDto>> ModifyAsync(CollectionUpdateDto dto)
    {
        var collection = await this.collectionRepository.SelectAsync(u => u.Id == dto.Id);

        if (collection is null)
            return new Response<CollectionResultDto>
            {
                Code = 404,
                Message = "Not found"
            };

        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        if (authorizedUser is null
            || authorizedUser.Id != collection.UserId
            && authorizedUser.Role != UserRole.Admin)
        {
            return new Response<CollectionResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };
        }

        collection.Name = dto.Name;
        collection.Description = dto.Description;
        collection.Category = dto.Category;
        collection.UpdatedAt = DateTime.UtcNow;

        await this.collectionRepository.SaveAsync();

        var mappedCollection = (await GetAsync(collection.Id)).Result;

        return new Response<CollectionResultDto>
        {
            Result = mappedCollection
        };
    }

    public async Task<Response<CollectionResultDto>> AddFieldAsync(CollectionFieldAddDto dto)
    {
        var collection = await this.collectionRepository.SelectAsync(u => u.Id == dto.Id);

        if (collection is null)
            return new Response<CollectionResultDto>
            {
                Code = 404,
                Message = "Not found"
            };

        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        if (authorizedUser is null
            || authorizedUser.Id != collection.UserId
            && authorizedUser.Role != UserRole.Admin)
        {
            return new Response<CollectionResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };
        }

        var fields = GetDictionary(collection.FieldsJson);

        var fieldWithName = fields.ContainsKey(dto.Name);

        if (fieldWithName)
            return new Response<CollectionResultDto>
            {
                Code = 400,
                Message = "The fields with this name is existing"
            };

        fields.Add(dto.Name, dto.Type);

        var json = GetJson(fields);

        collection.FieldsJson = json;

        await this.collectionRepository.SaveAsync();

        var mappedCollection = (await GetAsync(collection.Id)).Result;

        return new Response<CollectionResultDto>
        {
            Result = mappedCollection
        };
    }

    public async Task<Response<CollectionResultDto>> RemoveFieldAsync(CollectionFieldRemoveDto dto)
    {
        var collection = await this.collectionRepository.SelectAsync(u => u.Id == dto.Id);

        if (collection is null)
            return new Response<CollectionResultDto>
            {
                Code = 404,
                Message = "Not found"
            };

        var authorizedUser = await this.authorizationService.AuthorizeAsync();

        if (authorizedUser is null
            || authorizedUser.Id != collection.UserId
            && authorizedUser.Role != UserRole.Admin)
        {
            return new Response<CollectionResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };
        }

        var fields = GetDictionary(collection.FieldsJson);

        var fieldWithName = fields.ContainsKey(dto.Name);

        if (!fieldWithName)
            return new Response<CollectionResultDto>
            {
                Code = 404,
                Message = "Not found"
            };


        fields.Remove(dto.Name);

        var json = GetJson(fields);

        collection.FieldsJson = json;

        await this.collectionRepository.SaveAsync();

        var mappedCollection = (await GetAsync(collection.Id)).Result;

        return new Response<CollectionResultDto>
        {
            Result = mappedCollection
        };
    }

    public string GetJson(Dictionary<string, FieldType> dictionary)
        => dictionary is not null ? JsonSerializer.Serialize(dictionary) : "";

    public Dictionary<string, FieldType> GetDictionary(string json)
        => JsonSerializer.Deserialize<Dictionary<string, FieldType>>(json);
}