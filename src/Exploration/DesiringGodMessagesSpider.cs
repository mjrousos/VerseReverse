using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Scheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VerseReverse;

#if DotNetSpider
internal class DesiringGodMessagesSpider : Spider
{
    private const string InitialUrl = "https://www.desiringgod.org/messages/all";

    public DesiringGodMessagesSpider(
        IOptions<SpiderOptions> options,
        DependenceServices services,
        ILogger<Spider> logger) : base(
        options, services, logger)
    {
    }

    protected override Task InitializeAsync(CancellationToken ct = default)
    {
        var builder = Builder.CreateDefaultBuilder<DesiringGodMessagesSpider>(options =>
        {
            options.Depth = 1000;
        });
        builder.UseDownloader<HttpClientDownloader>();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}
#endif // DotNetSpider
