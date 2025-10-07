using System;
using System.Linq.Expressions;

namespace api_dotnet.Data.expressions;

public static class DateDiffExpressions
{
    public static readonly Expression<Func<DateTime, DateTime, int>> YearsFromBirth = 
        (birthDate, currentDate) => (currentDate.Year - birthDate.Year);
    
    public static readonly Expression<Func<DateTime, DateTime, int>> YearsAgo = 
        (date, currentDate) => (currentDate.Year - date.Year);

    public static readonly Expression<Func<DateTime, DateTime, int>> MonthsFromBirth = 
        (birthDate, currentDate) => ((currentDate.Year - birthDate.Year) * 12) + (currentDate.Month - birthDate.Month);

    public static readonly Expression<Func<DateTime, DateTime, int>> AgeInTotalMonths =
        (birthDate, currentDate) =>
            ((currentDate.Year - birthDate.Year) * 12)
            + (currentDate.Month - birthDate.Month)
            - (currentDate.Day < birthDate.Day ? 1 : 0); 
}