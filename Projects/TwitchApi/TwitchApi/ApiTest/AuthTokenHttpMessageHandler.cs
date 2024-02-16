namespace ApiTest;

public class AuthTokenHttpMessageHandler :  HttpClientHandler
{
    private readonly Action<HttpRequestMessage, CancellationToken> _processRequest;

    public AuthTokenHttpMessageHandler(Action<HttpRequestMessage, CancellationToken> processRequest)
    {
        _processRequest = processRequest;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _processRequest(request, cancellationToken);
        return base.SendAsync(request, cancellationToken);
    }
}