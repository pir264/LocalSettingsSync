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

        var patternList = patterns.ToList();
        var excludedDirs = CompileExcludePatterns(patternList).ToList();
        var compiledPatterns = CompilePatterns(patternList).ToList();
        if (compiledPatterns.Count == 0)
            return Array.Empty<FileMatch>();

        var results = new List<FileMatch>();

        foreach (var absolutePath in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceFolder, absolutePath);

            if (excludedDirs.Count > 0)
            {
                var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var dirSegments = segments.Take(segments.Length - 1);
                if (dirSegments.Any(seg => excludedDirs.Any(rx => rx.IsMatch(seg))))
                    continue;
            }

            var fileName = Path.GetFileName(absolutePath);
            if (compiledPatterns.Any(rx => rx.IsMatch(fileName)))
            {
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

    private static IEnumerable<Regex> CompileExcludePatterns(IEnumerable<string> patterns)
    {
        foreach (var raw in patterns)
        {
            var trimmed = raw.Trim();
            if (!trimmed.StartsWith('!')) continue;
            var dir = trimmed[1..].Trim();
            if (string.IsNullOrEmpty(dir)) continue;

            Regex compiled;
            try
            {
                compiled = new Regex(dir, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (ArgumentException)
            {
                compiled = new Regex("^" + Regex.Escape(dir) + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            yield return compiled;
        }
    }

    private static IEnumerable<Regex> CompilePatterns(IEnumerable<string> patterns)
    {
        foreach (var raw in patterns)
        {
            var trimmed = raw.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith('!'))
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
