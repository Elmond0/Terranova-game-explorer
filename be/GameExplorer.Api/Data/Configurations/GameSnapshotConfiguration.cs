using GameExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

public class GameSnapshotConfiguration : IEntityTypeConfiguration<GameSnapshot>
{
	public void Configure(EntityTypeBuilder<GameSnapshot> builder)
	{
		builder.ToTable("game_snapshots", table =>
			table.HasCheckConstraint("ck_game_snapshots_rating", "rating BETWEEN 0 AND 100"));

		builder.HasKey(g => g.GameId);
		builder.Property(g => g.GameId).ValueGeneratedNever();

		builder.ComplexCollection(g => g.Genres, b => b.ToJson());
		builder.ComplexCollection(g => g.Platforms, b => b.ToJson());
	}
}
