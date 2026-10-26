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
	public async Task<ActionResult<GameDetails>> Get( int id, CancellationToken ct )
	{
		GameDetails? gd = await _rawg.GetGameDetailsAsync(id, ct);
		if (gd is null)
			return NotFound();
		return gd;
	}

	
}