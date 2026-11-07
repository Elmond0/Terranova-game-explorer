using System.Security.Claims;
using System.Text.Json;
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

	static async Task<IResult> AddFavorite(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, RawgService games, GameSnapshotStore snapshots, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		GameDetails? game = await games.GetGameDetailsAsync(gameId, ct);
		if (game is null)
			return TypedResults.NotFound();

		await snapshots.SaveAsync(game, ct);

		await db.Database.ExecuteSqlAsync($"INSERT INTO favorites (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetFavorites(ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		List<string?> details = await db.Favorites
			.Where(f => f.UserId == userId)
			.Join(db.GameSnapshots, f => f.GameId, g => g.GameId, (f, g) => new { f.AddedAt, g.Details })
			.OrderByDescending(x => x.AddedAt)
			.Select(x => x.Details)
			.ToListAsync(ct);

		List<Game> favorites = details.Select(d => JsonSerializer.Deserialize<Game>(d!)!).ToList();

		return TypedResults.Ok(favorites);
	}
	static async Task<IResult> RemoveFromWishlist(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		await db.WishlistItems
				.Where(w => w.UserId == userId && w.GameId == gameId)
				.ExecuteDeleteAsync(ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> AddToWishlist(int gameId, ClaimsPrincipal user, UserManager<IdentityUser> users, RawgService games, GameSnapshotStore snapshots, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		GameDetails? game = await games.GetGameDetailsAsync(gameId, ct);
		if (game is null)
			return TypedResults.NotFound();

		await snapshots.SaveAsync(game, ct);

		await db.Database.ExecuteSqlAsync($"INSERT INTO wishlist (user_id, game_id, added_at) VALUES ({userId}, {gameId}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);

		return TypedResults.NoContent();
	}

	static async Task<IResult> GetWishlist(ClaimsPrincipal user, UserManager<IdentityUser> users, AppDbContext db, CancellationToken ct)
	{
		string userId = users.GetUserId(user)!;

		List<string?> details = await db.WishlistItems
				.Where(w => w.UserId == userId)
				.Join(db.GameSnapshots, w => w.GameId, g => g.GameId, (w, g) => new { w.AddedAt, g.Details })
				.OrderByDescending(x => x.AddedAt)
				.Select(x => x.Details)
				.ToListAsync(ct);

		List<Game> wishlist = details.Select(d => JsonSerializer.Deserialize<Game>(d!)!).ToList();

		return TypedResults.Ok(wishlist);
	}
}
