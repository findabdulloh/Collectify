using AutoMapper;
using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Domain.Entities.Others;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Extensions;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Collectify.Service.Services.Items;

public class ItemCommentLikeService : IItemCommentLikeService
{
    private readonly IMapper mapper;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<ItemComment> itemCommentRepository;
    private readonly IRepository<ItemCommentLike> itemCommentLikeRepository;

    public ItemCommentLikeService(IRepository<ItemCommentLike> itemCommentLikeRepository, IRepository<ItemComment> itemCommentRepository, IAuthorizationService authorizationService, IMapper mapper)
    {
        this.mapper = mapper;
        this.authorizationService = authorizationService;
        this.itemCommentRepository = itemCommentRepository;
        this.itemCommentLikeRepository = itemCommentLikeRepository;
    }

    public async Task<Response<bool>> RemoveAsync(long itemCommentId)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var like = await this.itemCommentLikeRepository
            .SelectAsync(l => l.UserId == authorizedUser.Id
                && l.ItemCommentId == itemCommentId);

        if (like is null)
            return new Response<bool>
            {
                Code = 400,
                Message = "User has not liked"
            };

        await this.itemCommentLikeRepository
            .DeleteAsync(l => l.Id == like.Id);
        await this.itemCommentLikeRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<ItemCommentLikeResultDto>> AddAsync(long itemCommentId)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var comment = await this.itemCommentRepository.SelectAsync(i => i.Id == itemCommentId);

        if (comment is null)
            return new Response<ItemCommentLikeResultDto>
            {
                Code = 404,
                Message = "Comment is not found"
            };


        if (authorizedUser is null)
            return new Response<ItemCommentLikeResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var likeExists = await this.itemCommentLikeRepository
            .SelectAsync(l => l.UserId == authorizedUser.Id
                && l.ItemCommentId == itemCommentId)
            is not null;

        if (likeExists)
            return new Response<ItemCommentLikeResultDto>
            {
                Code = 400,
                Message = "Like already exists"
            };

        var like = new ItemCommentLike
        {
            ItemCommentId = itemCommentId,
            UserId = authorizedUser.Id,
            ReceiverId = comment.UserId
        };

        var createdLike = await this.itemCommentLikeRepository.InsertAsync(like);

        await this.itemCommentLikeRepository.SaveAsync();

        var mappedLike = (await GetAsync(like.UserId, like.ItemCommentId)).Result;

        return new Response<ItemCommentLikeResultDto>
        {
            Result = mappedLike
        };
    }

    public async Task<Response<PaginatedData<ItemCommentLikeResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemCommentLike, bool>> expression = null)
    {
        var likesQuery = this.itemCommentLikeRepository.SelectAll();

        if (expression is not null) likesQuery = likesQuery.Where(expression);

        var totalCount = await likesQuery.CountAsync();
        var likes = await likesQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedLikes = new List<ItemCommentLikeResultDto>();

        foreach (var like in likes)
        {
            var mappedLike = (await GetAsync(like.UserId, like.ItemCommentId)).Result;

            mappedLikes.Add(mappedLike);
        }

        var paginatedData = new PaginatedData<ItemCommentLikeResultDto>
        {
            Data = mappedLikes
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<ItemCommentLikeResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<ItemCommentLikeResultDto>> GetAsync(long userId, long itemCommentId)
    {
        var like = await this.itemCommentLikeRepository
            .SelectAsync(c => c.UserId == userId && c.ItemCommentId == itemCommentId);

        if (like is null)
            return new Response<ItemCommentLikeResultDto>
            {
                Code = 404,
                Message = "Like is not found"
            };

        var likeResultDto = mapper.Map<ItemCommentLikeResultDto>(like);

        return new Response<ItemCommentLikeResultDto>
        {
            Result = likeResultDto
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

        var likes = await this.itemCommentLikeRepository
            .SelectAll(l => l.ReceiverId == authorizedUser.Id && !l.Seen).ToListAsync();

        for (int i = 0; i < likes.Count; i++)
            likes[i].Seen = true;

        await this.itemCommentLikeRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }
}