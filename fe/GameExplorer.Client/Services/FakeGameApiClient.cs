using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services;

public class FakeGameApiClient : IGameApiClient
{
	private static readonly Platform Pc = new Platform { Id = 4, Name = "PC", Slug = "pc" };
	private static readonly Platform Ps5 = new Platform { Id = 187, Name = "PlayStation 5", Slug = "playstation5" };
	private static readonly Platform Ps4 = new Platform { Id = 18, Name = "PlayStation 4", Slug = "playstation4" };
	private static readonly Platform XboxSeries = new Platform { Id = 186, Name = "Xbox Series S/X", Slug = "xbox-series-x" };
	private static readonly Platform XboxOne = new Platform { Id = 1, Name = "Xbox One", Slug = "xbox-one" };
	private static readonly Platform Switch = new Platform { Id = 7, Name = "Nintendo Switch", Slug = "nintendo-switch" };
	private static readonly List<Platform> Platforms = [Pc, Ps5, Ps4, XboxSeries, XboxOne, Switch];

	private static readonly Genre Action = new Genre { Id = 4, Name = "Action", Slug = "action" };
	private static readonly Genre Rpg = new Genre { Id = 5, Name = "RPG", Slug = "role-playing-games-rpg" };
	private static readonly Genre Adventure = new Genre { Id = 3, Name = "Adventure", Slug = "adventure" };
	private static readonly Genre Puzzle = new Genre { Id = 7, Name = "Puzzle", Slug = "puzzle" };
	private static readonly Genre Indie = new Genre { Id = 51, Name = "Indie", Slug = "indie" };

	private static readonly List<Genre> Genres = [Action, Rpg, Adventure, Puzzle, Indie];

	// Copertine vere di RAWG: le prime tre sono dei giochi omonimi, le altre servono per i titoli inventati
	private const string Witcher3Cover = "https://media.rawg.io/media/games/618/618c2031a07bbff6b4f611f10b6bcdbc.jpg";
	private const string Portal2Cover = "https://media.rawg.io/media/games/2ba/2bac0e87cf45e5b508f227d281c9252a.jpg";
	private const string GtaVCover = "https://media.rawg.io/media/games/456/456dea5e1c7e3cd07060c14e96612001.jpg";
	private const string SampleCover1 = "https://media.rawg.io/media/games/b7d/b7d3f1715fa8381a4e780173a197a615.jpg";
	private const string SampleCover2 = "https://media.rawg.io/media/games/511/5118aff5091cb3efec399c808f8c598f.jpg";
	private const string SampleCover3 = "https://media.rawg.io/media/games/d82/d82990b9c67ba0d2d09d4e6fa88885a7.jpg";

	private static readonly List<Game> TopRated =
	[
		new Game
		{
			Id = 3328,
			Name = "The Witcher 3: Wild Hunt",
			Slug = "the-witcher-3-wild-hunt",
			Rating = 4.65,
			Image = Witcher3Cover,
			ReleaseDate = new DateOnly(2015, 05, 18),
			Metacritic = 92,
			Platforms = [Pc, Ps4, XboxOne, Switch],
			Genres = [Action, Rpg]
		},
		new Game
		{
			Id = 4200,
			Name = "Portal 2",
			Slug = "portal-2",
			Rating = 4.61,
			Image = Portal2Cover,
			ReleaseDate = new DateOnly(2011, 04, 18),
			Metacritic = 95,
			Platforms = [Pc],
			Genres = [Puzzle]
		},
		new Game
		{
			Id = 3498,
			Name = "Grand Theft Auto V",
			Slug = "grand-theft-auto-v",
			Rating = 4.47,
			Image = GtaVCover,
			ReleaseDate = new DateOnly(2013, 09, 17),
			Metacritic = 92,
			Platforms = [Pc, Ps5, Ps4, XboxSeries, XboxOne],
			Genres = [Action, Adventure]
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
			Genres = [Indie, Puzzle]
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
			Image = SampleCover1,
			ReleaseDate = new DateOnly(2026, 10, 20),
			Metacritic = 78,
			Platforms = [Ps5, XboxSeries, Pc],
			Genres = [Action, Adventure]
		},
		new Game
		{
			Id = 900102,
			Name = "La leggenda del cavaliere che attraversò le sette valli di cristallo alla ricerca della corona perduta",
			Slug = "la-leggenda-del-cavaliere",
			Rating = 4.1,
			Image = SampleCover2,
			ReleaseDate = new DateOnly(2026, 10, 12),
			Platforms = [Switch],
			Genres = [Rpg, Adventure]
		},
		new Game
		{
			Id = 900103,
			Name = "Pixel Garden",
			Slug = "pixel-garden",
			Rating = 0,
			Image = SampleCover3,
			ReleaseDate = new DateOnly(2026, 10, 05),
			Genres = []
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
			Platforms = [Pc],
			Genres = [Indie]
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
			Image = SampleCover1,
			ReleaseDate = new DateOnly(2026, 11, 14),
			Platforms = [Ps5, XboxSeries],
			Genres = [Action]
		},
		new Game
		{
			Id = 900202,
			Name = "Frontiera Rossa",
			Slug = "frontiera-rossa",
			Rating = 0,
			Image = SampleCover3,
			ReleaseDate = new DateOnly(2027, 03, 02),
			Platforms = [Pc, Ps5],
			Genres = [Action, Adventure]
		},
		new Game
		{
			Id = 900203,
			Name = "Ombre su Venezia",
			Slug = "ombre-su-venezia",
			Rating = 0,
			Image = null,
			Platforms = [Pc],
			Genres = [Adventure, Puzzle]
		},
		new Game
		{
			Id = 900204,
			Name = "Kart Galattico",
			Slug = "kart-galattico",
			Rating = 0,
			Image = SampleCover2,
			ReleaseDate = new DateOnly(2027, 06, 18),
			Platforms = [Switch],
			Genres = [Indie]
		}
	];

	private static readonly List<Game> SearchOnly =
	[
		new Game
		{
			Id = 22511,
			Name = "The Legend of Zelda: Breath of the Wild",
			Slug = "the-legend-of-zelda-breath-of-the-wild",
			Rating = 4.5,
			Image = "https://media.rawg.io/media/games/511/5118aff5091cb3efec399c808f8c598f.jpg",
			ReleaseDate = new DateOnly(2017, 03, 03),
			Metacritic = 97,
			Platforms = [Switch],
			Genres = [Action, Adventure],
		},
		.. Enumerable.Range(1, 25).Select(i => new Game {
				Id = 900300 + i,
				Name = $"Gioco di prova {i}",
				Slug = $"gioco-di-prova-{i}",
				Rating = 3 + (i % 20) / 10.0,
				Image = SampleCover2,
				ReleaseDate = new DateOnly(2000 + i, 1, 1),
				Metacritic = 60 + i,
				Platforms = [Platforms[i % 6]],
				Genres = [Genres[i % 5]],
			}),
	];
	private static IEnumerable<Game> AllGames => TopRated.Concat(Recent).Concat(Upcoming).Concat(SearchOnly);

	// true: ogni metodo lancia ApiException, per provare ErrorMessage e "Riprova".
	// Si attiva in Program.cs: AddScoped<IGameApiClient>(_ => new FakeGameApiClient { SimulateError = true })
	public bool SimulateError { get; set; }

	public async Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return TopRated;
	}

	public async Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return Recent;
	}

	public async Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return Upcoming;
	}

	// Filtra, ordina e pagina come la Web API vera; sviluppatori, publisher e store li ignora
	public async Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);

		var games = AllGames;
		if (!string.IsNullOrWhiteSpace(filter.Query))
			games = games.Where(g =>
				g.Name.Contains(filter.Query, StringComparison.OrdinalIgnoreCase)
				|| Initials(g.Name).Contains(filter.Query.Trim(), StringComparison.OrdinalIgnoreCase));
		if (filter.GenreIds.Count > 0)
		{
			games = games.Where(g =>
				g.Genres.Any(genre =>
					filter.GenreIds.Contains(genre.Id)));
		}
		if (filter.PlatformIds.Count > 0)
		{
			games = games.Where(g =>
				g.Platforms.Any(platform =>
					filter.PlatformIds.Contains(platform.Id)));
		}
		if (filter.FromYear is not null)
			games = games.Where(g => g.ReleaseDate?.Year >= filter.FromYear);
		if (filter.ToYear is not null)
			games = games.Where(g => g.ReleaseDate?.Year <= filter.ToYear);
		if (filter.MinMetacritic is not null)
			games = games.Where(g => g.Metacritic >= filter.MinMetacritic);
		games = filter.Ordering switch
		{
			"rating" => games.OrderBy(g => g.Rating),
			"-rating" => games.OrderByDescending(g => g.Rating),
			"released" => games.OrderBy(g => g.ReleaseDate),
			"-released" => games.OrderByDescending(g => g.ReleaseDate),
			"name" => games.OrderBy(g => g.Name),
			"-name" => games.OrderByDescending(g => g.Name),
			"metacritic" => games.OrderBy(g => g.Metacritic),
			"-metacritic" => games.OrderByDescending(g => g.Metacritic),
			_ => games
		};
		var total = games.Count();
		var items = games
			.Skip((filter.Page - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToList();
		return new PagedResult<Game>
		{
			Items = items,
			Total = total,
			Page = filter.Page,
			PageSize = filter.PageSize,
			IsLast = filter.Page * filter.PageSize >= total,
		};
	}

	// Un id che non è in nessuna lista simula "non trovato" (la Web API risponderebbe 404)
	public async Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);

		var game = AllGames.FirstOrDefault(g => g.Id == id);
		if (game is null)
			return null;

		return new GameDetails
		{
			Id = game.Id,
			Name = game.Name,
			Slug = game.Slug,
			Rating = game.Rating,
			Image = game.Image,
			ReleaseDate = game.ReleaseDate,
			Metacritic = game.Metacritic,
			Platforms = game.Platforms,
			Genres = game.Genres,
			About = $"Descrizione di prova per {game.Name}.",
		};
	}

	public async Task<List<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return Genres;
	}

	public async Task<List<Platform>> GetPlatformsAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return Platforms;
	}

	// Iniziali delle parole del nome: "Grand Theft Auto V" → "GTAV", così "gta" trova il gioco come su RAWG
	private static string Initials(string name) =>
		string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => word[0]));

	// Attesa finta della rete e, se richiesto, errore simulato: comuni a tutti i metodi
	private async Task DelayAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(800, cancellationToken);
		if (SimulateError)
			throw new ApiException("Errore simulato dal FakeGameApiClient", null);
	}
}
