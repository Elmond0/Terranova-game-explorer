using Bunit;
using GameExplorer.Client.Pages;   // esiste già: GameDetailsPage

namespace GameExplorer.Client.Tests;

public class GameDetailsPageTests : PageTestBase
{
	[Fact]
	public void GiocoInesistente_MostraGiocoNonTrovato()
	{
		// Id che non c'è nei dati del client finto
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 999999));
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Gioco non trovato", cut.Markup);
			// Non deve sembrare un errore di rete
			Assert.DoesNotContain("Riprova", cut.Markup);
		});
	}
	[Fact]
	public void ApiGiu_MostraErrore_RiprovaCaricaIlGioco()
	{
		// La Web API è giù: ogni chiamata del client finto lancia ApiException
		Api.SimulateError = true;
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() => Assert.Contains("Riprova", cut.Markup));
		// La rete torna su e l'utente clicca "Riprova"
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
}


