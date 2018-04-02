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
			if (string.IsNullOrWhiteSpace(path))
				return null;

			path = path.Trim();

			if (Path.GetInvalidPathChars().Intersect(path).Any())
				return null;

			string parentdir = Path.GetDirectoryName(path);

#if DEBUG
			Console.WriteLine($"path = {path}; parentdir = {parentdir}");
#endif

			if (!_fileSystemService.DirectoryExists(parentdir))
				return null;

			string[] subdirs = _fileSystemService.GetDirectories(parentdir).OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase).ToArray();

#if DEBUG
			for (int index = 0; index < subdirs.Length; index++)
				Console.WriteLine($"subdirs[{index}] = {subdirs[index]}");
#endif

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