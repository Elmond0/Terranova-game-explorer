using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("users");
		builder.Property(u => u.Email).UseCollation(AppDbContext.CaseInsensitive);
		builder.HasIndex(u => u.Email).IsUnique();
		builder.Property(u => u.DisplayName).UseCollation(AppDbContext.CaseInsensitive);
		builder.HasIndex(u => u.DisplayName).IsUnique();
	}
}
