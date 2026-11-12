using System.Threading;
using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<List<Game>>> GetStudioUpcoming(int id, CancellationToken ct)
    {
        var stu = await gameService.GetStudioUpcomingAsync(id, ct);
        if (stu is [] or null)
            return NotFound();
        return Ok(stu);
    }
    
    [HttpGet("{id:int}/games")]
    public async Task<ActionResult<PagedResult<Game>>> GetStudioGames(int id, int page, int pageSize, CancellationToken ct)
    {
        var stg = await gameService.GetStudioGamesAsync(id, page, pageSize, ct);
        if (stg.Items is [] or null)
            return NotFound();
        return Ok(stg);
    }
}