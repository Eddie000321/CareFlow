using api_dotnet.Data;
using api_dotnet.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api_dotnet.Tests;

public sealed class ModelRetentionConfigurationTests
{
    public static TheoryData<Type, Type, DeleteBehavior> RelationshipCases => new()
    {
        { typeof(Pet), typeof(Owner), DeleteBehavior.Restrict },
        { typeof(ClinicalNote), typeof(Pet), DeleteBehavior.Restrict },
        { typeof(LabReport), typeof(Pet), DeleteBehavior.Restrict },
        { typeof(LabResult), typeof(LabReport), DeleteBehavior.Cascade }
    };

    [Theory]
    [MemberData(nameof(RelationshipCases))]
    public void ProductionModel_UsesExpectedDeleteBehavior(
        Type dependentType,
        Type principalType,
        DeleteBehavior expectedBehavior)
    {
        using var db = CreateProductionModelDb();
        var dependent = db.Model.FindEntityType(dependentType);
        Assert.NotNull(dependent);
        var foreignKey = Assert.Single(
            dependent.GetForeignKeys(),
            candidate => candidate.PrincipalEntityType.ClrType == principalType);

        Assert.Equal(expectedBehavior, foreignKey.DeleteBehavior);
    }

    private static CareflowDb CreateProductionModelDb()
    {
        var options = new DbContextOptionsBuilder<CareflowDb>()
            .UseNpgsql("Host=localhost;Database=careflow_model_test;Username=test;Password=test")
            .Options;

        return new CareflowDb(options);
    }
}
