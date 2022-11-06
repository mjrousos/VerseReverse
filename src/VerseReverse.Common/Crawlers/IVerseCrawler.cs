using VerseReverse.Models;

namespace VerseReverse.Crawlers
{
    public interface IVerseCrawler
    {
        string ProviderName { get; }

        IAsyncEnumerable<ArticleReference> GetReferences(IEnumerable<string> urlsToSkip, CancellationTokenSource cts);
    }
}
