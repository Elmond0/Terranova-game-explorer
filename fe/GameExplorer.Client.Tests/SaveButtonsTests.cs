using AngleSharp.Dom;                        // esiste già (dentro bUnit): IElement
using Bunit;
using GameExplorer.Client.Components;        // esiste già: SaveButtons
using Microsoft.AspNetCore.Components;       // esiste già: NavigationManager
using Microsoft.Extensions.DependencyInjection;

namespace GameExplorer.Client.Tests;

public class SaveButtonsTests : PageTestBase
{
	// GTA V: c'è nei dati del client finto
	private const int GameId = 3498;

	[Fact]
	public void Collegato_SalvaNeiPreferiti_IlPulsanteSiRiempie()
	{
		Auth.SetAuthorized("prova@example.com");
		var cut = Ctx.Render<SaveButtons>(parameters => parameters
			.Add(p => p.GameId, GameId));
		// Il pulsante si attiva solo quando la lettura delle due liste è finita
		cut.WaitForAssertion(() => Assert.False(FavoriteButton(cut).HasAttribute("disabled")));

		FavoriteButton(cut).Click();

		cut.WaitForAssertion(() =>
		{
			Assert.Equal("true", FavoriteButton(cut).GetAttribute("aria-pressed"));
			Assert.Contains("Nei preferiti", FavoriteButton(cut).TextContent);
		});
	}

	[Fact]
	public void Collegato_SalvataggioFallisce_IlPulsanteRestaVuotoEMostraErrore()
	{
		Auth.SetAuthorized("prova@example.com");
		var cut = Ctx.Render<SaveButtons>(parameters => parameters
			.Add(p => p.GameId, GameId));
		cut.WaitForAssertion(() => Assert.False(FavoriteButton(cut).HasAttribute("disabled")));
		// Solo adesso la Web API va giù: la lettura iniziale deve essere andata bene
		Api.SimulateError = true;

		FavoriteButton(cut).Click();

		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Non è stato possibile aggiornare i preferiti", cut.Markup);
			Assert.Equal("false", FavoriteButton(cut).GetAttribute("aria-pressed"));
			Assert.Contains("Aggiungi ai preferiti", FavoriteButton(cut).TextContent);
		});
	}

	[Fact]
	public void NonCollegato_Clic_PortaAllAccesso()
	{
		// Nessun SetAuthorized: di base l'utente non è collegato
		var cut = Ctx.Render<SaveButtons>(parameters => parameters
			.Add(p => p.GameId, GameId));

		FavoriteButton(cut).Click();

		// NavigateTo parte prima di qualsiasi await: non serve aspettare
		var nav = Ctx.Services.GetRequiredService<NavigationManager>();
		Assert.EndsWith("/account/login", nav.Uri);
	}

	private static IElement FavoriteButton(IRenderedComponent<SaveButtons> cut) =>
		cut.FindAll("button").Single(b => b.TextContent.Contains("preferiti"));
}
