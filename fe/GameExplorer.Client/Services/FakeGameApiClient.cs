using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services;

public class FakeGameApiClient: IGameApiClient
{
	private static readonly Platform Pc = new Platform { Id = 4, Name = "PC", Slug = "pc"};
	private static readonly Platform Ps5 = new Platform { Id = 187, Name = "PlayStation 5", Slug = "playstation5"};
	private static readonly Platform Ps4 = new Platform { Id = 18, Name = "PlayStation 4", Slug = "playstation4"};
	private static readonly Platform XboxSeries = new Platform { Id = 186, Name = "Xbox Series S/X", Slug = "xbox-series-x"};
	private static readonly Platform XboxOne = new Platform { Id = 1, Name = "Xbox One", Slug = "xbox-one"};
	private static readonly Platform Switch = new Platform { Id = 7, Name = "Nintendo Switch", Slug = "nintendo-switch"};

	private static readonly string A = "https://media.rawg.io/media/games/618/618c2031a07bbff6b4f611f10b6bcdbc.jpg";
	private static readonly string B = "https://media.rawg.io/media/games/2ba/2bac0e87cf45e5b508f227d281c9252a.jpg";
	private static readonly string C = "https://media.rawg.io/media/games/456/456dea5e1c7e3cd07060c14e96612001.jpg";
	private static readonly string D = "https://media.rawg.io/media/games/b7d/b7d3f1715fa8381a4e780173a197a615.jpg";
	private static readonly string E = "https://media.rawg.io/media/games/511/5118aff5091cb3efec399c808f8c598f.jpg";
	private static readonly string F = "https://media.rawg.io/media/games/d82/d82990b9c67ba0d2d09d4e6fa88885a7.jpg";

	private static readonly List<Game> TopRated =
	[
		new Game
		{
			Id = 3328,
			Name = "The Witcher 3: Wild Hunt",
			Slug = "the-witcher-3-wild-hunt",
			Rating = 4.65,
			Image = A,
			ReleaseDate = new DateOnly(2015, 05, 18),
			Metacritic = 92,
			Platforms = [Pc, Ps4, XboxOne, Switch],
		},
		new Game
		{
			Id = 4200,
			Name = "Portal 2",
			Slug = "portal-2",
			Rating = 4.61,
			Image = B,
			ReleaseDate = new DateOnly(2011, 04, 18),
			Metacritic = 95,
			Platforms = [Pc],
		},
		new Game
		{
			Id = 3498,
			Name = "Grand Theft Auto V",
			Slug = "grand-theft-auto-v",
			Rating = 4.47,
			Image = C,
			ReleaseDate = new DateOnly(2013, 09, 17),
			Metacritic = 92,
			Platforms = [Pc, Ps5, Ps4, XboxSeries, XboxOne],
		},
		new Game
		{
			Id = 900001,
			Name = "Lumen Drift",
			Slug = "lumen-drift",
			Rating = 4.2,
			Image = null,
			ReleaseDate = new DateOnly(2019, 03, 01),
			Metacritic = 81,
			Platforms = [Pc],
		}
	];

	private static readonly List<Game> Recent =
	[
		new Game
		{
			Id = 900101,
			Name = "Stellar Courier",
			Slug = "stellar-courier",
			Rating = 3.9,
			Image = D,
			ReleaseDate = new DateOnly(2026, 10, 20),
			Metacritic = 78,
			Platforms = [ps5, XboxSeries, pc],
		},
		new Game
		{
			Id = 900102,
			Name = "La leggenda del cavaliere che attraversò le sette valli di cristallo alla ricerca della corona perduta",
			Slug = "la-leggenda-del-cavaliere",
			Rating = 4.1,
			Image = E,
			ReleaseDate = new DateOnly(2026, 10, 12),
			Platforms = [Switch],
		},
		new Game
		{
			Id = 900103,
			Name = "Pixel Garden",
			Slug = "pixel-garden",
			Rating = 0,
			Image = F,
			ReleaseDate = new DateOnly(2026, 10, 05),
		},
		new Game
		{
			Id = 900104,
			Name = "Night Shift",
			Slug = "night-shift",
			Rating = 3.5,
			Image = null,
			ReleaseDate = new DateOnly(2026, 09, 30),
			Metacritic = 70,
			Platforms = [pc],
		}
	];

	private static readonly List<Game> Upcoming =
	[
		new Game
		{
			Id = 900201,
			Name = "Aurora Protocol",
			Slug = "aurora-protocol",
			Rating = 0,
			Image = D,
			ReleaseDate = new DateOnly(2026, 11, 14),
			Platforms = [ps5, XboxSeries],
		},
		new Game
		{
			Id = 900202,
			Name = "Frontiera Rossa",
			Slug = "frontiera-rossa",
			Rating = 0,
			Image = F,
			ReleaseDate = new DateOnly(2027, 03, 02),
			Platforms = [pc, ps5],
		},
		new Game
		{
			Id = 900203,
			Name = "Ombre su Venezia",
			Slug = "ombre-su-venezia",
			Rating = 0,
			Image = null,
			Platforms = [pc],
		},
		new Game
		{
			Id = 900204,
			Name = "Kart Galattico",
			Slug = "kart-galattico",
			Rating = 0,
			Image = E,
			ReleaseDate = new DateOnly(2027, 06, 18),
			Platforms = [Switch],
		}
	];

	public async Task<List<Game>> GetTopRatedAsync()
	{
		await Task.Delay(800);
		return TopRated;
	}
	public async Task<List<Game>> GetRecentAsync()
	{
		await Task.Delay(800);
		return Recent;
	}
	public async Task<List<Game>> GetUpcomingAsync()
	{
		await Task.Delay(800);
		return Upcoming;
	}
}
