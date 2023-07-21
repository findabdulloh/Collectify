using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Items.Fields;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Extensions;
using Collectify.Service.IServices;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Collectify.Service.Services.Items;

public class ItemFieldService : IItemFieldService
{
    private readonly IRepository<Item> itemRepository;
    private readonly ICollectionService collectionService;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<ItemField> itemFieldRepository;
    private readonly IRepository<Collection> collectionRepository;
    private readonly IRepository<ItemComment> itemCommentRepository;

    public ItemFieldService(IRepository<ItemComment> itemCommentRepository, IRepository<Collection> collectionRepository, IAuthorizationService authorizationService, IRepository<Item> itemRepository, ICollectionService collectionService)
    {
        this.itemRepository = itemRepository;
        this.collectionService = collectionService;
        this.authorizationService = authorizationService;
        this.collectionRepository = collectionRepository;
        this.itemCommentRepository = itemCommentRepository;
    }

    public async Task<Response<bool>> AddAllAsync(
        List<ItemFieldCreationDto> fields,
        long itemId)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var item = await this.itemRepository.SelectAsync(i => i.Id == itemId);

        if (item is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Item is not found"
            };

        var collection = (await this.collectionService
            .GetAsync(item.CollectionId)).Result;

        var fieldsList = collection.Fields.ToList();

        for (var i = 0; i < fields.Count; i++)
        {
            if (i >= fieldsList.Count)
                break;

            var field = fields[i];

            var fieldForCreation = new ItemField
            {
                Name = fieldsList[i].Key,
                Type = fieldsList[i].Value
            };

            if (field.Type == fieldForCreation.Type)
            {
                fieldForCreation.Value = field.Value;
            }

            await this.itemFieldRepository.InsertAsync(fieldForCreation);
        }


        await this.itemFieldRepository.SaveAsync();


        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<PaginatedData<ItemField>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemField, bool>> expression = null)
    {
        var fieldsQuery = this.itemFieldRepository.SelectAll();

        if (expression is not null) fieldsQuery = fieldsQuery.Where(expression);

        var totalCount = await fieldsQuery.CountAsync();
        var fields = await fieldsQuery
            .Paginate(@params)
            .ToListAsync();

        var paginatedData = new PaginatedData<ItemField>
        {
            Data = fields
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<ItemField>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<ItemField>> GetAsync(long id)
    {
        var field = await this.itemFieldRepository.SelectAsync(c => c.Id == id);

        if (field is null)
            return new Response<ItemField>
            {
                Code = 404,
                Message = "Not found"
            };

        return new Response<ItemField>
        {
            Result = field
        };
    }

    public async Task<Response<ItemField>> ModifyAsync(ItemFieldUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var field = await this.itemFieldRepository
            .SelectAsync(c => c.Id == dto.Id);

        if (field is null)
            return new Response<ItemField>
            {
                Code = 404,
                Message = "not found"
            };

        var item = await this.itemRepository.SelectAsync(i => i.Id == field.ItemId);

        if (authorizedUser is null
            || item.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<ItemField>
            {
                Code = 403,
                Message = "Authorization error"
            };


        if (field.Type == FieldType.Bool && dto.Value.GetType().Name == "Boolean"
             || field.Type == FieldType.Integer && dto.Value.GetType().Name == "Int32"
             || field.Type == FieldType.Date && dto.Value.GetType().Name == "DateTime"
             || (field.Type == FieldType.MultilineString || field.Type == FieldType.String)
             && field.Value.GetType().Name == "String")
        {
            field.Value = dto.Value;
            field.UpdatedAt = DateTime.UtcNow;

            await this.itemCommentRepository.SaveAsync();

            return new Response<ItemField>
            {
                Result = field
            };
        }

        return new Response<ItemField>
        {
            Code = 400,
            Message = "Type error"
        };
    }
}