using api_dotnet.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api_dotnet.Tests;

public sealed class MigrationSchemaContractTests
{
    [Fact]
    public void LatestMigrationSnapshot_MatchesRuntimeModel()
    {
        var options = new DbContextOptionsBuilder<CareflowDb>()
            .UseNpgsql("Host=localhost;Database=careflow_schema_contract;Username=test;Password=test")
            .Options;

        using var db = new CareflowDb(options);

        Assert.False(
            db.Database.HasPendingModelChanges(),
            "The runtime EF model and the latest checked-in migration snapshot must remain synchronized.");
    }
}
