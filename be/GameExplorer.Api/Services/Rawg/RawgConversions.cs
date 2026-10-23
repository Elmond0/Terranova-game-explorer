using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Services.Rawg;

internal static class RawgConversions
{
	public static Game		InGame( this RawgGame r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Rating = r.Rating,
		Image = r.BackgroundImage,
		ReleaseDate = r.Released,
		Metacritic = r.Metacritic,

		Platforms = (r.Platforms ?? [])
			.Where(w => w.Platform is not null)
			.Select(w => w.Platform!.InPlatform())
			.ToList(),

		Genres = (r.Genres ?? [])
			.Select(g => g.InGenre())
			.ToList(),
	};

	public static Platform	InPlatform( this RawgItem r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static Genre		InGenre( this RawgItem r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static GameDetails		InDetails( this RawgGameDetails r )
	{
		var base_ = r.InGame();

		return new GameDetails
		{
			Id = base_.Id,
			Slug = base_.Slug,
			Name = base_.Name,
			Rating = base_.Rating,
			Image = base_.Image,
			ReleaseDate = base_.ReleaseDate,
			Metacritic = base_.Metacritic,
			Platforms = base_.Platforms,
			Genres = base_.Genres,

			About = r.About,
			Website = r.Website,
			
			Developers = (r.Developers ?? [])
				.Select(s => s.InStudio())
				.ToList(),
			
			Publishers = (r.Publishers ?? [])
				.Select(s => s.InStudio())
				.ToList(),
			
			WhereToBuy = (r.Stores ?? [])
				.Where(s => s.Store is not null)
				.Select()
		}
	}

	public static Studio		InStudio( this RawgStudio r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Image = r.BackgroundImage,
		GamesCount = r.GamesCount,
		
	}
}