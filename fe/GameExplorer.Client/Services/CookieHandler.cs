using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace GameExplorer.Client.Services;

public class CookieHandler : DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
		return base.SendAsync(request, cancellationToken);
	}
}
