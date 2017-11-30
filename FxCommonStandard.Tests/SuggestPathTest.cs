using System;
using System.IO;
using System.Linq;
using FxCommon.Contracts;
using FxCommon.Services;
using Moq;
using NUnit.Framework;

namespace FxCommonStandard.Tests
{
	[TestFixture]
	public class SuggestPathTest
	{
		[Test]
		public void WhenNoPathPassedTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var utils = new PathService(fileSystemService.Object);

			Assert.That(utils.SuggestPath(null), Is.Null);
			Assert.That(utils.SuggestPath(string.Empty), Is.Null);
			Assert.That(utils.SuggestPath(" "), Is.Null);
		}

		[Test]
		public void WhenPathIsNotValidoOrNotExistsTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var utils = new PathService(fileSystemService.Object);

			Assert.That(utils.SuggestPath(@"z:\" + Guid.NewGuid()), Is.Null);
			Assert.That(utils.SuggestPath("$\"!£%"), Is.Null);
		}

		[Test]
		public void WhenPathIsPassedWithoutEndingBackslashAndTheDirectoryDoNotExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals(@"c:\somedir", StringComparison.OrdinalIgnoreCase))))
				.Returns(true);

			var utils = new PathService(fileSystemService.Object);

			Assert.That(utils.SuggestPath(@"c:\somedir"), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIR"), Is.EqualTo(@"C:\SOMEDIR"));
			Assert.That(utils.SuggestPath(@" c:\somedir "), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIR "), Is.EqualTo(@"C:\SOMEDIR"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"c:\somedir\"), Is.EqualTo(@"c:\somedir\"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIR\"), Is.EqualTo(@"C:\SOMEDIR\"));
			Assert.That(utils.SuggestPath(@" c:\somedir\ "), Is.EqualTo(@"c:\somedir\"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIR\ "), Is.EqualTo(@"C:\SOMEDIR\"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"c:\somedir\"), Is.EqualTo(@"c:\somedir\subdira"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIR\"), Is.EqualTo(@"c:\somedir\subdira"));
			Assert.That(utils.SuggestPath(@" c:\somedir\ "), Is.EqualTo(@"c:\somedir\subdira"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIR\ "), Is.EqualTo(@"c:\somedir\subdira"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"c:\somedira"), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIRA"), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@" c:\somedira "), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIRA "), Is.EqualTo(@"c:\somedira"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"c:\somedira"), Is.EqualTo(@"c:\somedirb"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIRA"), Is.EqualTo(@"c:\somedirb"));
			Assert.That(utils.SuggestPath(@"c:\somedirb"), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIRB"), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@" c:\somedira "), Is.EqualTo(@"c:\somedirb"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIRA "), Is.EqualTo(@"c:\somedirb"));
			Assert.That(utils.SuggestPath(@" c:\somedirb "), Is.EqualTo(@"c:\somedira"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIRB "), Is.EqualTo(@"c:\somedira"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"c:\some"), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@"c:\somedir"), Is.EqualTo(@"c:\otherdir"));
			Assert.That(utils.SuggestPath(@"c:\otherdir"), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@"C:\SOME"), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@"C:\SOMEDIR"), Is.EqualTo(@"c:\otherdir"));
			Assert.That(utils.SuggestPath(@"C:\OTHERDIR"), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@" c:\some "), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@" c:\somedir "), Is.EqualTo(@"c:\otherdir"));
			Assert.That(utils.SuggestPath(@" c:\otherdir "), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@" C:\SOME "), Is.EqualTo(@"c:\somedir"));
			Assert.That(utils.SuggestPath(@" C:\SOMEDIR "), Is.EqualTo(@"c:\otherdir"));
			Assert.That(utils.SuggestPath(@" C:\OTHERDIR "), Is.EqualTo(@"c:\somedir"));
		}

		[Test]
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

			Assert.That(utils.SuggestPath(@"\\networkpath"), Is.EqualTo(@"\\networkpath"));
			Assert.That(utils.SuggestPath(@"\\networkpath\"), Is.EqualTo(@"\\networkpath\"));
			Assert.That(utils.SuggestPath(@"\\networkpath\share"), Is.EqualTo(@"\\networkpath\share"));
		}

		[Test]
		public void WhenNetworkPathDoesntExistsIsReturnedNull()
		{
			var fileSystemService = new Mock<IFileSystemService> { DefaultValue = DefaultValue.Mock };
			fileSystemService
				.Setup(d => d.GetDirectories(It.IsAny<string>()))
				.Throws<IOException>();

			var utils = new PathService(fileSystemService.Object);

			Assert.That(utils.SuggestPath(@"\\invalid\share\"), Is.Null);
			Assert.That(utils.SuggestPath(@"\\invalid\share\folder"), Is.Null);
			Assert.That(utils.SuggestPath(@"\\invalid\share\folder\subfolder"), Is.Null);
		}
	}
}
