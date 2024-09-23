class Utilities
{
    public static string? GetHost(string[] requestLines)
    {
        foreach (string line in requestLines)
        {
            if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(6).Trim();
            }
        }
        return null;
    }
}