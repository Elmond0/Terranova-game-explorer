using Bunit;
using GameExplorer.Client.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace GameExplorer.Client.Tests;

public class SearchTests : PageTestBase
{
	[Fact]
	public void FiltriNellIndirizzo_UsatiNellaRicerca()
	{
		// MudSelect and MudTooltip need the provider, which in the app lives in the layout and here does not exist
		Ctx.Render<MudPopoverProvider>();
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		navigation.NavigateTo("/search?q=portal&genres=5");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Nessun gioco trovato.", cut.Markup);
			Assert.DoesNotContain("Portal 2", cut.Markup);
			// the button counts the filters read from the address (the text search is not a filter)
			Assert.Contains("Filtri (1)", cut.Markup);
		});
	}

	[Fact]
	public void AnnoFuoriIntervallo_CampoInErroreEAvvisoSugliAnni()
	{
		Ctx.Render<MudPopoverProvider>();
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		// 1800 is before 1950: the page must not call the web api (it would answer 400)
		navigation.NavigateTo("/search?fromYear=1800");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Tra 1950 e", cut.Markup);
			Assert.Contains("Controlla gli anni", cut.Markup);
			Assert.DoesNotContain("Nessun gioco trovato.", cut.Markup);
		});
	}

	[Fact]
	public void ParametriNonValidi_LaPaginaLiIgnora()
	{
		Ctx.Render<MudPopoverProvider>();
		var navigation = Ctx.Services.GetRequiredService<NavigationManager>();
		// "abc" in a numeric parameter used to crash the whole page
		navigation.NavigateTo("/search?fromYear=abc&genres=abc&page=abc&ordering=foo");
		var cut = Ctx.Render<Search>();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("giochi trovati", cut.Markup);
			Assert.DoesNotContain("Tra 1950 e", cut.Markup);
			Assert.DoesNotContain("Filtri (", cut.Markup);
		});
	}
}
