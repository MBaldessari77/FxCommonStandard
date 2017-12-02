using System;
using System.IO;
using System.Linq;
using FxCommon.Contracts;
using FxCommon.Services;
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
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\somedir"));
			Assert.Equal(@"C:\SOMEDIR", utils.SuggestPath(@"C:\SOMEDIR"));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\somedir "));
			Assert.Equal(@"C:\SOMEDIR", utils.SuggestPath(@" C:\SOMEDIR "));
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndNoSubDirectoriesExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.OrdinalIgnoreCase))))
				.Returns(Enumerable.Empty<string>());

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir\", utils.SuggestPath(@"c:\somedir\"));
			Assert.Equal(@"C:\SOMEDIR\", utils.SuggestPath(@"C:\SOMEDIR\"));
			Assert.Equal(@"c:\somedir\", utils.SuggestPath(@" c:\somedir\ "));
			Assert.Equal(@"C:\SOMEDIR\", utils.SuggestPath(@" C:\SOMEDIR\ "));
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndSubDirectoriesExistsReturnFirstSubdirectories()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\somedir\", StringComparison.OrdinalIgnoreCase))))
				.Returns(new[] { @"c:\somedir\subdira" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@"c:\somedir\"));
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@"C:\SOMEDIR\"));
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@" c:\somedir\ "));
			Assert.Equal(@"c:\somedir\subdira", utils.SuggestPath(@" C:\SOMEDIR\ "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndIsOnlySubDirectoriesIsReturnedSamePath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedira", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.OrdinalIgnoreCase))))
				.Returns(new[] { @"c:\somedira" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"c:\somedira"));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"C:\SOMEDIRA"));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" c:\somedira "));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" C:\SOMEDIRA "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndExistsIsReturnedNextPath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedira", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedirb", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.OrdinalIgnoreCase))))
				.Returns(new[] { @"c:\somedira", @"c:\somedirb" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@"c:\somedira"));
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@"C:\SOMEDIRA"));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"c:\somedirb"));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@"C:\SOMEDIRB"));
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@" c:\somedira "));
			Assert.Equal(@"c:\somedirb", utils.SuggestPath(@" C:\SOMEDIRA "));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" c:\somedirb "));
			Assert.Equal(@"c:\somedira", utils.SuggestPath(@" C:\SOMEDIRB "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashIsSearchedFirstDirectoryThatStartsWithLastToken()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\some", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"c:\", StringComparison.OrdinalIgnoreCase))))
				.Returns(new[] { @"c:\somedir", @"c:\otherdir" });

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\some"));
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@"c:\somedir"));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"c:\otherdir"));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"C:\SOME"));
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@"C:\SOMEDIR"));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@"C:\OTHERDIR"));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\some "));
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@" c:\somedir "));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" c:\otherdir "));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" C:\SOME "));
			Assert.Equal(@"c:\otherdir", utils.SuggestPath(@" C:\SOMEDIR "));
			Assert.Equal(@"c:\somedir", utils.SuggestPath(@" C:\OTHERDIR "));
		}

		[Fact]
		public void WhenPathIsAnIncompletedNetworkPathIsReturnedSamePath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath\", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"\\networkpath\share", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"\\networkpath", StringComparison.OrdinalIgnoreCase))))
				.Throws<ArgumentException>();
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals(@"\\networkpath\", StringComparison.OrdinalIgnoreCase))))
				.Throws<ArgumentException>();

			var utils = new PathService(fileSystemService.Object);

			Assert.Equal(@"\\networkpath", utils.SuggestPath(@"\\networkpath"));
			Assert.Equal(@"\\networkpath\", utils.SuggestPath(@"\\networkpath\"));
			Assert.Equal(@"\\networkpath\share", utils.SuggestPath(@"\\networkpath\share"));
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
