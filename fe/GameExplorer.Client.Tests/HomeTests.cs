using Bunit;
using GameExplorer.Client.Pages;                  // esiste già: Home
using GameExplorer.Client.Services;               // esiste già: IGameApiClient, FakeGameApiClient, ApiException
using GameExplorer.Shared.Models;                 // esiste già: Game, GameDetails, Genre, ...
using Microsoft.Extensions.DependencyInjection;   // esiste già: AddSingleton

namespace GameExplorer.Client.Tests;

public class HomeTests : PageTestBase
{
	[Fact]
	public void UnaSezioneInErrore_LeAltreMostranoIGiochi()
	{
		// Sostituisce il client della classe base: la pagina riceve l'ultimo registrato
		Ctx.Services.AddSingleton<IGameApiClient>(new UpcomingDownApiClient());
		var cut = Ctx.Render<Home>();
		cut.WaitForAssertion(() =>
		{
			// Nell'ordine della pagina: Più apprezzati, Usciti di recente, In arrivo
			var sections = cut.FindAll("section");
			Assert.Equal(3, sections.Count);
			Assert.NotEmpty(sections[0].QuerySelectorAll("a.game-card"));
			Assert.DoesNotContain("Riprova", sections[0].TextContent);
			Assert.NotEmpty(sections[1].QuerySelectorAll("a.game-card"));
			Assert.DoesNotContain("Riprova", sections[1].TextContent);
			Assert.Contains("Riprova", sections[2].TextContent);
			Assert.Empty(sections[2].QuerySelectorAll("a.game-card"));
		});
	}
	// Client finto con solo "In arrivo" giù: tutto il resto lo passa a FakeGameApiClient
	private sealed class UpcomingDownApiClient : IGameApiClient
	{
		private readonly FakeGameApiClient _fake = new();
		public Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default) =>
			throw new ApiException("In arrivo giù per il test");
		public Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default) =>
			_fake.GetTopRatedAsync(cancellationToken);
		public Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default) =>
			_fake.GetRecentAsync(cancellationToken);
		public Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
			_fake.SearchAsync(filter, cancellationToken);
		public Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.GetGameAsync(id, cancellationToken);
		public Task<List<Genre>> GetGenresAsync(CancellationToken cancellationToken = default) =>
			_fake.GetGenresAsync(cancellationToken);
		public Task<List<Platform>> GetPlatformsAsync(CancellationToken cancellationToken = default) =>
			_fake.GetPlatformsAsync(cancellationToken);
		public Task<Studio?> GetStudioAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.GetStudioAsync(id, cancellationToken);
		public Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, string? ordering, CancellationToken cancellationToken = default) =>
			_fake.GetStudioGamesAsync(id, page, ordering, cancellationToken);
	}
}
