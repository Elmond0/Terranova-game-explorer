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
	[Fact]
	public void CambioId_NonMostraIlGiocoPrecedente()
	{
		var cut = Ctx.Render<GameDetailsPage>(parameters => parameters
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() => Assert.Contains("Grand Theft Auto V", cut.Markup));
		// Come un link da una scheda all'altra: stessa pagina, Id nuovo
		cut.Render(parameters => parameters
			.Add(p => p.Id, 4200));
		// Subito dopo, senza aspettare: il gioco vecchio non deve vedersi
		Assert.DoesNotContain("Grand Theft Auto V", cut.Markup);
		Assert.Contains("Caricamento...", cut.Markup);
		cut.WaitForAssertion(() => Assert.Contains("Portal 2", cut.Markup));
	}
}


