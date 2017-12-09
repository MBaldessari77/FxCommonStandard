using System.Collections.Generic;

namespace FxCommonStandard.Contracts
{
	public interface IFileSystemService
	{
		bool DirectoryExists(string path);
		IEnumerable<string> GetDirectories(string path);
	}
}