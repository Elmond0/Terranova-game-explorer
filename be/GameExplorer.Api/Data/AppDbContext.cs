using GameExplorer.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameExplorer.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<IdentityUser>(options)
{
	public DbSet<GameSnapshot> GameSnapshots => Set<GameSnapshot>();
	public DbSet<Favorite> Favorites => Set<Favorite>();
	public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
