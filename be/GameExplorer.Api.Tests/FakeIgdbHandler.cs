using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace GameExplorer.Api.Tests;

public sealed class FakeIgdbHandler : HttpMessageHandler
{
	private int _calls;
	private Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> _respond;

	public FakeIgbdHandler( Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> respond ) { _respond = respond; }
	public int Calls => _calls;

	protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken ct )
	{
		var n = Interlocked.Increment(ref _calls);
		return _respond(request, n, ct);
	}

	public static HttpResponseMessage Json( HttpStatusCode code, string body )
		=> new(code) { Content = new StringContent( body, Encoding.UTF8, "application/json") };

	public static FakeIgbdHandler Status( HttpStatusCode code, string body = "{}")
		=> new((_, _, _)
			=> Task.FromResult(Json(code, body)));

	public static FakeIgbdHandler Throws( Exception e )
		=> new((_, _, _)
			=> Task.FromException<HttpResponseMessage>(e));

	public static FakeIgbdHandler Hangs()
		=> new(async (_, _, ct)
			=>	{
					await Task.Delay(Timeout.InfiniteTimeSpan, ct);
					throw new UnreachableException(); 
				});
}