using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Items.Basics;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Users;

namespace Collectify.Web.Models;

public class CollectionViewModel
{
    public UserResultDto Visitor { get; set; }
    public CollectionResultDto Collection { get; set; }
    public PaginatedData<ItemResultDto> Items { get; set; }
}