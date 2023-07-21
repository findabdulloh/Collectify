using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Users;

namespace Collectify.Web.Models;

public class HomeIndexViewModel
{
    public UserResultDto User { get; set; }
    public List<string> Tags { get; set; }
    public List<CollectionResultDto> TopCollections { get; set; }
    public PaginatedData<ItemResultDto> LatestItems { get; set; }
}