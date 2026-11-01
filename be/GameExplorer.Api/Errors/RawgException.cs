namespace GameExplorer.Api.Services;

public class RawgException : Exception
{
	public RawgException( string msg, Exception? inner = null ) : base(msg, inner) {}
}