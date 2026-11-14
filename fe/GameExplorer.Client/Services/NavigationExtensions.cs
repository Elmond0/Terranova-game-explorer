using Microsoft.AspNetCore.Components;

namespace GameExplorer.Client.Services;

public static class NavigationExtensions
{
	public static void NavigateToLogin(this NavigationManager navigation, bool replace = false)
	{
		var url = "/" + navigation.ToBaseRelativePath(navigation.Uri);
		navigation.NavigateTo($"/account/login?returnUrl={Uri.EscapeDataString(url)}", replace);
	}
}
