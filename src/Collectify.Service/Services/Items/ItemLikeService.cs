using AutoMapper;
using System.Linq.Expressions;
using Collectify.Service.Responses;
using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Others;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Collectify.Service.Services.Items;

public class ItemLikeService : IItemLikeService
{
    private readonly IMapper mapper;
    private readonly IRepository<Item> itemRepository;
    private readonly IRepository<ItemLike> itemLikeRepository;
    private readonly IAuthorizationService authorizationService;

    public ItemLikeService(IAuthorizationService authorizationService, IRepository<Item> itemRepository, IMapper mapper, IRepository<ItemLike> itemLikeRepository)
    {
        this.mapper = mapper;
        this.itemRepository = itemRepository;
        this.itemLikeRepository = itemLikeRepository;
        this.authorizationService = authorizationService;
    }

    public async Task<Response<ItemLikeResultDto>> AddAsync(long itemId)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<ItemLikeResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var item = await this.itemRepository.SelectAsync(i => i.Id == itemId);

        if (item is null)
            return new Response<ItemLikeResultDto>
            {
                Code = 404,
                Message = "Item is not found"
            };

        var likeExists = await this.itemLikeRepository
            .SelectAsync(l => l.UserId == authorizedUser.Id
                && l.ItemId == itemId)
            is not null;

        if (likeExists)
            return new Response<ItemLikeResultDto>
            {
                Code = 400,
                Message = "Like already exists"
            };

        var like = new ItemLike
        {
            ItemId = itemId,
            UserId = authorizedUser.Id,
            ReceiverId = item.UserId
        };

        var createdLike = await this.itemLikeRepository.InsertAsync(like);

        await this.itemLikeRepository.SaveAsync();

        var mappedLike = (await GetAsync(like.UserId, like.ItemId)).Result;

        return new Response<ItemLikeResultDto>
        {
            Result = mappedLike
        };
    }

    public async Task<Response<PaginatedData<ItemLikeResultDto>>> GetAllAsync(PaginationParams @params, Expression<Func<ItemLike, bool>> expression = null)
    {
        var likesQuery = this.itemLikeRepository.SelectAll();

        if (expression is not null) likesQuery = likesQuery.Where(expression);

        var totalCount = await likesQuery.CountAsync();
        var likes = await likesQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedLikes = new List<ItemLikeResultDto>();

        foreach (var like in likes)
        {
            var mappedLike = (await GetAsync(like.UserId, like.ItemId)).Result;

            mappedLikes.Add(mappedLike);
        }

        var paginatedData = new PaginatedData<ItemLikeResultDto>
        {
            Data = mappedLikes
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<ItemLikeResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<ItemLikeResultDto>> GetAsync(long userId, long itemId)
    {
        var like = await this.itemLikeRepository
            .SelectAsync(c => c.UserId == userId && c.ItemId == itemId);

        if (like is null)
            return new Response<ItemLikeResultDto>
            {
                Code = 404,
                Message = "Like is not found"
            };

        var likeResultDto = mapper.Map<ItemLikeResultDto>(like);

        return new Response<ItemLikeResultDto>
        {
            Result = likeResultDto
        };
    }

    public async Task<Response<bool>> RemoveAsync(long itemId)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var like = await this.itemLikeRepository
            .SelectAsync(l => l.UserId == authorizedUser.Id
                && l.ItemId == itemId);

        if (like is null)
            return new Response<bool>
            {
                Code = 400,
                Message = "User has not liked"
            };

        await this.itemLikeRepository
            .DeleteAsync(l => l.Id == like.Id);
        await this.itemLikeRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<bool>> SawAllReceivedLikesAsync()
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var likes = await this.itemLikeRepository
            .SelectAll(l => l.ReceiverId == authorizedUser.Id && !l.Seen).ToListAsync();

        for (int i = 0; i < likes.Count; i++)
            likes[i].Seen = true;

        await this.itemLikeRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }
}