using System;
using System.IO;
using System.Linq;
using FxCommonStandard.Contracts;

namespace FxCommonStandard.Services
{
    public class PathService
    {
        private readonly IFileSystemService _fileSystemService;

        public PathService(IFileSystemService fileSystemService) { _fileSystemService = fileSystemService; }

        public string SuggestPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.Trim();

            if (Path.GetInvalidPathChars()
                .Intersect(path)
                .Any())
                return null;

            string parentDir = Path.GetDirectoryName(path);

            if (!_fileSystemService.DirectoryExists(parentDir))
                return null;

            string[] subDirs = _fileSystemService.GetDirectories(parentDir)
                .OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            string subDir;
            if ((subDir = MatchSubDir(path, subDirs)) != null)
                return subDir;

            if (subDirs.Length > 0)
                return subDirs[0];

            return _fileSystemService.DirectoryExists(path) ? path : null;
        }

        private static string MatchSubDir(string path, string[] subDirs)
        {

            for (var index = 0; index < subDirs.Length; index++)
            {
                string subDir = subDirs[index];
                if (subDir.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    return index < subDirs.Length - 1 ? subDirs[index + 1] : subDirs[0];
                if (subDir.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                    return subDir;
            }

            return null;
        }
    }
}