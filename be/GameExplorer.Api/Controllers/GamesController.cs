using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController( GameService gameService ) : ControllerBase
{
	// [HttpGet]
	// public async Task<ActionResult<PagedResult<Game>>>	Search( [FromQuery] SearchFilter filter, CancellationToken ct )
	// {
	// 	PagedResult<Game>	p = await gameService.SearchAsync(filter, ct);
	// 	return Ok(p);
	// }

	[HttpGet("{id:int}")]
	public async Task<ActionResult<GameDetails>>	GetGameDetails( int id, CancellationToken ct )
	{
		GameDetails? gd = await gameService.GetGameDetailsAsync(id, ct);
		if (gd is null)
			return NotFound();
		return Ok(gd);
	}

	// [HttpGet("{id:int}/screenshots")]
	// public async Task<ActionResult<List<string?>>>			GetScreenshots( int id, CancellationToken ct )
	// {
	// 	List<string?>? ss = await gameService.GetScreenshotsAsync(id, ct);
	// 	if (ss is null)
	// 		return NotFound();
	// 	return Ok(ss);
	// }

	[HttpGet("top-rated")]
	public async Task<ActionResult<List<Game>>>	GetTopRated( CancellationToken ct ) => Ok(await gameService.GetTopRatedAsync(ct));

	[HttpGet("recent")]
	public async Task<ActionResult<List<Game>>>	GetRecent( CancellationToken ct ) => Ok(await gameService.GetRecentAsync(ct));
	
	[HttpGet("upcoming")]
	public async Task<ActionResult<List<Game>>>	GetUpcoming( CancellationToken ct ) => Ok(await gameService.GetUpcomingAsync(ct));

}