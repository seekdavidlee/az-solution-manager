using AzSolutionManager.Core;
using System.Text.Json;

namespace AzSolutionManager.Profiles;

public class ProfileClient
{
    private readonly IOneTimeOutWriter oneTimeOutWriter;
    private static readonly string directory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\.asm";
    private static readonly string filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\.asm\\profile.json";

    public ProfileClient(IOneTimeOutWriter oneTimeOutWriter)
    {
        this.oneTimeOutWriter = oneTimeOutWriter;
    }

    public void Save(string subscriptionId, string tenantId)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var o = new ProfileOut
        {
            Subscription = subscriptionId,
            TenantId = tenantId
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(o));

        oneTimeOutWriter.Write(o);
    }

    private static ProfileOut? cache;
    private static bool tested = false;

    public static ProfileOut? Get()
    {
        if (tested)
        {
            return cache;
        }

        if (File.Exists(filePath))
        {
            cache = JsonSerializer.Deserialize<ProfileOut>(File.ReadAllText(filePath));
        }

        tested = true;
        return cache;
    }

    public static void Delete()
    {
        cache = null;
        tested = false;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
