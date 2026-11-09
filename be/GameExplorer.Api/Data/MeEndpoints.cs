using System.Security.Claims;
using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
	}

	static async Task<IResult> RemoveFavorite(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		await db.Favorites
			.Where(f => f.UserId == userId && f.GameId == gameId)
			.ExecuteDeleteAsync(ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> AddFavorite(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, GameService games, GameSnapshotStore snapshots, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		GameDetails? game = await games.GetGameDetailsAsync(gameId, ct);
		if (game is null)
			return TypedResults.NotFound();

		await snapshots.SaveAsync(game, ct);

		await db.Database.ExecuteSqlAsync($"INSERT INTO favorites (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetFavorites(int[] platformIds, string? ordering, ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, GameSnapshotStore snapshots, CancellationToken ct)
	{
		if (!GameSnapshotStore.IsValidOrdering(ordering))
			return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Ordering"] = ["Ordering not valid."] });

		string userId = users.GetUserId(user)!;
		List<Game> games = await snapshots.GetListAsync(db.Favorites.Where(f => f.UserId == userId), platformIds, ordering, ct);

		return TypedResults.Ok(games);
	}

	static async Task<IResult> RemoveFromWishlist(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		await db.WishlistItems
				.Where(w => w.UserId == userId && w.GameId == gameId)
				.ExecuteDeleteAsync(ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> AddToWishlist(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, GameService games, GameSnapshotStore snapshots, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		GameDetails? game = await games.GetGameDetailsAsync(gameId, ct);
		if (game is null)
			return TypedResults.NotFound();

		await snapshots.SaveAsync(game, ct);

		await db.Database.ExecuteSqlAsync($"INSERT INTO wishlist (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetWishlist(int[] platformIds, string? ordering, ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, GameSnapshotStore snapshots, CancellationToken ct)
	{
		if (!GameSnapshotStore.IsValidOrdering(ordering))
			return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Ordering"] = ["Ordering not valid."] });

		string userId = users.GetUserId(user)!;
		List<Game> games = await snapshots.GetListAsync(db.WishlistItems.Where(w => w.UserId == userId), platformIds, ordering, ct);

		return TypedResults.Ok(games);
	}
}
