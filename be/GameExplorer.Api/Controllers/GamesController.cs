using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
	readonly RawgService _rawg;
	public GamesController( RawgService rawg )
	{
		_rawg = rawg;	
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<GameDetails>>	GetGameDetails( int id, CancellationToken ct )
	{
		GameDetails? gd = await _rawg.GetGameDetailsAsync(id, ct);
		if (gd is null)
			return NotFound();
		return Ok(gd);
	}

	[HttpGet("{id:int}/screenshots")]
	public async Task<ActionResult<List<string?>>>			GetScreenshots( int id, CancellationToken ct )
	{
		List<string?>? ss = await _rawg.GetScreenshotsAsync(id, ct);
		if (ss is null)
			return NotFound();
		return Ok();
	}

	[HttpGet("top-rated")]
	public async Task<ActionResult<List<Game>>>	GetTopRated( CancellationToken ct ) => Ok(await _rawg.GetTopRatedAsync(ct));

	[HttpGet("recent")]
	public async Task<ActionResult<List<Game>>>	GetRecent( CancellationToken ct ) => Ok(await _rawg.GetRecentAsync(ct));
	
	[HttpGet("upcoming")]
	public async Task<ActionResult<List<Game>>>	GetUpcoming( CancellationToken ct ) => Ok(await _rawg.GetUpcomingAsync(ct));

}