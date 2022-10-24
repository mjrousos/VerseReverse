using Microsoft.Extensions.Options;
using VerseReverse.Crawlers;
using VerseReverse.Ingestion.Models;

namespace VerseReverse.Ingestion;

public class IngestionWorker : BackgroundService
{
    private readonly IEnumerable<IVerseCrawler> _crawlers;
    private readonly ILogger<IngestionWorker> _logger;
    private readonly IngestionOptions _options;

    public IngestionWorker(IOptions<IngestionOptions> options, IEnumerable<IVerseCrawler> crawlers, ILogger<IngestionWorker> logger)
    {
        _crawlers = crawlers ?? throw new ArgumentNullException(nameof(crawlers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Beginning ingestion process");

            var crawlTasks = _crawlers.Select(IngestVerses);
            await Task.WhenAll(crawlTasks);

            await WaitForTriggerTimeAsync(stoppingToken);
        }

        _logger.LogInformation("Ingestion stopped");
    }

    private async Task IngestVerses(IVerseCrawler crawler)
    {
        _logger.LogInformation("Ingesting verses with crawler {CrawlerName}", crawler.Name);

        // TODO : Get already-ingested URLs
        var urlsToSkip = Enumerable.Empty<string>();

        // TODO : Get verses and write to the database (in batches?)
        await foreach (var reference in crawler.GetReferences(urlsToSkip, new CancellationTokenSource()))
        {
            Console.WriteLine($"[{reference.Provider}] {reference.Url}: {reference.Book.ToDisplayString()} {reference.Chapter}{(reference.Verse.HasValue ? $":{reference.Verse}" : string.Empty)}");
        }

        _logger.LogInformation("Ingestion finished with crawler {CrawlerName}", crawler.Name);
    }

    private async Task WaitForTriggerTimeAsync(CancellationToken stoppingToken)
    {
        var triggerDate = DateTimeOffset.UtcNow.AddDays(1);
        var trigger = new DateTimeOffset(
            triggerDate.Year,
            triggerDate.Month,
            triggerDate.Day,
            _options.TriggerTimeUTC.Hour,
            _options.TriggerTimeUTC.Minute,
            _options.TriggerTimeUTC.Second,
            TimeSpan.Zero);

        _logger.LogInformation("Awaiting next ingestion at {IngestionTrigger}", trigger);
        await Task.Delay(trigger - DateTimeOffset.UtcNow, stoppingToken);
    }
}
