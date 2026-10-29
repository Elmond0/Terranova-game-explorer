using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api")]
public class LookupController : ControllerBase
{
	readonly RawgService _rawg;
	public LookupController( RawgService rawg )
	{
		_rawg = rawg;	
	}

	[HttpGet("genres")]
	public async Task<ActionResult<List<Genre>>>	GetGenres( CancellationToken ct )
	{
		List<Genre>? g = await _rawg.GetGenresAsync(ct);
		if (g is null)
			return NotFound();
		return Ok(g);
	}

	[HttpGet("platforms")]
	public async Task<ActionResult<List<Platform>>>	GetPlatform( CancellationToken ct )
	{
		List<Platform>? g = await _rawg.GetPlatformsAsync(ct);
		if (g is null)
			return NotFound();
		return Ok(g);
	}
}