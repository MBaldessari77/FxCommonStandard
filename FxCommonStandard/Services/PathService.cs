using System;
using System.Collections.Generic;
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

				string firstsubdir = _fileSystemService.GetDirectories(path).FirstOrDefault();
				if (firstsubdir != null)
					return firstsubdir;
			}
			else
			{
				string parentdir = Directory.GetParent(path).Name;
				parentdir = parentdir.EndsWith($"{Path.DirectorySeparatorChar}") ? parentdir : parentdir + Path.DirectorySeparatorChar;
				if (isNetworkPath && !_fileSystemService.DirectoryExists(parentdir))
					return null;
				IEnumerable<string> subdirs = _fileSystemService.GetDirectories(parentdir).ToArray();
				string nextdir = subdirs.SkipWhile(p => !path.Equals(p, StringComparison.InvariantCultureIgnoreCase)).Skip(1).FirstOrDefault();
				if (nextdir != null)
					return nextdir;
				if (subdirs.Any())
					return subdirs.First();
			}

			return _fileSystemService.DirectoryExists(path) ? path : null;
		}
	}
}