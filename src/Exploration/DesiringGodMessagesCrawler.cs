using System.Text.RegularExpressions;
using System.Threading.Channels;
using Abot2.Crawler;
using Abot2.Poco;
using Exploration.Models;
using HtmlAgilityPack;

namespace VerseReverse;

public class DesiringGodMessagesCrawler
{
    private const string InitialUrl = "https://www.desiringgod.org/messages/all";

    private static readonly Regex MessageListRegex = new (@"^https:\/\/www\.desiringgod\.org\/messages\/all(\?page=\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex MessageRegex = new (@"^https:\/\/www\.desiringgod\.org\/messages\/[a-zA-Z0-9\-]+$", RegexOptions.Compiled | RegexOptions.Singleline);
    private readonly IEnumerable<string> _urlsToSkip;

    public string Name => "DesiringGodMessages";

    public DesiringGodMessagesCrawler(IEnumerable<string> urlsToSkip)
    {
        _urlsToSkip = urlsToSkip;
    }

    public async IAsyncEnumerable<ArticleVerseReference> GetReferences(CancellationTokenSource cts)
    {
        var config = new CrawlConfiguration
        {
            MaxPagesToCrawl = 10_000,
            MinCrawlDelayPerDomainMilliSeconds = 5000,
            MaxPagesToCrawlPerDomain = 10_000,
        };

        var references = Channel.CreateUnbounded<ArticleVerseReference>();
        var crawler = new PoliteWebCrawler(config);
        crawler.ShouldCrawlPageDecisionMaker = ShouldCrawlPage;
        crawler.PageCrawlCompleted += (sender, args) => ExtractReferences(args.CrawledPage, references.Writer);

        var crawlTask = crawler.CrawlAsync(new Uri(InitialUrl), cts)
            .ContinueWith(
                _ =>
                {
                    references.Writer.Complete();
                    return Task.CompletedTask;
                }, TaskScheduler.Default);

        var referenceReader = references.Reader;
        while (await referenceReader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
        {
            while (referenceReader.TryRead(out var reference))
            {
                yield return reference;
            }
        }

        await crawlTask.ConfigureAwait(false);
    }

    private void ExtractReferences(CrawledPage crawledPage, ChannelWriter<ArticleVerseReference> referenceWriter)
    {
        if (!IsMessage(crawledPage.Uri))
        {
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(crawledPage.Content.Text);

        var mainElement = doc.DocumentNode.SelectSingleNode("//main");

        foreach (var reference in mainElement.InnerHtml.GetPassages())
        {
            // Write chapter references
            if (!reference.Verse.HasValue)
            {
                var articleVerseReference = new ArticleVerseReference(
                    Name,
                    crawledPage.Uri.AbsoluteUri,
                    reference.Book,
                    reference.Chapter,
                    null);

                WriteArticleReference(referenceWriter, articleVerseReference);
            }
            else
            {
                var startVerse = reference.Verse.Value;
                var endVerse = reference.EndVerse ?? startVerse;

                for (var v = startVerse; v <= endVerse; v++)
                {
                    var articleVerseReference = new ArticleVerseReference(
                        Name,
                        crawledPage.Uri.AbsoluteUri,
                        reference.Book,
                        reference.Chapter,
                        v);

                    WriteArticleReference(referenceWriter, articleVerseReference);
                }
            }
        }
    }

    private static void WriteArticleReference(ChannelWriter<ArticleVerseReference> referenceWriter, ArticleVerseReference articleVerseReference)
    {
        if (!referenceWriter.TryWrite(articleVerseReference))
        {
            // TODO : Log error
            Console.WriteLine($"ERROR: Failed to write reference");
        }
    }

    private CrawlDecision ShouldCrawlPage(PageToCrawl page, CrawlContext context) =>
        new CrawlDecision { Allow = !_urlsToSkip.Contains(page.Uri.AbsoluteUri) && (IsMessage(page.Uri) || IsMessageList(page.Uri)) };

    private bool IsMessageList(Uri uri) =>
        MessageListRegex.IsMatch(uri.AbsoluteUri);

    private bool IsMessage(Uri uri) =>
        !IsMessageList(uri)
        && MessageRegex.IsMatch(uri.AbsoluteUri);

}
