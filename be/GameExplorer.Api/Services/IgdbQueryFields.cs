using GameExplorer.Api.Services.IgdbDtos;
namespace GameExplorer.Api.Services;

public record IgdbQueryFields()
{
    private static readonly long Today = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow)) ?? 0;
    private static readonly long LastMonth = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))) ?? 0;
    private static readonly long NextYear = IgdbMapper.ToUnix(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))) ?? 0;
    
    public const string TopRated = "where rating_count > 100 & first_release_date > 1577836800;\n" +
        $"sort rating desc;";	   
    
    public static string Recent
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return $"where first_release_date >= {now.AddDays(-30).ToUnixTimeSeconds()} " +
                   $"& first_release_date <= {now.ToUnixTimeSeconds()} & cover != null;\n" +
                   $"& hypes > 5 & cover != null\n" +
                   $"sort first_release_date desc;\n" +
                   $"limit 20;";
        }
    }

    public static string Upcoming
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return $"where first_release_date > {now.ToUnixTimeSeconds()} " +
                   $"& first_release_date < {now.AddYears(1).ToUnixTimeSeconds()}" +
                   $"& hypes > 20 & cover != null\n" +
                   $"& version_parent = null;\n" +
                   $"sort first_release_date asc;\n" +
                   $"limit 20;";
        }
    }
};