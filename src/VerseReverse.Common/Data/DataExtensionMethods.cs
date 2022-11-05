using FluentMigrator.Runner;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace VerseReverse.Data;

public static class DataExtensionMethods
{
    public static IServiceCollection AddSqlRepository(this IServiceCollection services, Action<SqlReferenceRepositoryOptions>? configureOptions = null)
    {
        services.AddOptions<SqlReferenceRepositoryOptions>()
            .Configure(configureOptions ?? (_ => { }))
            .ValidateDataAnnotations();
        services.AddSingleton<IDataRepository, SqlReferenceRepository>();

        var options = new SqlReferenceRepositoryOptions();
        configureOptions?.Invoke(options);
        services.AddFluentMigratorCore()
            .ConfigureRunner(c => c.AddSqlServer()
              .WithGlobalConnectionString(options.ConnectionString)
              .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

        return services;
    }
}
