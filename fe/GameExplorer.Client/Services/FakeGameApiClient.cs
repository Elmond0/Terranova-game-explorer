using System.Net;
using GameExplorer.Shared.Models;
namespace GameExplorer.Client.Services;

public class FakeGameApiClient : IGameApiClient
{
	// same ids, names and slugs IGDB sends, so links built from fake data also work against the real web api
	private static readonly Platform Pc = new Platform { Id = 6, Name = "PC (Microsoft Windows)", Slug = "win" };
	private static readonly Platform Ps5 = new Platform { Id = 167, Name = "PlayStation 5", Slug = "ps5" };
	private static readonly Platform Ps4 = new Platform { Id = 48, Name = "PlayStation 4", Slug = "ps4--1" };
	private static readonly Platform XboxSeries = new Platform { Id = 169, Name = "Xbox Series X|S", Slug = "series-x-s" };
	private static readonly Platform XboxOne = new Platform { Id = 49, Name = "Xbox One", Slug = "xboxone" };
	private static readonly Platform Switch = new Platform { Id = 130, Name = "Nintendo Switch", Slug = "switch" };
	private static readonly List<Platform> Platforms = [Pc, Ps5, Ps4, XboxSeries, XboxOne, Switch];

	// IGDB has no "Action" genre: Shooter is its closest match
	private static readonly Genre Shooter = new Genre { Id = 5, Name = "Shooter", Slug = "shooter" };
	private static readonly Genre Rpg = new Genre { Id = 12, Name = "Role-playing (RPG)", Slug = "role-playing-rpg" };
	private static readonly Genre Adventure = new Genre { Id = 31, Name = "Adventure", Slug = "adventure" };
	private static readonly Genre Puzzle = new Genre { Id = 9, Name = "Puzzle", Slug = "puzzle" };
	private static readonly Genre Indie = new Genre { Id = 32, Name = "Indie", Slug = "indie" };

	private static readonly List<Genre> Genres = [Shooter, Rpg, Adventure, Puzzle, Indie];

	// real IGDB images in the size the web api uses (t_cover_big, t_screenshot_big), so the layout looks like production.
	// The first four covers belong to the games with the same name, the others stand in for the made-up titles
	private const string IgdbImages = "https://images.igdb.com/igdb/image/upload";
	private static Image Cover(string imageId) =>
		new Image { Url = $"{IgdbImages}/t_cover_big/{imageId}.jpg", Width = 264, Height = 374 };
	private static Image Screenshot(string imageId) =>
		new Image { Url = $"{IgdbImages}/t_screenshot_big/{imageId}.jpg", Width = 889, Height = 500 };

	private const string Witcher3Cover = "coaarl";
	private const string Portal2Cover = "co1rs4";
	private const string GtaVCover = "co2lbd";
	private const string ZeldaBotwCover = "co3p2d";
	private const string SampleCover1 = "co670h";
	private const string SampleCover2 = "co4jni";
	private const string SampleCover3 = "co2ve1";
	private readonly List<int> _favoriteIds = [];
	private readonly List<int> _wishlistIds = [];
	// kept in memory so a profile change survives until the next reload, like a saved one would
	private ProfileResponse _profile = new("tester@example.com", "Tester", "ninja");
	// lets the "nickname already taken" (409) message be tried without a real database
	private const string TakenNickname = "preso";
	private static readonly List<Game> TopRated =
	[
		new Game
		{
			Id = 3328,
			Name = "The Witcher 3: Wild Hunt",
			Slug = "the-witcher-3-wild-hunt",
			Rating = 93.8,
			Image = Cover(Witcher3Cover),
			ReleaseDate = new DateOnly(2015, 05, 18),
			Platforms = [Pc, Ps4, XboxOne, Switch],
			Genres = [Rpg, Adventure]
		},
		new Game
		{
			Id = 4200,
			Name = "Portal 2",
			Slug = "portal-2",
			Rating = 91.3,
			Image = Cover(Portal2Cover),
			ReleaseDate = new DateOnly(2011, 04, 18),
			Platforms = [Pc],
			Genres = [Puzzle]
		},
		new Game
		{
			Id = 3498,
			Name = "Grand Theft Auto V",
			Slug = "grand-theft-auto-v",
			Rating = 89.6,
			Image = Cover(GtaVCover),
			ReleaseDate = new DateOnly(2013, 09, 17),
			Platforms = [Pc, Ps5, Ps4, XboxSeries, XboxOne],
			Genres = [Shooter, Adventure]
		},
		new Game
		{
			Id = 900001,
			Name = "Lumen Drift",
			Slug = "lumen-drift",
			Rating = 84,
			Image = null,
			ReleaseDate = new DateOnly(2019, 03, 01),
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
			Rating = 78,
			Image = Cover(SampleCover1),
			ReleaseDate = new DateOnly(2026, 10, 20),
			Platforms = [Ps5, XboxSeries, Pc],
			Genres = [Shooter, Adventure]
		},
		new Game
		{
			Id = 900102,
			Name = "La leggenda del cavaliere che attraversò le sette valli di cristallo alla ricerca della corona perduta",
			Slug = "la-leggenda-del-cavaliere",
			Rating = 82,
			Image = Cover(SampleCover2),
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
			Image = Cover(SampleCover3),
			ReleaseDate = new DateOnly(2026, 10, 05),
			Genres = []
		},
		new Game
		{
			Id = 900104,
			Name = "Night Shift",
			Slug = "night-shift",
			Rating = 70,
			Image = null,
			ReleaseDate = new DateOnly(2026, 09, 30),
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
			Image = Cover(SampleCover1),
			ReleaseDate = new DateOnly(2026, 11, 14),
			Platforms = [Ps5, XboxSeries],
			Genres = [Shooter]
		},
		new Game
		{
			Id = 900202,
			Name = "Frontiera Rossa",
			Slug = "frontiera-rossa",
			Rating = 0,
			Image = Cover(SampleCover3),
			ReleaseDate = new DateOnly(2027, 03, 02),
			Platforms = [Pc, Ps5],
			Genres = [Shooter, Adventure]
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
			Image = Cover(SampleCover2),
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
			Rating = 92.5,
			Image = Cover(ZeldaBotwCover),
			ReleaseDate = new DateOnly(2017, 03, 03),
			Platforms = [Switch],
			Genres = [Puzzle, Adventure],
		},
		.. Enumerable.Range(1, 25).Select(i => new Game {
				Id = 900300 + i,
				Name = $"Gioco di prova {i}",
				Slug = $"gioco-di-prova-{i}",
				Rating = 60 + (i % 20) * 2,
				Image = Cover(SampleCover2),
				ReleaseDate = new DateOnly(2000 + i, 1, 1),
				Platforms = [Platforms[i % 6]],
				Genres = [Genres[i % 5]],
			}),
	];
	private static IEnumerable<Game> AllGames => TopRated.Concat(Recent).Concat(Upcoming).Concat(SearchOnly);

	// the studios of the GTA V page plus a test one without image or description and with enough games for two pages,
	// to try the empty states and the pagination. Every studio has an unrated game (Rating 0) to try the average rating
	private static readonly Studio TestStudio = new Studio { Id = 900400, Name = "Studio di prova", Slug = "studio-di-prova" };
	private static readonly List<(Studio Studio, int[] GameIds)> Studios =
	[
		(new Studio
		{
			Id = 3524,
			Name = "Rockstar North",
			Slug = "rockstar-north",
			Image = Cover(GtaVCover),
			About = "Rockstar North is a Scottish video game developer and a studio of Rockstar Games based in Edinburgh. " +
				"The company was founded in 1987 as DMA Design and is best known for the Grand Theft Auto series.\n" +
				"Testo finto del FakeGameApiClient: serve a provare la descrizione dello studio.",
			Website = "https://www.rockstarnorth.com",
			CountryId = 826,
			StartDate = new DateOnly(1987, 1, 1),
		}, [3498, 900103]),
		(new Studio { Id = 10, Name = "Rockstar Games", Slug = "rockstar-games", Image = Cover(SampleCover1)}, [3498, 900104, 900201]),
		(TestStudio, [.. Enumerable.Range(900301, 25), 900202]),
	];
	private const int StudioGamesPageSize = 20;
	// small on purpose: the fake lists have few games, with 5 per page the pagination shows up in fake mode too
	private const int ListPageSize = 5;

	// lets ErrorMessage and "Riprova" be tried without stopping the web api.
	// Turned on in Program.cs: AddScoped<IGameApiClient>(_ => new FakeGameApiClient { SimulateError = true })
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
	public async Task<PagedResult<Game>> GetTopRatedPageAsync(int page, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return ToPage(TopRated, page, ListPageSize);
	}
	public async Task<PagedResult<Game>> GetRecentPageAsync(int page, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return ToPage(Recent, page, ListPageSize);
	}
	public async Task<PagedResult<Game>> GetUpcomingPageAsync(int page, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return ToPage(Upcoming, page, ListPageSize);
	}
	// the fake games have no vote count, so the real web api's "well-known games only" filter is not imitated
	public async Task<List<Game>> GetPlatformHighlightsAsync(int platformId, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return AllGames
			.Where(g => g.Platforms.Any(p => p.Id == platformId))
			.OrderByDescending(g => g.Rating)
			.Take(20)
			.ToList();
	}

	// developers and publishers are ignored: no page filters by them yet
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
		return ToPage(Sort(games, filter.Ordering), filter.Page, filter.PageSize);
	}

	// an id in no list returns null, like the 404 of the real web api, to try the "not found" page
	public async Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);

		var game = AllGames.FirstOrDefault(g => g.Id == id);
		if (game is null)
			return null;

		var details = new GameDetails
		{
			Id = game.Id,
			Name = game.Name,
			Slug = game.Slug,
			Rating = game.Rating,
			Image = game.Image,
			ReleaseDate = game.ReleaseDate,
			Platforms = game.Platforms,
			Genres = game.Genres,
		};

		// only GTA V has a full page: the other games are there to try missing data
		if (details.Id == 3498)
		{
			details.Description =
				"Rockstar Games went bigger, since their previous installment of the series. You get the complicated and realistic world-building from Liberty City of GTA4 in the setting of lively and diverse Los Santos, from an old fan favorite GTA San Andreas. 561 different vehicles (including every transport you can operate) and the amount is rising with every update.\n" +
				"Simultaneous storytelling from three unique perspectives: follow Michael, ex-criminal living his life of leisure away from the past, Franklin, a kid that seeks the better future, and Trevor, the exact past Michael is trying to run away from.\n" +
				"GTA Online will provide a lot of additional challenge even for the experienced players, coming fresh from the story mode. Now you will have other players around that can help you just as likely as ruin your mission. Every GTA mechanic up to date can be experienced by players through the unique customizable character, and community content paired with the leveling system tends to keep everyone busy and engaged.";
			details.Screenshots =
			[
				Screenshot("sc10f91"),
				Screenshot("sc10f92"),
				Screenshot("sc10f93"),
				Screenshot("n3t2agwuxlqggp3kryf9"),
			];
			details.Developers =
			[
				new Studio { Id = 3524, Name = "Rockstar North", Slug = "rockstar-north" },
				new Studio { Id = 10, Name = "Rockstar Games", Slug = "rockstar-games" },
			];
		}
		// the "Gioco di prova" games link to the test studio, the one with two pages of games
		else if (details.Name.StartsWith("Gioco di prova"))
		{
			details.Developers = [TestStudio];
		}

		return details;
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

	// an unknown id returns null, like the 404 of the real web api, to try the "not found" page
	public async Task<Studio?> GetStudioAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);

		var entry = Studios.FirstOrDefault(s => s.Studio.Id == id);
		if (entry.Studio is null)
			return null;

		return new Studio
		{
			Id = entry.Studio.Id,
			Name = entry.Studio.Name,
			Slug = entry.Studio.Slug,
			Image = entry.Studio.Image,
			About = entry.Studio.About,
			Website = entry.Studio.Website,
			CountryId = entry.Studio.CountryId,
			StartDate = entry.Studio.StartDate,
			StatusId = entry.Studio.StatusId
		};
	}

	public async Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);

		var gameIds = Studios.FirstOrDefault(s => s.Studio.Id == id).GameIds ?? [];
		var games = AllGames.Where(g => gameIds.Contains(g.Id));
		return ToPage(games, page, StudioGamesPageSize);
	}

	public async Task<List<Game>> GetStudioUpcomingAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		var gameIds = Studios.FirstOrDefault(s => s.Studio.Id == id).GameIds ?? [];
		return Upcoming.Where(g => gameIds.Contains(g.Id)).ToList();
	}

	private static IEnumerable<Game> Sort(IEnumerable<Game> games, string? ordering) => ordering switch
	{
		"rating" => games.OrderBy(g => g.Rating),
		"-rating" => games.OrderByDescending(g => g.Rating),
		"released" => games.OrderBy(g => g.ReleaseDate),
		"-released" => games.OrderByDescending(g => g.ReleaseDate),
		"name" => games.OrderBy(g => g.Name),
		"-name" => games.OrderByDescending(g => g.Name),
		_ => games
	};

	private static PagedResult<Game> ToPage(IEnumerable<Game> games, int page, int pageSize)
	{
		var total = games.Count();
		var items = games
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();
		return new PagedResult<Game>
		{
			Items = items,
			Total = total,
			Page = page,
			PageSize = pageSize,
		};
	}

	// people search by abbreviation: "gta" has to find "Grand Theft Auto V" through its initials
	private static string Initials(string name) =>
		string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => word[0]));

	// a fake network wait so skeletons and cancellations can be seen, plus the simulated error, in one place for every method
	private async Task DelayAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(800, cancellationToken);
		if (SimulateError)
			throw new ApiException("Errore simulato dal FakeGameApiClient", null);
	}
	private static List<Game> ToGames(List<int> ids) => ids.Select(
		id => AllGames.FirstOrDefault(g => g.Id == id)).OfType<Game>().ToList();

	public async Task AddFavoriteAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		if (!_favoriteIds.Contains(id))
		_favoriteIds.Insert(0, id);
	}
	public async Task RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		_favoriteIds.Remove(id);
	}
	public async Task<List<Game>> GetFavoritesAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return ToGames(_favoriteIds);
	}
	public async Task AddToWishlistAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		if (!_wishlistIds.Contains(id))
		_wishlistIds.Insert(0, id);
	}
	public async Task RemoveFromWishlistAsync(int id, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		_wishlistIds.Remove(id);
	}
	public async Task<List<Game>> GetWishlistAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return ToGames(_wishlistIds);
	}
	public async Task<ProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		return _profile;
	}
	public async Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		await DelayAsync(cancellationToken);
		if (request.DisplayName == TakenNickname)
			throw new ApiException("Nickname già preso (simulato dal FakeGameApiClient)", null, HttpStatusCode.Conflict);
		_profile = _profile with { DisplayName = request.DisplayName, Avatar = request.Avatar };
	}
}
