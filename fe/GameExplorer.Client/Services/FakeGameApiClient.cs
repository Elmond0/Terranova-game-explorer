using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services;

public class FakeGameApiClient: IGameApiClient
{
	public async Task<List<Game>> GetTopRatedAsync()
	{
		List<Game> giochi =[new Game {
			Id = 1,
			Slug = "boh",
			Name = "Halo",
			Rating = 4.5,
			Image = "http://foto.it",
			ReleaseDate = new DateOnly(2016, 7, 1),
			Metacritic = 12,
			Platforms = [new Platform {
				Id = 4,
				Name ="PC",
				Slug = "pc"
			}],
			Genres = [new Genre {
				Id = 5,
				Name = "guerra",
				Slug = "lumachine",
				Description = null
			}]
		}];
		await Task.Delay(800);
		return giochi;
	}
	public async Task<List<Game>> GetRecentAsync()
	{
		List<Game> giochi =[new Game {
			Id = 1,
			Slug = "boh",
			Name = "Halo",
			Rating = 4.5,
			Image = "http://foto.it",
			ReleaseDate = new DateOnly(2016, 7, 1),
			Metacritic = 12,
			Platforms = [new Platform {
				Id = 4,
				Name ="PC",
				Slug = "pc"
			}],
			Genres = [new Genre {
				Id = 5,
				Name = "guerra",
				Slug = "lumachine",
				Description = null
			}]
		}];
		await Task.Delay(800);
		return giochi;
	}
	public async Task<List<Game>> GetUpcomingAsync()
	{
		List<Game> giochi =[new Game {
			Id = 1,
			Slug = "boh",
			Name = "Halo",
			Rating = 4.5,
			Image = "http://foto.it",
			ReleaseDate = new DateOnly(2016, 7, 1),
			Metacritic = 12,
			Platforms = [new Platform {
				Id = 4,
				Name ="PC",
				Slug = "pc"
			}],
			Genres = [new Genre {
				Id = 5,
				Name = "guerra",
				Slug = "lumachine",
				Description = null
			}]
		}];
		await Task.Delay(800);
		return giochi;
	}
}
