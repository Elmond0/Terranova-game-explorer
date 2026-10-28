using System.Net;
// using System.Net.Http.Json;
// using GameExplorer.Api.Services.Rawg;
// using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Services;

public class RawgException : Exception
{
	public RawgException( string msg, Exception? inner = null ) : base(msg, inner) {}
}