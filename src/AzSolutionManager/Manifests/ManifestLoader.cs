using System.Text.Json;

namespace AzSolutionManager.Manifests;

public class ManifestLoader
{
    private readonly ManifestOptions options;

    public ManifestLoader(ManifestOptions options)
    {
        this.options = options;
    }

    private Manifest? manifest;

    public Manifest Get()
    {
        if (manifest is not null)
        {
            return manifest;
        }

        if (options.FilePath is null)
        {
            throw new Exception("Filepath is required.");
        }

        string content = File.ReadAllText(options.FilePath);

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("Unexpected for manifest content to be empty.");
        }

        var m = JsonSerializer.Deserialize<Manifest>(content) ?? throw new Exception("Unexpected for manifest null.");
        m.Validate();

        manifest = m;
        return manifest;
    }
}