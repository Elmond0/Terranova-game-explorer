using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace GameExplorer.Api.Services.IgdbDtos;

internal static class IgdbMapper
{
	public const string CoverSize = "t_cover_big";
	public const string ScreenshotSize = "t_screenshot_big";
	public const string OriginalSize = "t_original";
	
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
		Image = r.Cover?.ToImage(CoverSize),
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
	
	private static string? ToImageUrl(this string? url, string oldSize, string newSize, bool png = false)
	{
		if (string.IsNullOrWhiteSpace(url))
			return null;
		var result = url.Replace(oldSize, newSize);
		if (png && result.EndsWith(".jpg"))
			result = result[..^4] + ".png";
		if (result.StartsWith("//"))
			result = "https:" + result;
		return result;
	}

	public static Image ToImage(this IgdbImage r, string size, bool png = false) => new()
	{
		Id = r.Id,
		Url = r.Path.ToImageUrl("t_thumb", size),
		Height = r.Height,
		Width = r.Width
	};
	
	public static Studio? ToCompany( this IgdbInvolvedCompany r ) => r.Studio?.ToStudio() ?? null;

	public static Studio ToStudio( this IgdbStudio r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
		Image = (r.Logo is not null ? r.Logo.ToImage(OriginalSize) : null),
		About = r.Description,
		CountryId = r.CountryId,
		StartDate = FromUnix(r.StartDate),
		StatusId = r.Status,
		Website = r.Websites?.FirstOrDefault(w => w.Type == 1)?.Url
	};
	
	public static Genre ToGenre( this IgdbGenre r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static Platform ToPlatform( this IgdbPlatform r ) => new()
	{
		Id = r.Id,
		Slug = r.Slug,
		Name = r.Name,
	};

	public static GameDetails		ToDetails( this IgdbGameDetails r )
	{
		var g = r.ToGame();
		var image = g.Image;
		if (image is not null)
			image.Url = g.Image!.Url.ToImageUrl(CoverSize, OriginalSize);

		return new GameDetails
		{
			Id = g.Id,
			Slug = g.Slug,
			Name = g.Name,
			Rating = g.Rating,
			Image = image,
			ReleaseDate = g.ReleaseDate,
			Platforms = g.Platforms,
			Genres = g.Genres,

			Description = r.Description,
			Storyline = r.StoryLine,
			Website = r.Websites?.FirstOrDefault(w => w.Type == 1)?.Url,
			
			Developers = (r.Studios ?? [])
				.Where(c => c.Developer == true)
				.Select(c => c.Studio.ToStudio())
				.ToList(),
			
			Publishers = (r.Studios ?? [])
				.Where(c => c.Publisher == true)
				.Select(c => c.Studio.ToStudio())
				.ToList(),

			Screenshots = r.Screenshots.MapTo(s => s.ToImage(ScreenshotSize))
		};
	}
}