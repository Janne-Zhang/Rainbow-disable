namespace Rainbow.Kismet.Infrastructure
{
    public interface IContentTypeProvider
    {
        bool TryGetContentType(string subpath, out string contentType);
    }
}
