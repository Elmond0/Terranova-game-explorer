using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace GameExplorer.Api.Services.IgdbDtos;

internal static class IgdbMapper
{
	private static List<TOut> MapTo<TIn, TOut>(this IEnumerable<TIn>? source, Func<TIn, TOut> map) =>
		source?
			.Select(map)
			.ToList()
		?? new List<TOut>();
	
	public static DateOnly? FromUnix(long? unixSeconds) =>
		unixSeconds is > 0
			? DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime)
			: null;

	public static long? ToUnix(DateOnly? date) =>
		date is null
			? null
			: new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();
	
	public static Game				ToGame( this IgdbGame r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Rating = r.Rating,
		Image = r.Cover?.ToImage(),
		ReleaseDate = FromUnix(r.Released),

		Platforms = r.Platforms.MapTo( p => new Platform
		{
			Id = p.Id,
			Slug = p.Slug,
			Name = p.Name,
		}),

		Genres = r.Genres.MapTo( g => new Genre
		{
			Id = g.Id,
			Slug = g.Slug,
			Name = g.Name,
		})
	};
	
	public static Image ToImage( this IgdbImage r ) => new()
	{
		Id = r.Id,
		Url = r.Path,
		Height = r.Height,
		Width = r.Width
	};

	public static Studio ToStudio( this IgdbStudio r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};
	
	// public static Studio ToPlatform( this IgdbItem r ) => new()
	// {
	// 	Id = r.Id,
	// 	Slug = r.Slug,
	// 	Name = r.Name,
	// };

	public static GameDetails		ToDetails( this IgdbGameDetails r )
	{
		var _base = r.ToGame();

		return new GameDetails
		{
			Id = _base.Id,
			Slug = _base.Slug,
			Name = _base.Name,
			Rating = _base.Rating,
			Image = _base.Image,
			ReleaseDate = _base.ReleaseDate,
			Platforms = _base.Platforms,
			Genres = _base.Genres,

			Description = r.Description,
			Storyline = r.StoryLine,
			Website = r.Websites?.FirstOrDefault(w => w.Type == 1)?.Url,
			
			Developers = r.Studios.MapTo(ToStudio),

			Screenshots = r.Screenshots.MapTo(ToImage)
		};
	}
}