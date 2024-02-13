
public class MyException : Exception
{
	public MyException(HttpRequestMessage request, HttpResponseMessage? response)
		: base(CreateMessage(request, response))
	{
	}

	public static string CreateMessage(HttpRequestMessage request, HttpResponseMessage? response)
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

		return messsage;
	}
}
