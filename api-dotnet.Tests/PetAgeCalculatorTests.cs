using api_dotnet.Domain;
using Xunit;

namespace api_dotnet.Tests;

public class PetAgeCalculatorTests
{
    [Fact]
    public void FromBirthDate_UsesDaysForNewborns()
    {
        var age = PetAgeCalculator.FromBirthDate(
            new DateTime(2026, 05, 01),
            new DateTime(2026, 05, 20));

        Assert.Equal(0, age.Years);
        Assert.Equal(0, age.Months);
        Assert.Equal(0, age.TotalMonths);
        Assert.Equal("19d", age.Label);
    }

    [Fact]
    public void FromBirthDate_UsesMonthsForPetsUnderTwoYears()
    {
        var age = PetAgeCalculator.FromBirthDate(
            new DateTime(2025, 02, 21),
            new DateTime(2026, 05, 20));

        Assert.Equal(1, age.Years);
        Assert.Equal(2, age.Months);
        Assert.Equal(14, age.TotalMonths);
        Assert.Equal("14m", age.Label);
    }

    [Fact]
    public void FromBirthDate_UsesYearMonthLabelForOlderPets()
    {
        var age = PetAgeCalculator.FromBirthDate(
            new DateTime(2021, 12, 25),
            new DateTime(2026, 05, 20));

        Assert.Equal(4, age.Years);
        Assert.Equal(4, age.Months);
        Assert.Equal(52, age.TotalMonths);
        Assert.Equal("4y 4m", age.Label);
    }

    [Fact]
    public void FromBirthDate_ClampsFutureBirthDates()
    {
        var age = PetAgeCalculator.FromBirthDate(
            new DateTime(2026, 06, 01),
            new DateTime(2026, 05, 20));

        Assert.Equal(0, age.Years);
        Assert.Equal(0, age.Months);
        Assert.Equal(0, age.TotalMonths);
        Assert.Equal("0d", age.Label);
    }
}
