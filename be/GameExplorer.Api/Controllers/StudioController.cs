using System.Threading;
using GameExplorer.Api.Services;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Controllers;

[ApiController]
[Route("api/studios")]
public class StudiosController : Controller
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<List<Game>>> GetStudioUpcoming(int id, CancellationToken ct)
    {
        return Ok(await GameService.GetStudioUpcomingAsync(id, ct));
    }
}