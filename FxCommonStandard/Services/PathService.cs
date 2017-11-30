using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FxCommon.Contracts;

namespace FxCommon.Services
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

			path = path.Trim();

			var isNetworkPath = path.StartsWith(@"\\");
			if (isNetworkPath && path.Substring(2).Count(c => c == '\\') <= 1)
				return _fileSystemService.DirectoryExists(path) ? path : null;

			if (path.EndsWith(@"\"))
			{
				if (!_fileSystemService.DirectoryExists(path))
					return null;

				string firstsubdir = _fileSystemService.GetDirectories(path).FirstOrDefault();
				if (firstsubdir != null)
					return firstsubdir;
			}
			else
			{
				string parentdir = Directory.GetParent(path).FullName;
				if (isNetworkPath && !_fileSystemService.DirectoryExists(parentdir))
					return null;
				IEnumerable<string> subdirs = _fileSystemService.GetDirectories(parentdir).ToArray();
				string nextdir = subdirs.SkipWhile(p => !path.Equals(p, StringComparison.Ordinal)).Skip(1).FirstOrDefault();
				if (nextdir != null)
					return nextdir;
				string firstmatch = subdirs.FirstOrDefault(p => p.StartsWith(path, StringComparison.Ordinal) && !p.Equals(path, StringComparison.Ordinal));
				if (firstmatch != null)
					return firstmatch;
				if (subdirs.Any())
					return subdirs.First();
			}

			return _fileSystemService.DirectoryExists(path) ? path : null;
		}
	}
}