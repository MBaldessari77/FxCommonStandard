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

			var isNetworkPath = path.StartsWith($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}");
			if (isNetworkPath && path.Substring(2).Count(c => c == Path.DirectorySeparatorChar) <= 1)
				return _fileSystemService.DirectoryExists(path) ? path : null;

			if (path.EndsWith($@"{Path.DirectorySeparatorChar}"))
			{
				if (!_fileSystemService.DirectoryExists(path))
					return null;

				string firstsubdir = _fileSystemService.GetDirectories(path).OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase).FirstOrDefault();
				if (firstsubdir != null)
					return firstsubdir;
			}
			else
			{
				string parentdir = Directory.GetParent(path).Name;
				parentdir = parentdir.EndsWith($"{Path.DirectorySeparatorChar}") ? parentdir : parentdir + Path.DirectorySeparatorChar;
				if (isNetworkPath && !_fileSystemService.DirectoryExists(parentdir))
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
			}

			return _fileSystemService.DirectoryExists(path) ? path : null;
		}
	}
}