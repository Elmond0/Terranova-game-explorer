using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

// Key on (user, game): the same game cannot be saved twice by the same user, no extra id needed.
// GameId has no foreign key because games live on IGDB, not here. Deleting a user also deletes their favorites.
public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
	public void Configure(EntityTypeBuilder<Favorite> builder)
	{
		builder.ToTable("favorites");

		builder.HasKey(f => new { f.UserId, f.GameId });
		builder.HasOne<User>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
	}
}
