using Bunit;
using GameExplorer.Client.Pages;
using GameExplorer.Client.Services;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GameExplorer.Client.Tests;

public class HomeTests : PageTestBase
{
	[Fact]
	public void UnaSezioneInErrore_LeAltreMostranoIGiochi()
	{
		// replaces the base class client: the page receives the last one registered
		Ctx.Services.AddSingleton<IGameApiClient>(new UpcomingDownApiClient());
		var cut = Ctx.Render<Home>();
		cut.WaitForAssertion(() =>
		{
			// in page order: Più apprezzati, Usciti di recente, In arrivo,
			// then the 4 platform sections (PlayStation, Xbox, Nintendo, Pc)
			var sections = cut.FindAll("section");
			Assert.Equal(7, sections.Count);
			Assert.NotEmpty(sections[0].QuerySelectorAll("a.game-card"));
			Assert.DoesNotContain("Riprova", sections[0].TextContent);
			Assert.NotEmpty(sections[1].QuerySelectorAll("a.game-card"));
			Assert.DoesNotContain("Riprova", sections[1].TextContent);
			Assert.Contains("Riprova", sections[2].TextContent);
			Assert.Empty(sections[2].QuerySelectorAll("a.game-card"));
			// one failing section must not take the others down with it
			for (var i = 3; i < 7; i++)
				Assert.DoesNotContain("Riprova", sections[i].TextContent);
		});
	}
	// only "In arrivo" is down: SimulateError would break every section, so everything else goes to FakeGameApiClient
	private sealed class UpcomingDownApiClient : IGameApiClient
	{
		private readonly FakeGameApiClient _fake = new();
		public Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default) =>
			throw new ApiException("In arrivo giù per il test");
		public Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default) =>
			_fake.GetTopRatedAsync(cancellationToken);
		public Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default) =>
			_fake.GetRecentAsync(cancellationToken);
		public Task<List<Game>> GetPlatformHighlightsAsync(int platformId, CancellationToken cancellationToken = default) =>
			_fake.GetPlatformHighlightsAsync(platformId, cancellationToken);
		public Task<ProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default) =>
			_fake.GetProfileAsync(cancellationToken);
		public Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default) =>
			_fake.UpdateProfileAsync(request, cancellationToken);
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
		public Task<List<Game>> GetFavoritesAsync(CancellationToken cancellationToken = default) =>
			_fake.GetFavoritesAsync(cancellationToken);
		public Task AddFavoriteAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.AddFavoriteAsync(id, cancellationToken);
		public Task RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.RemoveFavoriteAsync(id, cancellationToken);
		public Task<List<Game>> GetWishlistAsync(CancellationToken cancellationToken = default) =>
			_fake.GetWishlistAsync(cancellationToken);
		public Task AddToWishlistAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.AddToWishlistAsync(id, cancellationToken);
		public Task RemoveFromWishlistAsync(int id, CancellationToken cancellationToken = default) =>
			_fake.RemoveFromWishlistAsync(id, cancellationToken);
		public Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, string? ordering, CancellationToken cancellationToken = default) =>
			_fake.GetStudioGamesAsync(id, page, ordering, cancellationToken);
		public Task<List<Game>> GetStudioUpcomingAsync(int id, CancellationToken cancellationToken = default) => _fake.GetStudioUpcomingAsync(id, cancellationToken);
		public Task<PagedResult<Game>> GetTopRatedPageAsync(int page, CancellationToken cancellationToken = default) =>
			_fake.GetTopRatedPageAsync(page, cancellationToken);
		public Task<PagedResult<Game>> GetRecentPageAsync(int page, CancellationToken cancellationToken = default) =>
			_fake.GetRecentPageAsync(page, cancellationToken);
		public Task<PagedResult<Game>> GetUpcomingPageAsync(int page, CancellationToken cancellationToken = default) =>
			_fake.GetUpcomingPageAsync(page, cancellationToken);
	}
}
