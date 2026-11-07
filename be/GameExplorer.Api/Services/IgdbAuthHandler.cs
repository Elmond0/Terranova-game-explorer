using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace GameExplorer.Api.Services;

public class IgdbAuthHandler( TwitchTokenProvider tokenProvider , IOptions<IgdbOptions> options ) : DelegatingHandler
{
     private async Task AddHeaderAsync(HttpRequestMessage request, CancellationToken ct)
     {
          request.Headers.Remove("Client-ID");
          request.Headers.Add("Client-ID", options.Value.ClientId);
          request.Headers.Authorization =
               new AuthenticationHeaderValue("Bearer", await tokenProvider.GetTokenAsync(ct));
     }
     
     // Overloads HttpResponseMessage.SendAsync not igdb.SendAsync
     // is called by HttpResponseMessage.PostSync
     protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
     {
          await AddHeaderAsync(request, ct);
          var response = await base.SendAsync(request, ct);

          if (response.StatusCode == HttpStatusCode.Unauthorized)
          {
               response.Dispose();
               await tokenProvider.InvalidateAsync(ct);
               await AddHeaderAsync(request, ct);
               response = await base.SendAsync(request, ct);
          }
          return response;
     }
}

