using Bunit;
using GameExplorer.Client.Pages;                // esiste già: ProfilePage
using GameExplorer.Client.Services.Auth;        // esiste già: CookieAuthenticationStateProvider
using Microsoft.Extensions.DependencyInjection; // AddHttpClient
using MudBlazor;                                // esiste già: MudPopoverProvider

namespace GameExplorer.Client.Tests;

public class ProfilePageTests : PageTestBase
{
	public ProfilePageTests()
	{
		// Come in RegisterPageTests: serve solo a creare la pagina, la Web API non viene chiamata
		Ctx.Services.AddHttpClient("Account", client => client.BaseAddress = new Uri("http://localhost/"));
		Ctx.Services.AddScoped<CookieAuthenticationStateProvider>();
	}

	[Fact]
	public void SalvaSenzaNickname_SpiegaPercheNonSalva()
	{
		Ctx.Render<MudPopoverProvider>();
		Auth.SetAuthorized("tester@example.com");
		var cut = Ctx.Render<ProfilePage>();
		// Il profilo finto arriva dopo 800 ms con il nickname "Tester"
		cut.WaitForAssertion(() => Assert.Equal("Tester", cut.Find("input").GetAttribute("value")));

		// Chi si è appena registrato non ha il nickname: prima "Salva" non faceva niente
		cut.Find("input").Input("");
		// InvokeAsync: il campo si aggiorna in modo asincrono e il pulsante va cercato dopo
		cut.InvokeAsync(() => cut.FindAll("button").First(b => b.TextContent.Trim() == "Salva").Click());

		cut.WaitForAssertion(() =>
			Assert.Contains("Nickname: Da 3 a 30 caratteri: lettere, numeri e _", cut.Markup));
	}
}
