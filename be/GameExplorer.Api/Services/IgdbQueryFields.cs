using GameExplorer.Api.Services.IgdbDtos;
namespace GameExplorer.Api.Services;

public record IgdbQueryFields()
{
    private static readonly long Today = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow)) ?? 0;
    private static readonly long LastMonth = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))) ?? 0;
    private static readonly long NextYear = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))) ?? 0;
    
    public const string List =
        "fields id, name, slug, rating, first_release_date, cover.*, genres.*, platforms.*;";	    
    
    public const string Details =
        "fields id, name, slug, rating, first_release_date, cover.*, genres.*, platforms.*, summary, storyline, involved_companies.company.*, websites.*, screenshots.*;";	    
    
    public const string TopRated =
        $"{List}\n" +
        $"where rating_count > 100 & first_release_date > 1577836800;\n" +
        $"sort rating desc;";	   
    
    public static readonly string Upcoming =
        $"{List}\n" +
        $"where rating_count > 100 & first_release_date > {Today} & first_release_date < {NextYear} ;\n" +
        $"sort rating desc;";	    
    
    public static readonly string Recent =
        $"{List}\n" +
        $"where rating_count > 100 & first_release_date < {Today} & first_release_date >= {LastMonth} ;\n" +
        $"sort rating desc;";
};