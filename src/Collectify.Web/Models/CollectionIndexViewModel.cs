using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Users;

namespace Collectify.Web.Models;

public class CollectionIndexViewModel
{
    public int PageIndex { get; set; }
    public UserResultDto CollectionsUser { get; set; }
    public UserResultDto Visitor { get; set; }
    public PaginatedData<CollectionResultDto> Collections { get; set; }
}