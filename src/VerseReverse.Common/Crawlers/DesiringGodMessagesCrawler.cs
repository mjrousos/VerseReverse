using System.Text.RegularExpressions;
using System.Threading.Channels;
using Abot2.Crawler;
using Abot2.Poco;
using HtmlAgilityPack;
using VerseReverse.Models;

namespace VerseReverse.Crawlers;

public class DesiringGodMessagesCrawler : IVerseCrawler
{
    private const string InitialUrl = "https://www.desiringgod.org/messages/all";

    private static readonly Regex MessageListRegex = new(@"^https:\/\/www\.desiringgod\.org\/messages\/all(\?page=\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex MessageRegex = new(@"^https:\/\/www\.desiringgod\.org\/messages\/[a-zA-Z0-9\-]+$", RegexOptions.Compiled | RegexOptions.Singleline);

    public string Name => "DesiringGodMessages";

    public DesiringGodMessagesCrawler()
    {
    }

    public async IAsyncEnumerable<ArticleReference> GetReferences(IEnumerable<string> urlsToSkip, CancellationTokenSource cts)
    {
        var config = new CrawlConfiguration
        {
            MaxPagesToCrawl = 10_000,
            MinCrawlDelayPerDomainMilliSeconds = 5000,
            MaxPagesToCrawlPerDomain = 10_000,
        };

        var references = Channel.CreateUnbounded<ArticleReference>();
        var crawler = new PoliteWebCrawler(config);
        crawler.ShouldCrawlPageDecisionMaker = (page, _) => ShouldCrawlPage(page, urlsToSkip);
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

    private void ExtractReferences(CrawledPage crawledPage, ChannelWriter<ArticleReference> referenceWriter)
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
            var articleVerseReference = new ArticleReference(
                Name,
                crawledPage.Uri.AbsoluteUri,
                new (reference.Book, reference.Chapter, reference.Verse, reference.EndVerse));

            WriteArticleReference(referenceWriter, articleVerseReference);
        }
    }

    private static void WriteArticleReference(ChannelWriter<ArticleReference> referenceWriter, ArticleReference articleVerseReference)
    {
        if (!referenceWriter.TryWrite(articleVerseReference))
        {
            // TODO : Log error
            Console.WriteLine($"ERROR: Failed to write reference");
        }
    }

    private CrawlDecision ShouldCrawlPage(PageToCrawl page, IEnumerable<string> urlsToSkip) =>
        new CrawlDecision { Allow = !urlsToSkip.Contains(page.Uri.AbsoluteUri) && (IsMessage(page.Uri) || IsMessageList(page.Uri)) };

    private bool IsMessageList(Uri uri) =>
        MessageListRegex.IsMatch(uri.AbsoluteUri);

    private bool IsMessage(Uri uri) =>
        !IsMessageList(uri)
        && MessageRegex.IsMatch(uri.AbsoluteUri);
}
