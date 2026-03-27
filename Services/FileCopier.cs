using System.IO;
using LocalSettingsSync.Models;

namespace LocalSettingsSync.Services;

public enum ConflictResolution { Overwrite, Skip, Cancel }

public interface IFileCopier
{
    int CopyToTarget(IReadOnlyList<FileMatch> files, string targetFolder);
    (int copied, bool cancelled) CopyToSource(IReadOnlyList<FileMatch> files, string sourceFolder, Func<FileMatch, string, ConflictResolution> onConflict);
}

public class FileCopier : IFileCopier
{
    public int CopyToTarget(IReadOnlyList<FileMatch> files, string targetFolder)
    {
        int count = 0;
        foreach (var file in files)
        {
            var destination = Path.Combine(targetFolder, file.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file.AbsolutePath, destination, overwrite: true);
            count++;
        }
        return count;
    }

    public (int copied, bool cancelled) CopyToSource(IReadOnlyList<FileMatch> files, string sourceFolder, Func<FileMatch, string, ConflictResolution> onConflict)
    {
        int count = 0;
        foreach (var file in files)
        {
            var destination = Path.Combine(sourceFolder, file.RelativePath);

            if (File.Exists(destination))
            {
                var resolution = onConflict(file, destination);
                if (resolution == ConflictResolution.Cancel)
                    return (count, true);
                if (resolution == ConflictResolution.Skip)
                    continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file.AbsolutePath, destination, overwrite: true);
            count++;
        }
        return (count, false);
    }
}
