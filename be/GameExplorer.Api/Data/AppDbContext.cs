using GameExplorer.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameExplorer.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<IdentityUser>(options)
{
	public DbSet<GameCacheEntry> GameCache => Set<GameCacheEntry>();
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);   // Identity tables

		// Each of our tables is configured in its own class (Data/Configurations/)
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
