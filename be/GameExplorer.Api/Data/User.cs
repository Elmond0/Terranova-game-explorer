using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Api.Data;

// Only the password hash is stored, never the password. MaxLength gives the columns a size limit in the database.
// Avatar is the name of one of the fixed avatars, not an image. CreatedAt in UTC because Npgsql only accepts UTC for timestamptz.

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
