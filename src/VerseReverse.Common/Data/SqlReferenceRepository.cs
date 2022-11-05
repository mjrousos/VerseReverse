using System.Data;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VerseReverse.Data;

internal class SqlReferenceRepository : IDataRepository
{
    private readonly SqlReferenceRepositoryOptions _options;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _initialized = false;
    private static readonly object SyncRoot = new object();

    public SqlReferenceRepository(ILogger<SqlReferenceRepository> logger, IOptions<SqlReferenceRepositoryOptions> options, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    private IDbConnection DbConnection
    {
        get
        {
            var connection = new SqlConnection(_options.ConnectionString);

            if (!_initialized)
            {
                lock (SyncRoot)
                {
                    if (!_initialized)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                        migrationRunner.MigrateUp();
                        _initialized = true;
                    }
                }
            }

            return connection;
        }
    }
}
