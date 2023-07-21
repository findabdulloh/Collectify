using AutoMapper;
using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Domain.Enums;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.Extensions;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Collectify.Service.Services.Items;

public class ItemCommentService : IItemCommentService
{
    private readonly IMapper mapper;
    private readonly IRepository<Item> itemRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<ItemComment> itemCommentRepository;
    private readonly IRepository<ItemCommentLike> itemCommentLikeRepository;

    public ItemCommentService(IRepository<ItemComment> itemCommentRepository, IAuthorizationService authorizationService, IRepository<Item> itemRepository, IMapper mapper, IRepository<ItemCommentLike> itemCommentLikeRepository)
    {
        this.mapper = mapper;
        this.itemRepository = itemRepository;
        this.authorizationService = authorizationService;
        this.itemCommentRepository = itemCommentRepository;
        this.itemCommentLikeRepository = itemCommentLikeRepository;
    }

    public async Task<Response<ItemCommentResultDto>> AddAsync(ItemCommentCreationDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var item = await this.itemRepository.SelectAsync(i => i.Id == dto.ItemId);

        if (item is null)
            return new Response<ItemCommentResultDto>
            {
                Code = 404,
                Message = "Item is not found"
            };

        if (authorizedUser is null)
            return new Response<ItemCommentResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var entity = new ItemComment
        {
            ItemId = dto.ItemId,
            Text = dto.Text,
            UserId = authorizedUser.Id,
            ReceiverId = item.UserId
        };

        var createdEntity = await this.itemCommentRepository.InsertAsync(entity);

        await this.itemCommentRepository.SaveAsync();

        var mappedEntity = (await GetAsync(createdEntity.Id)).Result;

        return new Response<ItemCommentResultDto>
        {
            Result = mappedEntity
        };
    }

    public async Task<Response<PaginatedData<ItemCommentResultDto>>> GetAllAsync(
        PaginationParams @params,
        Expression<Func<ItemComment, bool>> expression = null,
        string searchString = null)
    {
        var commentsQuery = this.itemCommentRepository.SelectAll();

        if (expression is not null) commentsQuery = commentsQuery.Where(expression);

        if (!string.IsNullOrEmpty(searchString))
        {
            var searchKey = searchString.ToLower();

            commentsQuery = commentsQuery.Where(u =>
                   u.Text.ToLower().Contains(searchKey));
        }

        var totalCount = await commentsQuery.CountAsync();
        var comments = await commentsQuery
            .Paginate(@params)
            .ToListAsync();

        var mappedComments = new List<ItemCommentResultDto>();

        foreach (var comment in comments)
        {
            var mappedComment = (await GetAsync(comment.Id)).Result;

            mappedComments.Add(mappedComment);
        }

        var paginatedData = new PaginatedData<ItemCommentResultDto>
        {
            Data = mappedComments
        };
        paginatedData.InitializePaginationMetaData(@params, totalCount);

        return new Response<PaginatedData<ItemCommentResultDto>>
        {
            Result = paginatedData
        };
    }

    public async Task<Response<ItemCommentResultDto>> GetAsync(long id)
    {
        var comment = await this.itemCommentRepository.SelectAsync(c => c.Id == id);

        if (comment is null)
            return new Response<ItemCommentResultDto>
            {
                Code = 404,
                Message = "Comment is not found"
            };

        var mappedComment = mapper.Map<ItemCommentResultDto>(comment);

        mappedComment.LikeCount = await this.itemCommentLikeRepository
            .SelectAll(l => l.ItemCommentId == mappedComment.Id)
            .CountAsync();

        var visitorUser = await this.authorizationService.GetUserAsync();

        if (visitorUser is not null)
            mappedComment.LikedByUser =
                await this.itemCommentLikeRepository
                    .SelectAsync(l => l.UserId == visitorUser.Id
                        && l.ItemCommentId == mappedComment.Id)
                is not null;

        return new Response<ItemCommentResultDto>
        {
            Result = mappedComment
        };
    }

    public async Task<Response<ItemCommentResultDto>> ModifyAsync(ItemCommentUpdateDto dto)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var comment = await this.itemCommentRepository
            .SelectAsync(c => c.Id == dto.Id);

        if (comment is null)
            return new Response<ItemCommentResultDto>
            {
                Code = 404,
                Message = "Comment is not found"
            };

        if (authorizedUser is null
            || comment.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<ItemCommentResultDto>
            {
                Code = 403,
                Message = "Authorization error"
            };

        comment.Text = dto.Text;
        comment.UpdatedAt = DateTime.UtcNow;

        await this.itemCommentRepository.SaveAsync();

        var mappedEntity = (await GetAsync(comment.Id)).Result;

        return new Response<ItemCommentResultDto>
        {
            Result = mappedEntity
        };
    }

    public async Task<Response<bool>> RemoveAsync(long id)
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        var comment = await this.itemCommentRepository
            .SelectAsync(c => c.Id == id);

        if (comment is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Comment is not found"
            };

        if (authorizedUser is null
            || comment.UserId != authorizedUser.Id
            && authorizedUser.Role != UserRole.Admin)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        await this.itemCommentRepository.DeleteAsync(c => c.Id == id);
        await this.itemCommentLikeRepository.DeleteManyAsync(l => l.ItemCommentId == id);

        await this.itemCommentLikeRepository.SaveAsync();
        await this.itemCommentRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<bool>> SawAllReceivedCommentsAsync()
    {
        var authorizedUser = await this.authorizationService
            .AuthorizeAsync();

        if (authorizedUser is null)
            return new Response<bool>
            {
                Code = 403,
                Message = "Authorization error"
            };

        var comments = await this.itemCommentRepository
            .SelectAll(l => l.ReceiverId == authorizedUser.Id && !l.Seen).ToListAsync();

        for (int i = 0; i < comments.Count; i++)
            comments[i].Seen = true;

        await this.itemCommentRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }
}