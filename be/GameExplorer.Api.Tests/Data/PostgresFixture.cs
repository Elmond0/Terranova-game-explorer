using GameExplorer.Api.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace GameExplorer.Api.Tests.Data;

public class PostgresFixture : IAsyncLifetime
{
	readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6-trixie").Build();

	public async Task InitializeAsync()
	{
		await _container.StartAsync();

		await using AppDbContext db = CreateContext();
		await db.Database.MigrateAsync();
	}

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();

	public AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_container.GetConnectionString())
			.UseSnakeCaseNamingConvention()
			.Options);
}
