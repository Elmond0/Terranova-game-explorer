using GameExplorer.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Errors;

public sealed class IgdbExceptionHandler( ILogger<IgdbExceptionHandler> logger, IProblemDetailsService problems ) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync( HttpContext context, Exception e, CancellationToken ct )
	{
		if (e is not IgdbException)
			return false;

		logger.LogWarning(e, "IGDB request failed: {Message}", e.Message);

		context.Response.StatusCode = StatusCodes.Status502BadGateway;
		return await problems.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = context,
			ProblemDetails = new ProblemDetails
			{	
				Status = StatusCodes.Status502BadGateway,
				Title = "Igdb service not available.",
				Detail = "Igdb is not responding. Try again."
			}
		});
	}
}