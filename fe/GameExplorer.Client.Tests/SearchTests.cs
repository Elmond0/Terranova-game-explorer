using Bunit;
using GameExplorer.Client.Pages;                  // esiste già: Search
using Microsoft.AspNetCore.Components;            // esiste già: NavigationManager
using Microsoft.Extensions.DependencyInjection;   // esiste già: GetRequiredService
using MudBlazor;                                  // esiste già: MudPopoverProvider

namespace GameExplorer.Client.Tests;

public class SearchTests : PageTestBase
{
	[Fact]
	public void FiltriNellIndirizzo_UsatiNellaRicerca()
	{
		// MudSelect e MudTooltip hanno bisogno del provider, che nell'app sta nel layout
		Ctx.Render<MudPopoverProvider>();
		// Portal 2 è solo Puzzle (7): con il genere Action (4) non deve uscire
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		navigation.NavigateTo("/search?q=portal&genres=4");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Nessun gioco trovato.", cut.Markup);
			Assert.DoesNotContain("Portal 2", cut.Markup);
			// Il pulsante conta i filtri letti dall'indirizzo (la ricerca testuale non è un filtro)
			Assert.Contains("Filtri (1)", cut.Markup);
		});
	}
}
