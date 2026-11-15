using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController( GameService gameService ) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDetails>>	GetGameDetails( int id, CancellationToken ct )
    {
        GameDetails? gd = await gameService.GetGameDetailsAsync(id, ct);
        if (gd is null)
            return NotFound();
        return Ok(gd);
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<List<Game>>>	GetTopRated( CancellationToken ct ) => Ok(await gameService.GetHomeTopRatedAsync(ct));

    [HttpGet("recent")]
    public async Task<ActionResult<List<Game>>>	GetRecent( CancellationToken ct ) => Ok(await gameService.GetHomeRecentAsync(ct));
	
    [HttpGet("upcoming")]
    public async Task<ActionResult<List<Game>>>	GetUpcoming( CancellationToken ct ) => Ok(await gameService.GetHomeUpcomingAsync(ct));
	
}