public static class HttpContextExtensions
{
    private const string IfMatchHeader = "If-Match";
    private const string ETagHeader = "ETag";
    private const string GameIdRouteKey = "gameId";

    public static void SetResponseETagVersionHeader(this HttpContext httpContext, long version)
    {
        httpContext.Response.Headers[ETagHeader] = $"\"{version}\"";
    }

    public static long GetRequestIfMatchVersionHeader(this HttpContext httpContext)
    {
        var dict = httpContext.Request.Headers;

        if (!dict.TryGetValue(IfMatchHeader, out var ifMatchHeader))
            throw new InvalidOperationException($"{IfMatchHeader} header is required.");

        var ifMatchValue = ifMatchHeader.ToString().Trim('"');
        if (!long.TryParse(ifMatchValue, out var expectedVersion))
            throw new InvalidOperationException($"{IfMatchHeader} must be a valid long type. Actual: {ifMatchValue}");

        return expectedVersion;
    }

    public static Guid GetGameIdFromRoute(this HttpContext httpContext)
    {
        var routeValue = httpContext.GetRouteValue(GameIdRouteKey) 
            ?? throw new InvalidOperationException("Couldn't find gameId route value.");

        if (!Guid.TryParse(routeValue.ToString(), out var result))
            throw new InvalidOperationException($"Game Id route value must be a valid GUID. Actual: {routeValue}");

        return result;
    }
}
