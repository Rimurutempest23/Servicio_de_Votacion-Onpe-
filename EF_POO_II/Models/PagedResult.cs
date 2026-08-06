namespace EF_POO_II.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalRegistros { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalRegistros / (double)PageSize));
}
