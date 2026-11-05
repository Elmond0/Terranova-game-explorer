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
			.Add(p => p.Id, 3498));
		cut.WaitForAssertion(() =>
		{
			Assert.Contains("Gioco non trovato", cut.Markup);
			// Non deve sembrare un errore di rete
			Assert.DoesNotContain("Riprova", cut.Markup);
		});
	}
}
