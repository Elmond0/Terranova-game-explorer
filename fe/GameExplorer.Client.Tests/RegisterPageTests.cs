using Bunit;
using GameExplorer.Client.Pages.Account;        // esiste già: RegisterPage
using GameExplorer.Client.Services.Auth;        // esiste già: CookieAuthenticationStateProvider
using Microsoft.Extensions.DependencyInjection; // AddHttpClient

namespace GameExplorer.Client.Tests;

public class RegisterPageTests : PageTestBase
{
	public RegisterPageTests()
	{
		// La pagina riceve CookieAuthenticationStateProvider, che vuole il client HTTP "Account".
		// Nessun test qui arriva fino alla Web API: l'indirizzo non viene mai chiamato.
		Ctx.Services.AddHttpClient("Account", client => client.BaseAddress = new Uri("http://localhost/"));
		Ctx.Services.AddScoped<CookieAuthenticationStateProvider>();
	}

	[Fact]
	public void PasswordUguali_NessunErrore()
	{
		var cut = Ctx.Render<RegisterPage>();

		cut.Find("input[name=new-password]").Change("Password1!");
		cut.Find("input[name=confirm-password]").Change("Password1!");

		Assert.DoesNotContain("Le password non coincidono", cut.Markup);
		Assert.DoesNotContain("La password deve", cut.Markup);
	}

	[Fact]
	public void ConfermaDiversa_PoiCorrettaLaPassword_LErroreSparisce()
	{
		var cut = Ctx.Render<RegisterPage>();

		cut.Find("input[name=new-password]").Change("Password1!");
		cut.Find("input[name=confirm-password]").Change("Password2!");
		Assert.Contains("Le password non coincidono", cut.Markup);

		// Si corregge la password, non la conferma: prima l'errore della conferma restava
		cut.Find("input[name=new-password]").Change("Password2!");
		Assert.DoesNotContain("Le password non coincidono", cut.Markup);
	}

	[Fact]
	public void PasswordCorta_MostraErrore_PoiCorretta_Sparisce()
	{
		var cut = Ctx.Render<RegisterPage>();

		cut.Find("input[name=new-password]").Change("abc");
		Assert.Contains("La password deve avere da 8 a 128 caratteri", cut.Markup);

		cut.Find("input[name=new-password]").Change("Password1!");
		Assert.DoesNotContain("La password deve avere da 8 a 128 caratteri", cut.Markup);
	}

	[Fact]
	public void InvioSenzaEmail_ChiedeLEmail()
	{
		var cut = Ctx.Render<RegisterPage>();

		cut.Find("input[name=new-password]").Change("Password1!");
		cut.Find("input[name=confirm-password]").Change("Password1!");
		cut.Find("form").Submit();

		cut.WaitForAssertion(() => Assert.Contains("Inserisci l", cut.Find(".mud-alert").TextContent));
	}
}
