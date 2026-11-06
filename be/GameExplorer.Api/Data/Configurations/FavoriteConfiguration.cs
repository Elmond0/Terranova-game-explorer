using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace GameExplorer.Api.Data.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
	public void Configure(EntityTypeBuilder<Favorite> builder)
	{
		builder.ToTable("favorites");

		builder.HasKey(f => new { f.UserId, f.GameId });
		builder.HasOne<IdentityUser>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
		builder.HasOne<GameSnapshot>().WithMany().HasForeignKey(f => f.GameId).OnDelete(DeleteBehavior.Restrict);
	}
}