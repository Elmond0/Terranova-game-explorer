using GameExplorer.Api.Services.IgdbDtos;
namespace GameExplorer.Api.Services;

public record IgdbQueryFields()
{
    public static (string query, string pagination, string sort) Genres()
    {
        const string query = "";
        const string sort = "sort name;\n";
        const string pagination = $"limit 500;";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) Platforms()
    {
        const string query = "";
        const string sort = "sort name;\n";
        const string pagination = $"limit 500;";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) TopRated( int page = 1, int pageSize = 20 )
    {
        var query = $"where rating_count > 100" +
                    $"& cover != null\n" +
                    $"& version_parent = null;\n";
        var sort = "sort rating desc;\n";
        var offset = (page - 1) * pageSize;
        var pagination = $"limit {pageSize};" +
                         $"offset {offset};";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) Recent( int page = 1, int pageSize = 20, bool home = true )
    {
        var now = DateTimeOffset.UtcNow;
        var query = $"where first_release_date <= {now.ToUnixTimeSeconds()} " +
                    $"& version_parent = null " +
                    $"& cover != null ";
        if (home == true)
            query += "& hypes > 10;\n";
        else
            query += "& hypes > 5;\n";
        var sort = "sort first_release_date desc;\n";
        var offset = (page - 1) * pageSize;
        var pagination = $"limit {pageSize}; " +
                         $"offset {offset};";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) Upcoming( int page = 1, int pageSize = 20, bool home = true )
    {
        var now = DateTimeOffset.UtcNow;
        var query = $"where first_release_date > {now.ToUnixTimeSeconds()} " +
                    $"& first_release_date < {now.AddYears(2).ToUnixTimeSeconds()}" +
                    $"& version_parent = null" +
                    $"& cover != null";
        if (home == true)
            query += "& hypes > 20;\n";
        else
            query += "& hypes > 10;\n";
        var sort = "sort first_release_date asc;\n";
        var offset = (page - 1) * pageSize;
        var pagination = $"limit {pageSize};" +
                         $"offset {offset};";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) StudioUpcoming( int id )
    {
        var now = DateTimeOffset.UtcNow;
        var query = $"where involved_companies.company = {id}" +
                    $"&first_release_date > {now.ToUnixTimeSeconds()} " +
                    $"& cover != null\n" +
                    $"& version_parent = null;\n";
        var sort = $"sort first_release_date asc;\n";
        var pagination = $"limit 5;";
        return (query, pagination, sort);
    }
    
    public static (string query, string pagination, string sort) StudioGames( int id, int page, int pageSize )
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

    public static (string query, string pagination, string sort) PlatformHighlights( int id )
    {
        var now = DateTimeOffset.UtcNow;
        var query = $"where platforms = ({id})\n" +
                    $"& first_release_date <= {now.ToUnixTimeSeconds()}\n" +
                    $"& first_release_date >= {now.AddYears(-2).ToUnixTimeSeconds()}\n" +
                    $"& rating_count > 20\n" +
                    $"& cover != null\n" +
                    $"& version_parent = null;\n";
        var sort = $"sort rating desc;\n";
        var pagination = "limit 20;";
        return (query, pagination, sort);
    }
};