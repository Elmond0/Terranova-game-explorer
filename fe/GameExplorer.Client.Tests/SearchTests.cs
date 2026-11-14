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
		// Portal 2 è solo Puzzle (9): con il genere Shooter (5) non deve uscire
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		navigation.NavigateTo("/search?q=portal&genres=5");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Nessun gioco trovato.", cut.Markup);
			Assert.DoesNotContain("Portal 2", cut.Markup);
			// Il pulsante conta i filtri letti dall'indirizzo (la ricerca testuale non è un filtro)
			Assert.Contains("Filtri (1)", cut.Markup);
		});
	}

	[Fact]
	public void AnnoFuoriIntervallo_CampoInErroreEAvvisoSugliAnni()
	{
		Ctx.Render<MudPopoverProvider>();
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		// 1800 è prima di 1950: la pagina non deve chiamare la Web API (risponderebbe 400)
		navigation.NavigateTo("/search?fromYear=1800");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Tra 1950 e", cut.Markup);
			// Al posto di "Nessun gioco trovato." la pagina spiega che il problema sono gli anni
			Assert.Contains("Controlla gli anni", cut.Markup);
			Assert.DoesNotContain("Nessun gioco trovato.", cut.Markup);
		});
	}

	[Fact]
	public void ParametriNonValidi_LaPaginaLiIgnora()
	{
		Ctx.Render<MudPopoverProvider>();
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		// Prima "abc" in un parametro numerico faceva fallire tutta la pagina
		navigation.NavigateTo("/search?fromYear=abc&genres=abc&page=abc&ordering=foo");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("giochi trovati", cut.Markup);
			Assert.DoesNotContain("Tra 1950 e", cut.Markup);
			// Nessun filtro valido: il pulsante non mostra un numero
			Assert.DoesNotContain("Filtri (", cut.Markup);
		});
	}
}
