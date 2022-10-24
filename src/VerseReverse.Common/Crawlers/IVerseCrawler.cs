using VerseReverse.Models;

namespace VerseReverse.Crawlers
{
    public interface IVerseCrawler
    {
        string Name { get; }

        IAsyncEnumerable<ArticleVerseReference> GetReferences(CancellationTokenSource cts);
    }
}