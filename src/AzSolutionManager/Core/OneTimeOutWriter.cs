using System.Text.Json;

namespace AzSolutionManager.Core;

public class OneTimeOutWriter : IOneTimeOutWriter
{
    private static readonly JsonSerializerOptions DefaultOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions CompressedOptions = new() { WriteIndented = false };

    public void Write<T>(T obj, bool compress = false)
    {
        var options = compress ? CompressedOptions : DefaultOptions;
        var message = JsonSerializer.Serialize(obj, options);
        Console.Out.WriteLine(message);
    }
}


