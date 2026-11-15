using System.Security.Claims;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Api.Services;
using GameExplorer.Api.Services.IgdbDtos;
using GameExplorer.Shared.Models;
using GameExplorer.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Npgsql;

namespace GameExplorer.Api.Data;

public static class MeEndpoints
{
	public static void MapMeEndpoints(this IEndpointRouteBuilder app)
	{
		var me = app.MapGroup("/api/me").RequireAuthorization();

		me.MapGet("/favorites", GetFavorites);
		me.MapPut("/favorites/{gameId:int}", AddFavorite);
		me.MapDelete("/favorites/{gameId:int}", RemoveFavorite);
		me.MapGet("/wishlist", GetWishlist);
		me.MapPut("/wishlist/{gameId:int}", AddToWishlist);
		me.MapDelete("/wishlist/{gameId:int}", RemoveFromWishlist);
		me.MapGet("/profile", GetProfile);
		me.MapPut("/profile", UpdateProfile);
		me.MapDelete("", DeleteAccount);
	}

	static bool IsValidDisplayName(string? name) =>
		name is { Length: >= 3 and <= 30 } && name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');

	static async Task<IResult> GetProfile(ClaimsPrincipal user, AppDbContext db)
	{
		User? appUser = await db.Users.FindAsync(user.GetUserId());
		if (appUser is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(new ProfileResponse(appUser.Email, appUser.DisplayName, appUser.Avatar));
	}

	static async Task<IResult> UpdateProfile(UpdateProfileRequest request, ClaimsPrincipal user, AppDbContext db)
	{
		Dictionary<string, string[]> errors = [];
		if (!IsValidDisplayName(request.DisplayName))
			errors["DisplayName"] = ["From 3 to 30 characters: letters, numbers and underscore."];
		if (request.Avatar is not null && !Avatars.All.Contains(request.Avatar))
			errors["Avatar"] = ["Avatar not valid."];
		if (errors.Count > 0)
			return TypedResults.ValidationProblem(errors);

		User? appUser = await db.Users.FindAsync(user.GetUserId());
		if (appUser is null)
			return TypedResults.NotFound();

		appUser.DisplayName = request.DisplayName;
		appUser.Avatar = request.Avatar;
		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
		{
			return TypedResults.Problem(title: "Nickname already taken.", statusCode: StatusCodes.Status409Conflict);
		}

		return TypedResults.NoContent();
	}

	static async Task<IResult> DeleteAccount(ClaimsPrincipal user, AppDbContext db, HttpContext http)
	{
		int id = user.GetUserId();

		await db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
		await http.SignOutAsync();

		return TypedResults.NoContent();
	}

	static readonly string[] ValidOrders = ["added", "name", "released", "rating"];

	static bool IsValidOrdering(string? ordering) =>
		string.IsNullOrWhiteSpace(ordering) || ValidOrders.Contains(ordering.TrimStart('-'));

	static async Task<List<Game>> GetCardsAsync(IEnumerable<int> ids, IgdbClient igdb, HybridCache cache, CancellationToken ct)
	{
		string list = string.Join(",", ids.Order());

		return await cache.GetOrCreateAsync(
			$"cards:{list}",
			async cancel =>
			{
				List<IgdbGame> raw = await igdb.QueryAsync<IgdbGame>($"{IgdbGame.Fields} where id = ({list}); limit 500;", cancel);
				return raw.Select(IgdbMapper.ToGame).ToList();
			},
			IgdbCache.Search,
			cancellationToken: ct);
	}

	static async Task<IResult> GetListAsync<T>(IQueryable<T> entries, int[] platformIds, string? ordering, IgdbClient igdb, HybridCache cache, CancellationToken ct)
		where T : class, IListEntry
	{
		if (!IsValidOrdering(ordering))
			return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Ordering"] = ["Ordering not valid."] });

		Dictionary<int, DateTime> addedAt = await entries.ToDictionaryAsync(e => e.GameId, e => e.AddedAt, ct);
		if (addedAt.Count == 0)
			return TypedResults.Ok(new List<Game>());

		List<Game> cards = await GetCardsAsync(addedAt.Keys, igdb, cache, ct);

		IEnumerable<Game> filtered = cards.Where(g => platformIds.All(id => g.Platforms.Any(p => p.Id == id)));

		string sort = string.IsNullOrWhiteSpace(ordering) ? "-added" : ordering;
		bool desc = sort.StartsWith('-');
		string field = sort.TrimStart('-');

		Func<Game, object?> key = field switch
		{
			"name" => g => g.Name,
			"rating" => g => g.Rating,
			"released" => g => g.ReleaseDate,
			_ => g => addedAt[g.Id],
		};

		IOrderedEnumerable<Game> sorted = filtered.OrderBy(g => field == "released" && g.ReleaseDate is null);
		sorted = desc
			? sorted.ThenByDescending(key).ThenByDescending(g => g.Id)
			: sorted.ThenBy(key).ThenBy(g => g.Id);

		return TypedResults.Ok(sorted.ToList());
	}

	static async Task<IResult> RemoveFavorite(int gameId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
	{
		int userId = user.GetUserId();

		await db.Favorites
			.Where(f => f.UserId == userId && f.GameId == gameId)
			.ExecuteDeleteAsync(ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> AddFavorite(int gameId, ClaimsPrincipal user, GameService games, AppDbContext db, CancellationToken ct)
	{
		int userId = user.GetUserId();

		if (await games.GetGameDetailsAsync(gameId, ct) is null)
			return TypedResults.NotFound();

		await db.Database.ExecuteSqlAsync($"INSERT INTO favorites (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetFavorites(int[] platformIds, string? ordering, ClaimsPrincipal user, AppDbContext db, IgdbClient igdb, HybridCache cache, CancellationToken ct)
	{
		int userId = user.GetUserId();
		return await GetListAsync(db.Favorites.Where(f => f.UserId == userId), platformIds, ordering, igdb, cache, ct);
	}

	static async Task<IResult> RemoveFromWishlist(int gameId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
	{
		int userId = user.GetUserId();

		await db.WishlistItems
			.Where(w => w.UserId == userId && w.GameId == gameId)
			.ExecuteDeleteAsync(ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> AddToWishlist(int gameId, ClaimsPrincipal user, GameService games, AppDbContext db, CancellationToken ct)
	{
		int userId = user.GetUserId();

		if (await games.GetGameDetailsAsync(gameId, ct) is null)
			return TypedResults.NotFound();

		await db.Database.ExecuteSqlAsync($"INSERT INTO wishlist (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetWishlist(int[] platformIds, string? ordering, ClaimsPrincipal user, AppDbContext db, IgdbClient igdb, HybridCache cache, CancellationToken ct)
	{
		int userId = user.GetUserId();
		return await GetListAsync(db.WishlistItems.Where(w => w.UserId == userId), platformIds, ordering, igdb, cache, ct);
	}
}
