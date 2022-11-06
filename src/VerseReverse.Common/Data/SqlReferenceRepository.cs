using System.Data;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReferenceDto = VerseReverse.Data.Models.Reference;
using VerseReverse.Models;
using Dapper;

namespace VerseReverse.Data;

internal class SqlReferenceRepository : IDataRepository
{
    private const int AddChunkSize = 50;
    private const string GetArticleIdQuery = "SELECT Id FROM [dbo].[Articles] WHERE [Url]=@Url;";
    private const string InsertArticleQuery = "INSERT INTO [dbo].[Articles] VALUES(@Provider, @Url);";
    private const string GetReferenceIdQuery = "SELECT Id FROM [dbo].[References] WHERE [Book]=@Book AND [Chapter]=@Chapter AND [Verse]=@Verse;";
    private const string GetReferenceIdQueryNoVerse = "SELECT Id FROM [dbo].[References] WHERE [Book]=@Book AND [Chapter]=@Chapter AND [Verse] IS NULL;";
    private const string InsertReferenceQuery = "INSERT INTO [dbo].[References] VALUES(@Book, @Chapter, @Verse);";

    private readonly SqlReferenceRepositoryOptions _options;
    private readonly ILogger _logger;

    public SqlReferenceRepository(ILogger<SqlReferenceRepository> logger, IOptions<SqlReferenceRepositoryOptions> options, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        using var scope = serviceProvider?.CreateScope() ?? throw new ArgumentNullException(nameof(serviceProvider));
        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateUp();
    }

    private IDbConnection DbConnection => new SqlConnection(_options.ConnectionString);

    public async Task<int> AddArticleReferencesAsync(Article article, IEnumerable<Reference> references)
    {
        var ret = 0;
        var referencesToAdd = new List<ReferenceDto>();

        foreach (var reference in references)
        {
            if (!reference.EndVerse.HasValue || !reference.Verse.HasValue)
            {
                referencesToAdd.Add(new ReferenceDto(0, (int)reference.Book, reference.Chapter, reference.Verse));
            }
            else
            {
                for (var verse = reference.Verse.Value; verse <= reference.EndVerse.Value; verse++)
                {
                    referencesToAdd.Add(new ReferenceDto(0, (int)reference.Book, reference.Chapter, verse));
                }
            }

            if (referencesToAdd.Count >= AddChunkSize)
            {
                ret += await InternalAddReferencesAsync(article, referencesToAdd).ConfigureAwait(false);
                referencesToAdd = new List<ReferenceDto>();
            }
        }

        if (referencesToAdd.Count > 0)
        {
            ret += await InternalAddReferencesAsync(article, referencesToAdd).ConfigureAwait(false);
        }

        return ret;
    }

    private async Task<int> InternalAddReferencesAsync(Article article, List<ReferenceDto> referencesToAdd)
    {
        var articleId = await GetOrInsertArticleAsync(article).ConfigureAwait(false);
        var referenceIds = new List<int>();
        foreach (var reference in referencesToAdd)
        {
            referenceIds.Add(await GetOrInsertReferenceAsync(reference).ConfigureAwait(false));
        }

        const string GetArticleReferenceIds = "SELECT ReferenceId FROM [ArticleXReference] WHERE ArticleId=@ArticleId";
        using var connection = DbConnection;
        var alreadyReferenced = await connection.QueryAsync<int>(GetArticleReferenceIds, new { articleId }).ConfigureAwait(false);

        var toAdd = referenceIds.Where(i => !alreadyReferenced.Contains(i)).Select(i => new { ArticleId = articleId, ReferenceId = i });
        const string AddArticleReferences = "INSERT INTO [ArticleXReference] VALUES (@ArticleId, @ReferenceId)";
        var ret = await connection.ExecuteAsync(AddArticleReferences, toAdd).ConfigureAwait(false);

        return ret;
    }

    public IEnumerable<Reference> GetReferencesForArticle(string url)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Article> GetArticlesForReference(Reference reference, IEnumerable<string>? providers, bool includeChapterOnlyMatch, int pageSize, int page)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<Reference, int>> GetChaptersByReferenceCount(IEnumerable<string>? providers, int take)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<Reference, int>> GetVersesByReferenceCount(IEnumerable<string>? providers, int take)
    {
        throw new NotImplementedException();
    }

    // TODO : Eliminate duplicate code
    private async Task<int> GetOrInsertArticleAsync(Article article)
    {
        using var connection = DbConnection;
        var articleId = await connection.QuerySingleOrDefaultAsync<int>(GetArticleIdQuery, article).ConfigureAwait(false);
        if (articleId == 0)
        {
            await connection.ExecuteAsync(InsertArticleQuery, article).ConfigureAwait(false);
            articleId = await connection.QuerySingleOrDefaultAsync<int>(GetArticleIdQuery, article).ConfigureAwait(false);
        }

        return articleId;
    }

    private async Task<int> GetOrInsertReferenceAsync(ReferenceDto reference)
    {
        var getQuery = reference.Verse.HasValue
            ? GetReferenceIdQuery
            : GetReferenceIdQueryNoVerse;

        using var connection = DbConnection;
        var referenceId = await connection.QuerySingleOrDefaultAsync<int>(getQuery, reference).ConfigureAwait(false);
        if (referenceId == 0)
        {
            await connection.ExecuteAsync(InsertReferenceQuery, reference).ConfigureAwait(false);
            referenceId = await connection.QuerySingleOrDefaultAsync<int>(getQuery, reference).ConfigureAwait(false);
        }

        return referenceId;
    }
}
