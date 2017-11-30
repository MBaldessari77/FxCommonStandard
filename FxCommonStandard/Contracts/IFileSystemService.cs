using System.Collections.Generic;

namespace FxCommon.Contracts
{
	public interface IFileSystemService
	{
		bool DirectoryExists(string path);
		IEnumerable<string> GetDirectories(string path);
	}
}