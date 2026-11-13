namespace GameExplorer.Shared.Models;

public record ProfileResponse(string Email, string? DisplayName, string? Avatar);

public record UpdateProfileRequest(string? DisplayName, string? Avatar);

public static class Avatars
{
	public static IReadOnlyList<string> All { get; } =
		["knight", "wizard", "ranger", "rogue", "pirate", "ninja", "robot", "alien", "ghost", "dragon", "racer", "pilot"];
}
