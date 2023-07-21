namespace Collectify.Service.DTOs.Paginations;

public class PaginatedData<TResult>
{
    public List<TResult> Data { get; set; }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }

    public void InitializePaginationMetaData(PaginationParams @params, int totalCount)
    {
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(TotalCount / (double)@params.PageSize);
        CurrentPage = @params.PageIndex;
        HasPrevious = CurrentPage > 1;
        HasNext = CurrentPage < TotalPages;
    }
}