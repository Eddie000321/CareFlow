using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using api_dotnet.Data;
using api_dotnet.Domain;
using api_dotnet.Domain.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace api_dotnet.Tests;

public sealed class ApiWorkflowIntegrationTests
{
    [Fact]
    public async Task OwnerAndPetWorkflow_EnforcesRetentionGuardAcrossHttpPipeline()
    {
        await using var factory = new CareFlowApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var ownerResponse = await client.PostAsJsonAsync("/api/owners", new
        {
            name = "HTTP Test Owner",
            phone = "555-0199",
            email = "http-test@example.com",
            ownerAddress = new
            {
                country = "CA",
                province = "ON",
                city = "Toronto",
                street = "1 Test Street",
                postalCode = "M5V 1A1"
            }
        });

        Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);
        var ownerId = await ReadIdAsync(ownerResponse);

        var petResponse = await client.PostAsJsonAsync("/api/pets", new
        {
            ownerId,
            name = "Milo",
            species = "Canine",
            breed = "Mixed",
            birthDate = "2022-04-03"
        });

        Assert.Equal(HttpStatusCode.Created, petResponse.StatusCode);
        var petId = await ReadIdAsync(petResponse);

        var getPetResponse = await client.GetAsync($"/api/pets/{petId}");
        Assert.Equal(HttpStatusCode.OK, getPetResponse.StatusCode);

        var blockedOwnerDelete = await client.DeleteAsync($"/api/owners/{ownerId}");
        Assert.Equal(HttpStatusCode.BadRequest, blockedOwnerDelete.StatusCode);
        Assert.Contains(
            "Cannot delete an owner with existing pets",
            await blockedOwnerDelete.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);

        var petDeleteResponse = await client.DeleteAsync($"/api/pets/{petId}");
        Assert.Equal(HttpStatusCode.NoContent, petDeleteResponse.StatusCode);

        var ownerDeleteResponse = await client.DeleteAsync($"/api/owners/{ownerId}");
        Assert.Equal(HttpStatusCode.NoContent, ownerDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task PostPet_ReturnsBadRequest_WhenOwnerDoesNotExist()
    {
        await using var factory = new CareFlowApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/pets", new
        {
            ownerId = 404,
            name = "Milo",
            species = "Canine",
            birthDate = "2022-04-03"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "Cannot create a pet for an owner that does not exist",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostPet_ReturnsBirthDateValidationError_WhenBirthDateIsMissing()
    {
        await using var factory = new CareFlowApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/pets", new
        {
            ownerId = 1,
            name = "Milo",
            species = "Canine"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(CreatePetRequest.BirthDate), problem.Errors.Keys);
    }

    [Fact]
    public async Task PostLabReport_ReturnsFieldValidationErrors_WhenBoundedMetadataIsTooLong()
    {
        await using var factory = new CareFlowApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/labreports", new
        {
            petId = 1,
            testType = 0,
            labName = new string('L', 129),
            status = new string('S', 33)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(LabReport.LabName), problem.Errors.Keys);
        Assert.Contains(nameof(LabReport.Status), problem.Errors.Keys);
    }

    [Fact]
    public async Task PostLabResult_ReturnsFieldValidationErrors_WhenBoundedMetadataIsTooLong()
    {
        await using var factory = new CareFlowApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/labresults", new
        {
            labReportId = 1,
            analyteCode = "CBC",
            analyteName = "Complete blood count",
            units = new string('U', 33),
            flag = new string('F', 9)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(LabResult.Units), problem.Errors.Keys);
        Assert.Contains(nameof(LabResult.Flag), problem.Errors.Keys);
    }

    private static async Task<int> ReadIdAsync(HttpResponseMessage response)
    {
        await using var body = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(body);
        return json.RootElement.GetProperty("id").GetInt32();
    }

    private sealed class CareFlowApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"careflow-http-{Guid.NewGuid()}";
        private readonly InMemoryDatabaseRoot _databaseRoot = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<CareflowDb>>();
                services.RemoveAll<DbContextOptions<CareflowDb>>();
                services.RemoveAll<CareflowDb>();
                services.AddScoped<CareflowDb>(_ =>
                {
                    var options = new DbContextOptionsBuilder<CareflowDb>()
                        .UseInMemoryDatabase(_databaseName, _databaseRoot)
                        .Options;
                    return new HttpTestDb(options);
                });
            });
        }
    }

    private sealed class HttpTestDb(DbContextOptions<CareflowDb> options)
        : CareflowDb(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>().Ignore(owner => owner.OwnerAddress);
            modelBuilder.Entity<Owner>()
                .HasMany(owner => owner.Pets)
                .WithOne(pet => pet.Owner)
                .HasForeignKey(pet => pet.OwnerId);

            modelBuilder.Entity<Pet>()
                .HasMany(pet => pet.ClinicalNotes)
                .WithOne(note => note.Pet)
                .HasForeignKey(note => note.PetId);

            modelBuilder.Entity<Pet>()
                .HasMany(pet => pet.LabReports)
                .WithOne(report => report.Pet)
                .HasForeignKey(report => report.PetId);

            modelBuilder.Entity<LabReport>()
                .HasMany(report => report.Results)
                .WithOne(result => result.Report)
                .HasForeignKey(result => result.LabReportId);
        }
    }
}
