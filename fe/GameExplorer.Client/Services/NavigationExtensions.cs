using Microsoft.AspNetCore.Components;

namespace GameExplorer.Client.Services;

public static class NavigationExtensions
{
	// after logging in the user expects to be back where they were, so the login address carries the current page.
	// A string, not a navigation, so it also works as the Href of a "Accedi" button
	public static string LoginUrl(this NavigationManager navigation)
	{
		var url = "/" + navigation.ToBaseRelativePath(navigation.Uri);
		return $"/account/login?returnUrl={Uri.EscapeDataString(url)}";
	}

	// only after a user action (a click), never while the page opens: at that moment Blazor does not replace
	// the history entry, so Back would return to the page, which would send the user to the login again
	public static void NavigateToLogin(this NavigationManager navigation, bool replace = false)
	{
		navigation.NavigateTo(navigation.LoginUrl(), replace);
	}
}
