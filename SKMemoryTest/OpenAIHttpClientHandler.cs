namespace SKMemory;

public sealed class OpenAIHttpClientHandler : HttpClientHandler
{
    private readonly string _uri;

    public OpenAIHttpClientHandler(string uri) => _uri = uri.TrimEnd('/');

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder;
        if (request.RequestUri?.LocalPath == "/v1/chat/completions")
        {
            uriBuilder = new UriBuilder(_uri + "/v1/chat/completions");
            request.RequestUri = uriBuilder.Uri;
        }
        else if (request.RequestUri?.LocalPath == "/v1/embeddings")
        {
            uriBuilder = new UriBuilder(_uri + "/v1/embeddings");
            request.RequestUri = uriBuilder.Uri;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}