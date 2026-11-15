using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameExplorer.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: DbContext(options)
{
	public const string CaseInsensitive = "case_insensitive";

	public DbSet<User> Users => Set<User>();
	public DbSet<Favorite> Favorites => Set<Favorite>();
	public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

	// ICU collation "ks-level2": compares ignoring case but not accents. It must be non-deterministic, or PostgreSQL won't treat "A" and "a" as equal.
	// Each table's rules live in its own file in Configurations/, loaded here automatically.
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasCollation(CaseInsensitive, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
