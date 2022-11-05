using VerseReverse.Crawlers;
using VerseReverse.Data;
using VerseReverse.Ingestion;
using VerseReverse.Ingestion.Models;

const string DatabaseOptionsSectionName = "DatabaseOptions";
const string IngestionOptionsSectionName = "IngestionOptions";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<IngestionOptions>(context.Configuration.GetSection(IngestionOptionsSectionName));
        services.AddSingleton<IVerseCrawler, DesiringGodMessagesCrawler>();
        services.AddSqlRepository(options => context.Configuration.Bind(DatabaseOptionsSectionName, options));
        services.AddHostedService<IngestionWorker>();
    })
    .Build();

await host.RunAsync();
