using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api")]
public class LookupController( GameService gameService ) : ControllerBase
{
	[HttpGet("genres")]
	public async Task<ActionResult<List<Genre>>>	GetGenres( CancellationToken ct )
	{
		List<Genre>? g = await gameService.GetGenresAsync(ct);
		if (g is null)
			return NotFound();
		return Ok(g);
	}

	[HttpGet("platforms")]
	public async Task<ActionResult<List<Platform>>>	GetPlatform( CancellationToken ct )
	{
		List<Platform>? g = await gameService.GetPlatformsAsync(ct);
		if (g is null)
			return NotFound();
		return Ok(g);
	}
}