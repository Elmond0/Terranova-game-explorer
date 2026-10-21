using System.Dynamic;

namespace GameExplorer.Shared.Models;

class SearchFilter
{
	public string? Text { get; set; }
	public List<Genre>? Genres { get; set; }
	public List<Platform>? Platforms { get; set; }
	public int? FromYear { get; set; }
	public int? ToYear { get; set; }
	public int? Year { get; set; }
	public int? MinMetacritic { get; set; }
	public List<Studio>? Developers { get; set; }
	public List<Studio>? Publishers { get; set; }
	public List<Store>? Stores { get; set; } 
}