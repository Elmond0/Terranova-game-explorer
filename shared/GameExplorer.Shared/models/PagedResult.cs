namespace GameExplorer.Shared.Models;

public class PagedResult<T>
{
	public List<T>? Items { get; set; } = [];
	public int Total { get; set; }
	public int Page { get; set; }
	public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
	public int? Previous => Page > 1 ? Math.Min(Page - 1, TotalPages) : null;
	public int? Next => Page < TotalPages ? Page + 1 : null;
	public int PageSize { get; set; }
}