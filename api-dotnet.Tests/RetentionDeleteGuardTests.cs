using api_dotnet.Controllers;
using api_dotnet.Data;
using api_dotnet.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api_dotnet.Tests;

public sealed class RetentionDeleteGuardTests
{
    [Fact]
    public async Task DeleteOwner_ReturnsNotFound_WhenOwnerDoesNotExist()
    {
        await using var db = CreateDb();
        var controller = new OwnersController(db);

        var result = await controller.DeleteOwner(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteOwner_BlocksDeletion_WhenOwnerHasPets()
    {
        await using var db = CreateDb();
        var owner = NewOwner();
        owner.Pets.Add(NewPet(owner));
        db.Owners.Add(owner);
        await db.SaveChangesAsync();
        var controller = new OwnersController(db);

        var result = await controller.DeleteOwner(owner.Id);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(
            "Cannot delete an owner with existing pets. Reassign or archive pets first.",
            badRequest.Value);
        Assert.True(await db.Owners.AnyAsync(candidate => candidate.Id == owner.Id));
    }

    [Fact]
    public async Task DeleteOwner_RemovesOwner_WhenOwnerHasNoPets()
    {
        await using var db = CreateDb();
        var owner = NewOwner();
        db.Owners.Add(owner);
        await db.SaveChangesAsync();
        var controller = new OwnersController(db);

        var result = await controller.DeleteOwner(owner.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await db.Owners.AnyAsync(candidate => candidate.Id == owner.Id));
    }

    [Fact]
    public async Task DeletePet_ReturnsNotFound_WhenPetDoesNotExist()
    {
        await using var db = CreateDb();
        var controller = new PetsController(db);

        var result = await controller.DeletePet(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePet_BlocksDeletion_WhenPetHasClinicalNotes()
    {
        await using var db = CreateDb();
        var (_, pet) = await AddOwnerAndPet(db);
        db.ClinicalNotes.Add(new ClinicalNote
        {
            PetId = pet.Id,
            Pet = pet,
            Content = "Follow-up observation"
        });
        await db.SaveChangesAsync();
        var controller = new PetsController(db);

        var result = await controller.DeletePet(pet.Id);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.True(await db.Pets.AnyAsync(candidate => candidate.Id == pet.Id));
    }

    [Fact]
    public async Task DeletePet_BlocksDeletion_WhenPetHasLabReports()
    {
        await using var db = CreateDb();
        var (_, pet) = await AddOwnerAndPet(db);
        db.LabReports.Add(new LabReport
        {
            PetId = pet.Id,
            Pet = pet,
            TestType = LabTestType.CBC
        });
        await db.SaveChangesAsync();
        var controller = new PetsController(db);

        var result = await controller.DeletePet(pet.Id);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.True(await db.Pets.AnyAsync(candidate => candidate.Id == pet.Id));
    }

    [Fact]
    public async Task DeletePet_RemovesPet_WhenClinicalHistoryIsEmpty()
    {
        await using var db = CreateDb();
        var (_, pet) = await AddOwnerAndPet(db);
        var controller = new PetsController(db);

        var result = await controller.DeletePet(pet.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await db.Pets.AnyAsync(candidate => candidate.Id == pet.Id));
    }

    private static CareflowDb CreateDb()
    {
        var options = new DbContextOptionsBuilder<CareflowDb>()
            .UseInMemoryDatabase($"careflow-tests-{Guid.NewGuid()}")
            .Options;

        return new DeleteGuardTestDb(options);
    }

    private static async Task<(Owner Owner, Pet Pet)> AddOwnerAndPet(CareflowDb db)
    {
        var owner = NewOwner();
        var pet = NewPet(owner);
        owner.Pets.Add(pet);
        db.Owners.Add(owner);
        await db.SaveChangesAsync();
        return (owner, pet);
    }

    private static Owner NewOwner() => new()
    {
        Name = "Test Owner",
        Phone = "555-0100"
    };

    private static Pet NewPet(Owner owner) => new()
    {
        Owner = owner,
        Name = "Milo",
        Species = "Canine",
        BirthDate = new DateTime(2022, 4, 3)
    };

    // These are controller unit tests, so the test model intentionally contains only
    // the relationships exercised by the delete guards. The production model's
    // complex address mapping does not affect these branches. Production relationship
    // configuration is verified separately in ModelRetentionConfigurationTests.
    private sealed class DeleteGuardTestDb(DbContextOptions<CareflowDb> options)
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
