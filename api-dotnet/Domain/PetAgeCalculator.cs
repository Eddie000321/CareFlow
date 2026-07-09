namespace api_dotnet.Domain;

public sealed record PetAge(
    int Years,
    int Months,
    int TotalMonths,
    DateTime AsOf,
    string Label);

public static class PetAgeCalculator
{
    public static PetAge FromBirthDate(DateTime birthDate, DateTime asOf)
    {
        var asOfDate = asOf.Date;
        var birthDateOnly = birthDate.Date;
        var days = Math.Max(0, (asOfDate - birthDateOnly).Days);

        var totalMonths =
            ((asOfDate.Year - birthDateOnly.Year) * 12)
            + (asOfDate.Month - birthDateOnly.Month)
            - (asOfDate.Day < birthDateOnly.Day ? 1 : 0);

        totalMonths = Math.Max(0, totalMonths);
        var years = totalMonths / 12;
        var months = totalMonths % 12;

        string label;
        if (days < 28)
        {
            label = $"{days}d";
        }
        else if (totalMonths < 24)
        {
            label = $"{totalMonths}m";
        }
        else
        {
            label = $"{years}y {months}m";
        }

        return new PetAge(years, months, totalMonths, asOfDate, label);
    }
}
