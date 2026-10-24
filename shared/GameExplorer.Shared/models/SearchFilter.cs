namespace GameExplorer.Shared.Models;

public class SearchFilter
{
	public string? Text { get; set; }
	public List<int> GenreIds { get; set; } = [];
	public List<int> PlatformIds { get; set; } = [];
	public int? FromYear { get; set; }
	public int? ToYear { get; set; }
	public int? MinMetacritic { get; set; }
	public List<int> DeveloperIds { get; set; } = [];
	public List<int> PublisherIds { get; set; } = [];
	public List<int> StoreIds { get; set; } = [];
	// Valori RAWG: name, released, rating, metacritic. Con "-" davanti l'ordine è decrescente
	public string? Ordering { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
