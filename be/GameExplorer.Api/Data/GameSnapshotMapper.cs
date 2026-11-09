using System.Linq.Expressions;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Data;

public static class GameSnapshotMapper
{
	public static readonly Expression<Func<GameSnapshot, Game>> ToGame = s => new Game
	{
		Id = s.GameId,
		Slug = s.Slug,
		Name = s.Name,
		Rating = s.Rating,
		ReleaseDate = s.ReleaseDate,
		Image = s.Image == null ? null : new Image { Url = s.Image },
		Genres = s.Genres,
		Platforms = s.Platforms,
	};
}
