namespace GameExplorer.Shared.Models;

public class PagedResult<T>
{
	public List<T>? Items { get; set; }
	public int Total { get; set; }
	public int Page { get; set; }
	public bool IsLast { get; set; }
	public int PageSize { get; set; }
}