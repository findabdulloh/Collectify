namespace Collectify.Service.DTOs.Paginations;

public class PaginationParams
{
    public int PageSize { get; set; } = 10;
    public int PageIndex { get; set; } = 1;
}