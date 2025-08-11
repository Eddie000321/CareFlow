namespace api_dotnet.Domain;

public static class AgoLabel
{
    public static string From(int years, int months) =>
        $"{years}y {months}m ago";
}