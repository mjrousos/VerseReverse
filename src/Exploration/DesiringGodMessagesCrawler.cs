using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Abot2.Crawler;
using Abot2.Poco;
using Exploration.Models;
using HtmlAgilityPack;

namespace VerseReverse;

public class DesiringGodMessagesCrawler
{
    private const string CrawlerName = "DesiringGodMessages";
    private const string InitialUrl = "https://www.desiringgod.org/messages/all";

    private static readonly Regex MessageListRegex = new (@"^https:\/\/www\.desiringgod\.org\/messages\/all(\?page=\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex MessageRegex = new (@"^https:\/\/www\.desiringgod\.org\/messages\/[a-zA-Z0-9\-]+$", RegexOptions.Compiled | RegexOptions.Singleline);
    private readonly IEnumerable<string> _urlsToSkip;

#if CustomCrawler
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentQueue<string> _crawlerQueue;

    public DesiringGodMessagesCrawler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    async IAsyncEnumerable<Reference> GetReferences(IEnumerable<string> urlsToSkip, CancellationToken ct)
    {

    }
#endif // CustomCrawler

    public DesiringGodMessagesCrawler(IEnumerable<string> urlsToSkip)
    {
        _urlsToSkip = urlsToSkip;
    }

    public async IAsyncEnumerable<Reference> GetReferences(CancellationTokenSource cts)
    {
        var config = new CrawlConfiguration
        {
            MaxPagesToCrawl = 10_000,
            MinCrawlDelayPerDomainMilliSeconds = 5000,
            MaxPagesToCrawlPerDomain = 10_000,
        };

        var references = Channel.CreateUnbounded<Reference>();
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

    private void ExtractReferences(CrawledPage crawledPage, ChannelWriter<Reference> referenceWriter)
    {
        if (!IsMessage(crawledPage.Uri))
        {
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(crawledPage.Content.Text);

        var mainElement = doc.DocumentNode.SelectSingleNode("//main");

        // TODO
        foreach (var reference in mainElement.InnerHtml.GetPassages())
        {
            Console.WriteLine($"{crawledPage.Uri}: {reference}");
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
