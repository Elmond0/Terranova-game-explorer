namespace GameExplorer.Client.Services;
public class SearchFocusService
{
	public event Func<Task>? Requested;
	public bool HasSearchBar => Requested is not null;
	private bool _pending;
	public async Task RequestAsync()
	{
		var handler = Requested;
		if (handler is not null)
			await handler();
		else
			_pending = true;
	}
	public bool ConsumePending()
	{
		bool ret = _pending;
		_pending = false;
		return ret;
	}
}
