using Microsoft.AspNetCore.Components;

namespace GameExplorer.Client.Services;

public static class NavigationExtensions
{
	// Indirizzo della pagina di accesso che, dopo l'accesso, riporta alla pagina attuale.
	// Serve anche come Href di un pulsante "Accedi".
	public static string LoginUrl(this NavigationManager navigation)
	{
		var url = "/" + navigation.ToBaseRelativePath(navigation.Uri);
		return $"/account/login?returnUrl={Uri.EscapeDataString(url)}";
	}

	// Da usare solo dopo un'azione dell'utente (un clic), non mentre la pagina si apre:
	// in quel momento Blazor non sostituisce la voce della cronologia e il tasto Indietro
	// tornerebbe sulla pagina, che rimanderebbe di nuovo all'accesso.
	public static void NavigateToLogin(this NavigationManager navigation, bool replace = false)
	{
		navigation.NavigateTo(navigation.LoginUrl(), replace);
	}
}
