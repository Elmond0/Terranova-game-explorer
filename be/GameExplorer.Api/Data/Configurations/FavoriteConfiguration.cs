using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
	public void Configure(EntityTypeBuilder<Favorite> builder)
	{
		builder.ToTable("favorites");

		builder.HasKey(f => new { f.UserId, f.GameId });
		builder.HasOne<User>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
	}
}
