using GameExplorer.Api.Services.IgdbDtos;
namespace GameExplorer.Api.Services;

public record IgdbQueryFields()
{
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
    
    public static string StudioUpcoming( int id )
    {
        var now = DateTimeOffset.UtcNow;
        return $"where involved_companies.company = {id}" +
               $"&first_release_date > {now.ToUnixTimeSeconds()} " +
               $"& cover != null\n" +
               $"& version_parent = null;\n" +
               $"sort first_release_date asc;\n" +
               $"limit 5;";
    }
    
    public static (string, string, string) StudioGames( int id, int page, int pageSize )
    {
        var now = DateTimeOffset.UtcNow;
        var query = $"where involved_companies.company = {id}" +
                    $"& first_release_date <= {now.ToUnixTimeSeconds()} " +
                    $"& cover != null\n" +
                    $"& version_parent = null;\n";
        var sort = "sort total_rating_count desc;\n";
        var offset = (page - 1) * pageSize;
        var pagination = $"limit {pageSize};" +
                         $"offset {offset};";
        return (query, pagination, sort);
    }

    public static string PlatformHighlights( int id )
    {
        var now = DateTimeOffset.UtcNow;
        return $"where platforms = ({id})\n" +
               $"& first_release_date <= {now.ToUnixTimeSeconds()}\n" +
               // $"& first_release_date >= {now.AddYears(-2).ToUnixTimeSeconds()}\n" +
               // $"& rating_count > 20\n" +
               $"& cover != null\n" +
               $"& version_parent = null;\n" +
               $"sort rating desc;\n" +
               $"limit 20;";
    }
};