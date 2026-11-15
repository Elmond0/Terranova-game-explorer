using Bunit;
using GameExplorer.Client.Pages;

namespace GameExplorer.Client.Tests;

public class GameDetailsPageTests : PageTestBase
{
	[Fact]
	public void GiocoInesistente_MostraGiocoNonTrovato()
	{
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
		Api.SimulateError = true;
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() => Assert.Contains("Riprova", cut.Markup));
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


