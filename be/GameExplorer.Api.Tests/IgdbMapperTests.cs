using System.Text.Json;
using GameExplorer.Api.Services.Rawg;

public class RawgConverterTests
{
    [Fact]
    public void GiocoSenzaDataImmagineMetacritic_NonEsplode()
    {
        var json = """
            {"id":1,"slug":"x","name":"X","released":null,
             "background_image":null,"metacritic":null}
            """;
        var raw = JsonSerializer.Deserialize<RawgGame>(json)!;
        var game = raw.InGame();

        Assert.Null(game.ReleaseDate);
        Assert.Null(game.Image);
        Assert.Null(game.Metacritic);
        Assert.Empty(game.Platforms);
    }
}