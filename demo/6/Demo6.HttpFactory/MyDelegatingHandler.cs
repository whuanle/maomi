using Microsoft.Extensions.Logging;

public class MyDelegatingHandler : DelegatingHandler
{
	private readonly ILogger<MyDelegatingHandler> _logger;

	public MyDelegatingHandler(ILogger<MyDelegatingHandler> logger)
	{
		_logger = logger;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		HttpResponseMessage httpResponseMessage = null;
		try
		{
			httpResponseMessage = await base.SendAsync(request, cancellationToken);
#if DEBUG
			_logger.LogDebug(MyException.CreateMessage(request, httpResponseMessage));
#endif
			if (httpResponseMessage.IsSuccessStatusCode)
			{
				return httpResponseMessage;
			}
			throw new MyException(request, httpResponseMessage);
		}
		catch (Exception)
		{
			_logger.LogError(MyException.CreateMessage(request, httpResponseMessage));
			throw;
		}
	}
}
