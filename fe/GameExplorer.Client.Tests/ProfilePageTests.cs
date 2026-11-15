using Bunit;
using GameExplorer.Client.Pages;
using GameExplorer.Client.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace GameExplorer.Client.Tests;

public class ProfilePageTests : PageTestBase
{
	public ProfilePageTests()
	{
		// as in RegisterPageTests: only needed to create the page, the web api is never called
		Ctx.Services.AddHttpClient("Account", client => client.BaseAddress = new Uri("http://localhost/"));
		Ctx.Services.AddScoped<CookieAuthenticationStateProvider>();
	}

	[Fact]
	public void SalvaSenzaNickname_SpiegaPercheNonSalva()
	{
		Ctx.Render<MudPopoverProvider>();
		Auth.SetAuthorized("tester@example.com");
		var cut = Ctx.Render<ProfilePage>();
		cut.WaitForAssertion(() => Assert.Equal("Tester", cut.Find("input").GetAttribute("value")));

		// a user who just registered has no nickname: "Salva" used to do nothing without saying why
		cut.Find("input").Input("");
		// InvokeAsync: the field updates asynchronously, so the button must be looked up afterwards
		cut.InvokeAsync(() => cut.FindAll("button").First(b => b.TextContent.Trim() == "Salva").Click());

		cut.WaitForAssertion(() =>
			Assert.Contains("Nickname: Da 3 a 30 caratteri: lettere, numeri e _", cut.Markup));
	}
}
