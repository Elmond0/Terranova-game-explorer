using Bunit;
using GameExplorer.Client.Pages;

namespace GameExplorer.Client.Tests;

public class GameDetailsPageTests : PageTestBase
{
	[Fact]
	public void GiocoInesistente_MostraGiocoNonTrovato()
	{
		// an id missing from the fake data, like a deleted game
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 999999));
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Gioco non trovato", cut.Markup);
			// a missing game must not look like a network error: "Riprova" would never help
			Assert.DoesNotContain("Riprova", cut.Markup);
		});
	}
	[Fact]
	public void ApiGiu_MostraErrore_RiprovaCaricaIlGioco()
	{
		// the web api is down: every call of the fake client throws ApiException
		Api.SimulateError = true;
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() => Assert.Contains("Riprova", cut.Markup));
		// the network comes back and the user clicks "Riprova"
		Api.SimulateError = false;
		cut.FindAll("button")
			.Single(b => b.TextContent.Contains("Riprova"))
			.Click();
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Grand Theft Auto V", cut.Markup);
			Assert.DoesNotContain("Riprova", cut.Markup);
		});
	}
	[Fact]
	public void CambioId_NonMostraIlGiocoPrecedente()
	{
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() => Assert.Contains("Grand Theft Auto V", cut.Markup));
		// like a link from one game page to another: Blazor reuses the page and only the Id changes
		cut.Render(parameters => parameters
			.Add(p => p.Id, 4200));
		// right after, without waiting: showing the old game under the new address would mislead the user
		Assert.DoesNotContain("Grand Theft Auto V", cut.Markup);
		Assert.Contains("Caricamento...", cut.Markup);
		cut.WaitForAssertion(() => Assert.Contains("Portal 2", cut.Markup));
	}
}


