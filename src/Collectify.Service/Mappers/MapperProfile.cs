using AutoMapper;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Domain.Entities.Others;
using Collectify.Domain.Entities.Users;
using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Service.DTOs.Items.Comments;
using Collectify.Service.DTOs.Items.Fields;
using Collectify.Service.DTOs.Photos;
using Collectify.Service.DTOs.Users;

namespace Collectify.Service.Mappers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserResultDto>().ReverseMap();
        CreateMap<User, UserCreationDto>().ReverseMap();

        CreateMap<Photo, PhotoUpdateDto>().ReverseMap();
        CreateMap<Photo, PhotoCreationDto>().ReverseMap();

        CreateMap<ItemField, ItemFieldCreationDto>().ReverseMap();

        CreateMap<ItemComment, ItemCommentResultDto>().ReverseMap();
        CreateMap<ItemComment, ItemCommentCreationDto>().ReverseMap();

        CreateMap<ItemCommentLike, ItemCommentLikeResultDto>().ReverseMap();

        CreateMap<Item, ItemResultDto>().ReverseMap();
        CreateMap<Item, ItemCreationDto>().ReverseMap();

        CreateMap<Item, ItemLikeResultDto>().ReverseMap();

        CreateMap<Collection, CollectionResultDto>().ReverseMap();
        CreateMap<Collection, CollectionCreationDto>().ReverseMap();
    }
}