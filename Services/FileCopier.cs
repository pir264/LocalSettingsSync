using System.IO;
using LocalSettingsSync.Models;

namespace LocalSettingsSync.Services;

public enum ConflictResolution { Overwrite, OverwriteAll, Skip, SkipAll, Cancel }

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
        ConflictResolution? blanket = null;

        foreach (var file in files)
        {
            var destination = Path.Combine(sourceFolder, file.RelativePath);

            if (File.Exists(destination))
            {
                var resolution = blanket ?? onConflict(file, destination);

                switch (resolution)
                {
                    case ConflictResolution.Cancel:
                        return (count, true);
                    case ConflictResolution.Skip:
                        continue;
                    case ConflictResolution.SkipAll:
                        blanket = ConflictResolution.SkipAll;
                        continue;
                    case ConflictResolution.OverwriteAll:
                        blanket = ConflictResolution.OverwriteAll;
                        break;
                    // ConflictResolution.Overwrite: fall through to copy
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file.AbsolutePath, destination, overwrite: true);
            count++;
        }
        return (count, false);
    }
}
