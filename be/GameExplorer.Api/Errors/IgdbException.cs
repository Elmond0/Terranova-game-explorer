namespace GameExplorer.Api.Services;

public class IgdbException( string msg, Exception? inner = null ) : Exception(msg, inner);