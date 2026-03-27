using System.IO;
using System.Text.RegularExpressions;
using LocalSettingsSync.Models;

namespace LocalSettingsSync.Services;

public interface IFileScanner
{
    IReadOnlyList<FileMatch> Scan(string sourceFolder, IEnumerable<string> patterns);
}

public class FileScanner : IFileScanner
{
    public IReadOnlyList<FileMatch> Scan(string sourceFolder, IEnumerable<string> patterns)
    {
        if (!Directory.Exists(sourceFolder))
            return Array.Empty<FileMatch>();

        var compiledPatterns = CompilePatterns(patterns).ToList();
        if (compiledPatterns.Count == 0)
            return Array.Empty<FileMatch>();

        var results = new List<FileMatch>();

        foreach (var absolutePath in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(absolutePath);
            if (compiledPatterns.Any(rx => rx.IsMatch(fileName)))
            {
                var relativePath = Path.GetRelativePath(sourceFolder, absolutePath);
                results.Add(new FileMatch
                {
                    AbsolutePath = absolutePath,
                    RelativePath = relativePath,
                    FileName = fileName
                });
            }
        }

        results.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    private static IEnumerable<Regex> CompilePatterns(IEnumerable<string> patterns)
    {
        foreach (var raw in patterns)
        {
            var trimmed = raw.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            Regex? compiled = null;
            try
            {
                compiled = new Regex(trimmed, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                // Basic sanity check: if the pattern looks like a plain filename (no regex metacharacters),
                // it would have been compiled successfully but will work as expected.
                // If it has metacharacters and compiled fine, use it as a regex.
            }
            catch (ArgumentException)
            {
                // Not valid regex — treat as literal filename
                compiled = new Regex("^" + Regex.Escape(trimmed) + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            yield return compiled;
        }
    }
}
