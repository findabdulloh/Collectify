using AutoMapper;
using Collectify.Domain.Enums;
using System.Linq.Expressions;
using Collectify.Service.IServices;
using Collectify.Service.Responses;
using Collectify.Data.IRepositories;
using Microsoft.EntityFrameworkCore;
using Collectify.Service.Extensions;
using Collectify.Domain.Entities.Others;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Dropbox.Api.Files;

namespace Collectify.Service.Services.Items;

public class ItemService : IItemService
{
    private readonly IMapper mapper;
    private readonly IPhotoService photoService;
    private readonly IRepository<Item> itemRepository;
    private readonly IRepository<Photo> photoRepository;
    private readonly IItemFieldService itemFieldService;
    private readonly IRepository<ItemLike> itemLikeRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<ItemField> itemFieldRepository;
    private readonly IRepository<Collection> collectionRepository;
    private readonly IRepository<ItemComment> itemCommentRepository;
    private readonly IRepository<ItemCommentLike> itemCommentLikeRepository;

    public ItemService(IAuthorizationService authorizationService, IRepository<Photo> photoRepository, IRepository<Item> itemRepository, IPhotoService photoService, IMapper mapper, IRepository<ItemField> itemFieldRepository, IRepository<ItemLike> itemLikeRepository, IRepository<ItemComment> itemCommentRepository, IRepository<ItemCommentLike> itemCommentLikeRepository, IRepository<Collection> collectionRepository, IItemFieldService itemFieldService)
    {
        this.mapper = mapper;
        this.photoService = photoService;
        this.itemRepository = itemRepository;
        this.photoRepository = photoRepository;
        this.itemFieldService = itemFieldService;
        this.itemLikeRepository = itemLikeRepository;
        this.itemFieldRepository = itemFieldRepository;
        this.authorizationService = authorizationService;
        this.collectionRepository = collectionRepository;
        this.itemCommentRepository = itemCommentRepository;
        this.itemCommentLikeRepository = itemCommentLikeRepository;
    }

    public async Task<Response<ItemResultDto>> AddAsync(ItemCreationDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var collection = await this.collectionRepository.SelectAsync(c => c.Id == dto.CollectionId);

        if (collection is null)
            return new Response<ItemResultDto>
            {
                Code = 404,
                Message = "Collection is not found"
            };

        if (authorizedUser is null
            || collection.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<ItemResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        for (int i = 0; i < dto.Tags.Count; i++)
        {
            dto.Tags[i] = dto.Tags[i].ToLower().Replace(" ", "");
            dto.Tags.RemoveAll(t => string.IsNullOrEmpty(t));
        }

        var entity = mapper.Map<Item>(dto);
        entity.UserId = collection.Id;
        entity.Tags = ConvertTagsToString(dto.Tags);

        var photo = await this.photoRepository.InsertAsync(new Photo
        {
            UserId = collection.UserId
        });

        entity.PhotoId = photo.Id;

        var createdEntity = await this.itemRepository.InsertAsync(entity);

        await this.itemFieldService.AddAllAsync(dto.Fields, createdEntity.Id);

        await this.itemRepository.SaveAsync();
        await this.photoRepository.SaveAsync();

        var mappedEntity = (await GetAsync(createdEntity.Id)).Result;

        return new Response<ItemResultDto>
        {
            Result = mappedEntity
        };
    }

    public async Task<Response<PaginatedData<ItemResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<Item, bool>> expression = null,
        string searchString = null)
    {
        var itemsQuery = this.itemRepository.SelectAll();

        if (expression is not null) itemsQuery = itemsQuery.Where(expression);

        if (!string.IsNullOrEmpty(searchString))
        {
            var searchKey = searchString.ToLower();

            var searchedItemIds = await this.itemFieldRepository
                .SelectAll(f => f.Value.ToString().ToLower().Contains(searchKey))
                .Select(s => new { s.ItemId }).ToListAsync();

            itemsQuery = itemsQuery.Where(u =>
                   u.Name.ToLower().Contains(searchKey)
                || u.Tags.Contains(searchKey)
                || searchedItemIds.Any(s => s.ItemId == u.Id));
        }

        var totalCount = await itemsQuery.CountAsync();
        var items = await itemsQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedItems = new List<ItemResultDto>();

        foreach (var item in items)
        {
            var mappedItem = (await GetAsync(item.Id)).Result;

            mappedItems.Add(mappedItem);
        }

        var paginatedData = new PaginatedData<ItemResultDto>
        {
            Data = mappedItems
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<ItemResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<ItemResultDto>> GetAsync(long id)
    {
        var item = await this.itemRepository.SelectAsync(u => u.Id == id);

        if (item is null)
            return new Response<ItemResultDto>
            {
                Code = 404,
                Message = "Not found"
            };

        var mappedItem = mapper.Map<ItemResultDto>(item);
        if (item.PhotoId is not null)
            mappedItem.Photo = (await this.photoService
                .GetAsync((long)item.PhotoId))
                .Result;

        mappedItem.LikeNumber = await this.itemLikeRepository
            .SelectAll(l => l.ItemId == mappedItem.Id)
            .CountAsync();

        mappedItem.CommentNumber = await this.itemCommentRepository
            .SelectAll(c => c.ItemId == mappedItem.Id)
            .CountAsync();

        mappedItem.Tags = ConvertStringToTags(item.Tags);

        var visitorUser = await this.authorizationService.GetUserAsync();

        if (visitorUser is not null)
            mappedItem.LikedByUser = await this.itemLikeRepository
                .SelectAsync(l => l.UserId == visitorUser.Id && l.ItemId == mappedItem.Id)
                is not null;

        return new Response<ItemResultDto>
        {
            Result = mappedItem
        };
    }

    public async Task<Response<ItemResultDto>> ModifyAsync(ItemUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var collection = await this.collectionRepository.SelectAsync(c => c.Id == dto.CollectionId);
        
        var entity = await this.itemRepository.SelectAsync(i => i.Id == dto.Id);

        if (entity is null)
            return new Response<ItemResultDto>
            {
                Code = 404,
                Message = "Item is not found"
            };

        if (collection is null)
            return new Response<ItemResultDto>
            {
                Code = 404,
                Message = "Collection is not found"
            };

        if ((authorizedUser is null
            || collection.UserId != authorizedUser.Id
            || entity.UserId != authorizedUser.Id)
            && authorizedUser.Role != UserRole.Admin)
            return new Response<ItemResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };


        for (int i = 0; i < dto.Tags.Count; i++)
        {
            dto.Tags[i] = dto.Tags[i].ToLower().Replace(" ", "");
            dto.Tags.RemoveAll(t => string.IsNullOrEmpty(t));
        }


        entity.Tags = ConvertTagsToString(dto.Tags);
        entity.Name = dto.Name;
        entity.CollectionId = dto.CollectionId;
        entity.UpdatedAt = DateTime.UtcNow;

        await this.itemRepository.SaveAsync();

        var mappedEntity = (await GetAsync(entity.Id)).Result;

        return new Response<ItemResultDto>
        {
            Result = mappedEntity
        };
    }

    public async Task<Response<bool>> RemoveAsync(long id)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var entity = await this.itemRepository.SelectAsync(i => i.Id == id);

        if (entity is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Item is not found"
            };

        if (authorizedUser is null
            || entity.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };


        await this.itemRepository.DeleteAsync(i => i.Id == id);
        await this.itemCommentRepository.DeleteManyAsync(c => c.ItemId == id);
        await this.itemLikeRepository.DeleteManyAsync(l => l.ItemId == id);
        await this.itemCommentLikeRepository.DeleteManyAsync(cl => cl.ItemId == id);

        await this.itemRepository.SaveAsync();
        await this.itemCommentRepository.SaveAsync();
        await this.itemLikeRepository.SaveAsync();
        await this.itemCommentLikeRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public string ConvertTagsToString(List<string> tags)
        => string.Join(" ", tags);

    public List<string> ConvertStringToTags(string tags)
        => tags.Split(' ').ToList();
}