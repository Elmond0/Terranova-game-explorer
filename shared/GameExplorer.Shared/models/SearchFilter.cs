using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Shared.Models;

public class SearchFilter : IValidatableObject
{
	private static readonly string[] ValidOrders =
		["name", "released", "added", "created", "updated", "rating"];
	[MaxLength(128)]
	public string Query { get; set; } = "";
	[MaxLength(10)]
	public List<int> GenreIds { get; set; } = [];
	[MaxLength(10)]
	public List<int> PlatformIds { get; set; } = [];
	
	[Range(1950, 2100)]
	public int? FromYear { get; set; }
	
	[Range(1950, 2100)]
	public int? ToYear { get; set; }
	[MaxLength(10)]
	public List<int> DeveloperIds { get; set; } = [];
	[MaxLength(10)]
	public List<int> PublisherIds { get; set; } = [];
	public string? Ordering { get; set; }
	
	[Range(1, int.MaxValue)]
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
			var order = Ordering.TrimStart('-');
			if(!ValidOrders.Contains(order))
			{
				yield return new ValidationResult(
					"Ordering not valid.",
					[nameof(Ordering)]);
			}
		}
	}
}
