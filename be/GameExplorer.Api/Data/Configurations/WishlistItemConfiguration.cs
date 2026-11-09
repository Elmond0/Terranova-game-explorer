using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace GameExplorer.Api.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
	public void Configure(EntityTypeBuilder<WishlistItem> builder)
	{
		builder.ToTable("wishlist");

		builder.HasKey(f => new { f.UserId, f.GameId });
		builder.HasOne<IdentityUser>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
		builder.HasOne(f => f.Game).WithMany().HasForeignKey(f => f.GameId).OnDelete(DeleteBehavior.Restrict);
	}
}
