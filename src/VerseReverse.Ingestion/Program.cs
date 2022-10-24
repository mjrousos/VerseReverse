using VerseReverse.Crawlers;
using VerseReverse.Ingestion;
using VerseReverse.Ingestion.Models;

const string IngestionOptionsSectionName = "IngestionOptions";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<IngestionOptions>(context.Configuration.GetSection(IngestionOptionsSectionName));
        services.AddSingleton<IVerseCrawler, DesiringGodMessagesCrawler>();
        services.AddHostedService<IngestionWorker>();
    })
    .Build();

await host.RunAsync();
