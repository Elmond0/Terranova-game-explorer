using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameExplorer.Api.Data.Configurations;

// Email and display name are compared ignoring case, so "Mario@x.it" and "mario@x.it" count as the same account.
// The unique index makes the database itself refuse duplicates, even if two sign-ups arrive at the same time.
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
