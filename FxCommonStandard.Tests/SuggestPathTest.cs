using System;
using System.IO;
using System.Linq;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class SuggestPathTest
	{
		[Fact]
		public void WhenNoPathPassedTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var utils = new PathService(fileSystemService.Object);

			Assert.Null(utils.SuggestPath(null));
			Assert.Null(utils.SuggestPath(string.Empty));
			Assert.Null(utils.SuggestPath(" "));
		}

		[Fact]
		public void WhenPathIsNotValidoOrNotExistsTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var utils = new PathService(fileSystemService.Object);

			Assert.Null(utils.SuggestPath(@"z:\" + Guid.NewGuid()));
			Assert.Null(utils.SuggestPath("$\"!£%"));
		}

		[Fact]
		public void WhenPathIsPassedWithoutEndingBackslashAndTheDirectoryDoNotExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\somedir"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"C:\SOMEDIR", utils.SuggestPath(@"C:\SOMEDIR"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\somedir "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"C:\SOMEDIR", utils.SuggestPath(@" C:\SOMEDIR "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndNoSubDirectoriesExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(Enumerable.Empty<string>());

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir\", utils.SuggestPath(@"c:\somedir\"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"C:\SOMEDIR\", utils.SuggestPath(@"C:\SOMEDIR\"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir\", utils.SuggestPath(@" c:\somedir\ "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"C:\SOMEDIR\", utils.SuggestPath(@" C:\SOMEDIR\ "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndSubDirectoriesExistsReturnFirstSubdirectories()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { @"c:\somedir\subdira" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@"c:\somedir\"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@"C:\SOMEDIR\"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@" c:\somedir\ "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@" C:\SOMEDIR\ "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndIsOnlySubDirectoriesIsReturnedSamePath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedira", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { @"c:\somedira" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"c:\somedira"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"C:\SOMEDIRA"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" c:\somedira "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" C:\SOMEDIRA "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndExistsIsReturnedNextPath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedira", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedirb", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { @"c:\somedira", @"c:\somedirb" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@"c:\somedira"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@"C:\SOMEDIRA"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"c:\somedirb"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"C:\SOMEDIRB"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@" c:\somedira "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@" C:\SOMEDIRA "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" c:\somedirb "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" C:\SOMEDIRB "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashIsSearchedFirstDirectoryThatStartsWithLastToken()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\some", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { @"c:\somedir", @"c:\otherdir" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\some"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\otherdir"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"C:\SOME"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@"C:\SOMEDIR"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"C:\OTHERDIR"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\some "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@" c:\somedir "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\otherdir "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" C:\SOME "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@" C:\SOMEDIR "), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" C:\OTHERDIR "), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenPathIsAnIncompletedNetworkPathIsReturnedSamePath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath\", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath\share", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"\\networkpath", StringComparison.InvariantCultureIgnoreCase))))
				.Throws<ArgumentException>();
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"\\networkpath\", StringComparison.InvariantCultureIgnoreCase))))
				.Throws<ArgumentException>();

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"\\networkpath", utils.SuggestPath(@"\\networkpath"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"\\networkpath\", utils.SuggestPath(@"\\networkpath\"), StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(@"\\networkpath\share", utils.SuggestPath(@"\\networkpath\share"), StringComparer.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void WhenNetworkPathDoesntExistsIsReturnedNull()
		{
			var fileSystemService = new Mock<IFileSystemService> { DefaultValue = DefaultValue.Mock };
			fileSystemService
				.Setup(d => d.GetDirectories(It.IsAny<string>()))
				.Throws<IOException>();

			var utils = new PathService(fileSystemService.Object);

			Assert.Null(utils.SuggestPath(@"\\invalid\share\"));
			Assert.Null(utils.SuggestPath(@"\\invalid\share\folder"));
			Assert.Null(utils.SuggestPath(@"\\invalid\share\folder\subfolder"));
		}
	}
}
