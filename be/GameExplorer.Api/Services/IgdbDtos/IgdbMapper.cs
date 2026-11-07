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
		Image = r.Cover!.ToImage(),
		ReleaseDate = FromUnix(r.Released),

		Platforms = r.Platforms.MapTo( p => new Platform
		{
			Id = r.Id,
			Slug = r.Slug,
			Name = r.Name,
		}),

		Genres = r.Genres.MapTo( p => new Genre
		{
			Id = r.Id,
			Slug = r.Slug,
			Name = r.Name,
		})
	};
	
	public static Image ToImage( this IgdbImage r ) => new()
	{
		Id = r.Id,
		Url = r.Url,
		Height = r.Height,
		Width = r.Width
	};

	public static Studio ToStudio( this IgdbStudio r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Image = r.BackgroundImage,
		GamesCount = r.GamesCount,
	};

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
			
			Developers = r.Developers.MapTo(ToStudio),
			
			Publishers = r.Publishers.MapTo(ToStudio),

			Screenshots = r.Screenshots.MapTo(ToImage)
		};
	}

	public static PagedResult<TOut>	ToPage<TIn, TOut>( this IgdbList<TIn> r, int page, int pageSize, Func<TIn, TOut> map) => new()
	{
		Items = r.Results.MapTo(map),
		Total = r.Count,
		Page = page,
		PageSize = pageSize,
		IsLast = r.Next is null,
	};
}