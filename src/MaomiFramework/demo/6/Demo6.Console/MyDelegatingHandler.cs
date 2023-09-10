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
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return httpResponseMessage;
            }
            throw new MyException(request, httpResponseMessage);
        }
        catch (Exception ex)
        {
            throw new MyException(request, httpResponseMessage);
        }
    }
    private void LogMessage(HttpRequestMessage request, HttpResponseMessage response)
    {
        string? requestContent = null;
        string? responseContent = null;
        if (request.Content != null)
        {
            requestContent = request.Content.ReadAsStringAsync().Result;
        }
        if (response != null && response.Content != null)
        {
            responseContent = response.Content.ReadAsStringAsync().Result;
        }

        var messsage =
        $"""
            Request URL:{request.RequestUri} - {request.Method.Method}
            Header: {string.Join("\r\n\t", request.Headers.Select(x => $" {x.Key} : {x.Value.FirstOrDefault()}"))}
            Request Content:
            {requestContent}
            ---
            Response Status: {response?.StatusCode} ({response?.ReasonPhrase})"
            Response Content:
            {responseContent}
            """;

        _logger.LogDebug(messsage);
    }
}
