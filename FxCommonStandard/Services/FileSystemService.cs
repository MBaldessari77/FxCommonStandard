using System.Collections.Generic;
using System.IO;
using FxCommon.Contracts;

namespace FxCommon.Services
{
	public class FileSystemService : IFileSystemService
	{
		public bool DirectoryExists(string path) { return Directory.Exists(path); }
		public IEnumerable<string> GetDirectories(string path) { return Directory.GetDirectories(path); }
	}
}