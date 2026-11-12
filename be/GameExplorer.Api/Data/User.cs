using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Api.Data;

public class User
{
	public int Id { get; set; }
	[MaxLength(256)] public string Email { get; set; } = "";
	public string PasswordHash { get; set; } = "";
	public Guid SessionStamp { get; set; } = Guid.NewGuid();
	[MaxLength(30)] public string? DisplayName { get; set; }
	[MaxLength(30)] public string? Avatar { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
