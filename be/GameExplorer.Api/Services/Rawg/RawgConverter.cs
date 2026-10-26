using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace GameExplorer.Api.Services.Rawg;

internal static class RawgConverter
{
	public static Game				InGame( this RawgGame r ) => new()
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

	public static Platform			InPlatform( this RawgItem r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static Genre				InGenre( this RawgItem r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static GameDetails		InDetails( RawgGameDetails r, List<RawgScreenshot>? shots )
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

			Screenshots = (shots ?? [])
				.Select(s => s.Image)
				.ToList(),

			WhereToBuy = (r.Stores ?? [])
				.Select(s => s.InStoreLink())
				.ToList(),
		};
	}

	public static Studio			InStudio( this RawgStudio r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Image = r.BackgroundImage,
		GamesCount = r.GamesCount,
	};

	public static Store				InStore( this RawgStore r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Domain = r.Domain,
	};

	public static StoreLink			InStoreLink( this RawgStoreWrapper r ) => new()
	{
		Store = r.Store!.InStore(),
		Url = string.IsNullOrWhiteSpace(r.Url) ? null : r.Url,
	};

	public static PagedResult<TOut>	InPage<TIn, TOut>( this RawgList<TIn> r, int page, Func<TIn, TOut> map) => new()
	{
		Items = (r.Results ?? [])
			.Select(map)
			.ToList(),
		Total = r.Count,
		Page = page,
		IsLast = r.Next is null,
	};
}