
using Hyperbee.Migrations;
using Hyperbee.Migrations.Providers.Postgres;
using Hyperbee.Migrations.Providers.Postgres.Resources;

namespace AspireMe.Migrations.Migrations;


[Migration(1000)]
public class Initial(PostgresResourceRunner<Initial> resourceRunner) : Migration
{
    public override async Task UpAsync(CancellationToken cancellationToken = default)
    {
        await resourceRunner.SqlFromAsync([
         "CreateSchema.sql"
     ], cancellationToken);
        await resourceRunner.AllSqlFromAsync(cancellationToken);
    }
}


