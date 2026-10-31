using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Net;
using System.Runtime.ExceptionServices;

namespace GameExplorer.Api.Tests;

public class CacheTests
{
	// FakeRawg settato per rispondere a ogni chamata con 200 e un json con le informazioni obbligatorie
	// il client fa una chiamata a FakeRawg cercando i details del gioco id = 3498, controlla che lo status code sia 200 e lo salva in una variabile calls.
	// Dopodiche il client esegue un'altra chiamata allo stesso endpoint e compara le chiamate a FakeRawg alla variabile calls 
	[Fact]
	public async Task App_DoesNotCallRawgAtSecondRequest()
	{
		var rawg = FakeRawgHandler.Status(HttpStatusCode.OK, """{"id":3498,"name":"GTA V","slug":"gta-v"}""");
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.OK, first.StatusCode);
		int calls = rawg.Calls;

		await client.GetAsync("/api/games/3498");
		Assert.Equal(calls, rawg.Calls);
	}
}
