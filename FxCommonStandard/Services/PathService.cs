using System;
using System.IO;
using System.Linq;
using FxCommonStandard.Contracts;

namespace FxCommonStandard.Services
{
	public class PathService
	{
		readonly IFileSystemService _fileSystemService;

		public PathService(IFileSystemService fileSystemService) { _fileSystemService = fileSystemService; }

		public string SuggestPath(string path)
		{
			if (string.IsNullOrEmpty(path) || path.Trim() == string.Empty)
				return null;

			if (Path.GetInvalidPathChars().Intersect(path).Any())
				return null;

			if (Path.GetDirectoryName(path) == string.Empty)
				return null;

			path = path.Trim();

			string parentdir = Path.GetDirectoryName(path);
#if DEBUG
			Console.WriteLine($"path = {path}; parentdir = {parentdir}");
#endif
			if (parentdir == null || !_fileSystemService.DirectoryExists(parentdir))
				return null;

			string[] subdirs = _fileSystemService.GetDirectories(parentdir).OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase).ToArray();

			for (var index = 0; index < subdirs.Length; index++)
			{
				string subdir = subdirs[index];
				if (subdir.Equals(path, StringComparison.InvariantCultureIgnoreCase))
					return index < subdirs.Length - 1 ? subdirs[index + 1] : subdirs[0];
				if (subdir.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
					return subdir;
			}

			if (subdirs.Length > 0)
				return subdirs[0];

			return _fileSystemService.DirectoryExists(path) ? path : null;
		}
	}
}