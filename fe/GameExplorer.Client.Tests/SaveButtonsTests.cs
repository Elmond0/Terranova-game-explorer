using AngleSharp.Dom;
using Bunit;
using GameExplorer.Client.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace GameExplorer.Client.Tests;

public class SaveButtonsTests : PageTestBase
{
	private const int GameId = 3498;

	[Fact]
	public void Collegato_SalvaNeiPreferiti_IlPulsanteSiRiempie()
	{
		Auth.SetAuthorized("prova@example.com");
		var cut = Ctx.Render<SaveButtons>(parameters => parameters
			.Add(p => p.GameId, GameId));
		// the button turns on only when both lists are read: before that it cannot know the current state
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
		// only now the web api goes down: the initial read must have succeeded
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
		var cut = Ctx.Render<SaveButtons>(parameters => parameters
			.Add(p => p.GameId, GameId));

		FavoriteButton(cut).Click();

		// NavigateTo runs before any await, so there is nothing to wait for
		var nav = Ctx.Services.GetRequiredService<NavigationManager>();
		// with the return address: after logging in the user is back on the starting page
		Assert.Contains("/account/login?returnUrl=", nav.Uri);
	}

	private static IElement FavoriteButton(IRenderedComponent<SaveButtons> cut) =>
		cut.FindAll("button").Single(b => b.TextContent.Contains("preferiti"));
}
