using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Shared.Models;

public class SearchFilter : IValidatableObject
{
	private static readonly string[] ValidOrders =
		["name", "released", "added", "created", "updated", "rating", "metacritic"];
	public string? Query { get; set; }
	public List<int> GenreIds { get; set; } = [];
	public List<int> PlatformIds { get; set; } = [];

	[Range(1950, 2100)]
	public int? FromYear { get; set; }

	[Range(1950, 2100)]
	public int? ToYear { get; set; }

	[Range(0, 100)]
	public int? MinMetacritic { get; set; }

	public List<int> DeveloperIds { get; set; } = [];
	public List<int> PublisherIds { get; set; } = [];
	public List<int> StoreIds { get; set; } = [];
	// Valori RAWG: name, released, rating, metacritic. Con "-" davanti l'ordine è decrescente
	public string? Ordering { get; set; }

	[Range(0, int.MaxValue)]
	public int Page { get; set; } = 1;

	[Range(1, 40)]
	public int PageSize { get; set; } = 20;

	public IEnumerable<ValidationResult> Validate( ValidationContext vc )
	{
		if (FromYear.HasValue && ToYear.HasValue && FromYear > ToYear)
		{
			yield return new ValidationResult(
				"FromYear must be less than or equal to ToYear.",
				[nameof(FromYear), nameof(ToYear)]);
		}

		if (!string.IsNullOrWhiteSpace(Ordering))
		{
			string order = Ordering.TrimStart('-');
			if(!ValidOrders.Contains(order))
			{
				yields return new ValidationResult(
					"Ordinamento non valido.",
					[nameof(Ordering)]);
			}
		}
	}
}
