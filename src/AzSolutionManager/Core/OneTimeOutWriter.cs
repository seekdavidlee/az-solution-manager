using System.Text.Json;

namespace AzSolutionManager.Core;

public class OneTimeOutWriter : IOneTimeOutWriter
{
    public void Write<T>(T obj, bool compress = false)
    {
        var options = compress ? new JsonSerializerOptions
        {
            WriteIndented = false
        } : new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var message = JsonSerializer.Serialize(obj, options);
        Console.Out.WriteLine(message);
    }
}


