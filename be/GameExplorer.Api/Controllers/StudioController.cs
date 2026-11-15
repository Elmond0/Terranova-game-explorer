using System.Threading;
using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/studios")]
public class StudiosController( GameService gameService ) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetStudio(int id, CancellationToken ct)
    {
        var st = await gameService.GetStudioDetailsAsync(id, ct);
        if (st is null)
            return NotFound();
        return Ok(st);
    }
    
    [HttpGet("{id:int}/upcoming")]
    public async Task<ActionResult<List<Game>>> GetStudioUpcoming(int id, CancellationToken ct) => Ok(await gameService.GetStudioUpcomingAsync(id, ct));
    
    [HttpGet("{id:int}/games")]
    public async Task<ActionResult<PagedResult<Game>>> GetStudioGames(
        int id,
        [FromQuery, Range(1, 500)] int page = 1,
        [FromQuery, Range(1, 40)] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await gameService.GetStudioGamesAsync(id, page, pageSize, ct));
}