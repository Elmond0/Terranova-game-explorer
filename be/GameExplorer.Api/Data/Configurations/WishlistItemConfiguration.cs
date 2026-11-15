using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
	public void Configure(EntityTypeBuilder<WishlistItem> builder)
	{
		builder.ToTable("wishlist");

		builder.HasKey(f => new { f.UserId, f.GameId });
		builder.HasOne<User>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
	}
}
