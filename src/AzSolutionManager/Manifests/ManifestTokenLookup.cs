using AzSolutionManager.Core;
using FastMember;
using System.Text;

namespace AzSolutionManager.Manifests;

public class ManifestTokenLookup
{
    private readonly Dictionary<string, string> replacements = new();
    public ManifestTokenLookup(IBaseOptions options)
    {
        var t = TypeAccessor.Create(typeof(IBaseOptions));
        t.GetMembers().Where(x => x.Name.StartsWith("ASM"))
            .ToList().ForEach(x =>
            {
                replacements.Add(x.Name[3..], (string)t[options, x.Name]);
            });
    }

    /// <summary>
    /// Token replace matches within the input string.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <returns>Replaced string.</returns>
    public string Replace(string input)
    {
        StringBuilder result = new();
        int startIndex = 0;

        while (startIndex < input.Length)
        {
            int tokenStart = input.IndexOf("@(asm.", startIndex);
            if (tokenStart == -1)
            {
                // No more tokens found, append the rest of the input string and break
                result.Append(input[startIndex..]);
                break;
            }

            int tokenEnd = input.IndexOf(')', tokenStart + 6); // Skip "@(env." and find the closing ')'
            if (tokenEnd == -1)
            {
                // Malformed token, append the rest of the input string and break
                result.Append(input[startIndex..]);
                break;
            }

            // Append the part of the input before the token
            result.Append(input[startIndex..tokenStart]);

            // Extract the token and check if it exists in the replacements dictionary
            string token = input.Substring(tokenStart + 6, tokenEnd - tokenStart - 6);
            if (replacements.TryGetValue(token, out string? replacement))
            {
                // If the token exists in the dictionary, append the replacement value
                result.Append(replacement);
            }
            else
            {
                // If the token doesn't exist in the dictionary, append the original token
                result.Append("@(asm." + token + ")");
            }

            // Move the startIndex to the end of the current token
            startIndex = tokenEnd + 1;
        }

        return result.ToString();
    }
}
