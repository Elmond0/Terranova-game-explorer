using System.ComponentModel.DataAnnotations;
using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController( GameService gameService ) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<PagedResult<Game>>>	Search( [FromQuery] SearchFilter filter, CancellationToken ct )
	{
		if (filter.Page > 500)
		{
			ModelState.AddModelError(nameof(filter.Page), $"Page must be between 1 and 500.");
			return ValidationProblem(ModelState);
		}
		var	p = await gameService.SearchAsync(filter, ct);
		return Ok(p);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<GameDetails>>	GetGameDetails( int id, CancellationToken ct )
	{
		GameDetails? gd = await gameService.GetGameDetailsAsync(id, ct);
		if (gd is null)
			return NotFound();
		return Ok(gd);
	}

	[HttpGet("top-rated")]
	public async Task<ActionResult<PagedResult<Game>>>	GetTopRated(
		[FromQuery, Range(1, 500)] int page = 1,
		[FromQuery, Range(1, 40)] int pageSize = 20,
		CancellationToken ct = default) => Ok(await gameService.GetTopRatedAsync(page, pageSize, ct));

	[HttpGet("recent")]
	public async Task<ActionResult<PagedResult<Game>>>	GetRecent(
		[FromQuery, Range(1, 500)] int page = 1,
		[FromQuery, Range(1, 40)] int pageSize = 20,
		CancellationToken ct = default) => Ok(await gameService.GetRecentAsync(page, pageSize, ct));
	
	[HttpGet("upcoming")]
	public async Task<ActionResult<PagedResult<Game>>>	GetUpcoming(
		[FromQuery, Range(1, 500)] int page = 1,
		[FromQuery, Range(1, 40)] int pageSize = 20,
		CancellationToken ct = default) => Ok(await gameService.GetUpcomingAsync(page, pageSize, ct));
	
}