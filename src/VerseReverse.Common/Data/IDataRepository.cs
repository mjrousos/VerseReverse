using VerseReverse.Models;

namespace VerseReverse.Data
{
    public interface IDataRepository
    {
        Task<int> AddArticleReferencesAsync(Article article, IEnumerable<Reference> references);

        IEnumerable<Reference> GetReferencesForArticle(string url);

        IEnumerable<Article> GetArticlesForReference(Reference reference, IEnumerable<string>? providers, bool includeChapterOnlyMatch, int pageSize, int page);

        IEnumerable<KeyValuePair<Reference, int>> GetChaptersByReferenceCount(IEnumerable<string>? providers, int take);

        IEnumerable<KeyValuePair<Reference, int>> GetVersesByReferenceCount(IEnumerable<string>? providers, int take);
    }
}
