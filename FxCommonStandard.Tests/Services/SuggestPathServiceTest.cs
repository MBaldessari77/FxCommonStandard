using System;
using System.IO;
using System.Linq;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests.Services
{
	public class SuggestPathServiceTest
	{
		[Fact]
		public void WhenNoPathPassedTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var service = new PathService(fileSystemService.Object);

			Assert.Null(service.SuggestPath(null));
			Assert.Null(service.SuggestPath(string.Empty));
			Assert.Null(service.SuggestPath(" "));
		}

		[Fact]
		public void WhenPathIsNotValidoOrNotExistsTheResultIsNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();

			var service = new PathService(fileSystemService.Object);

			Assert.Null(service.SuggestPath($"z:{Path.DirectorySeparatorChar}" + Guid.NewGuid()));
			Assert.Null(service.SuggestPath("$\"!£%"));
		}

		[Fact]
		public void WhenPathIsPassedWithoutEndingBackslashAndTheDirectoryDoNotExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals("c:", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			 
			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedir"));
			Assert.Equal($"C:{Path.DirectorySeparatorChar}SOMEDIR", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIR"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedir "));
			Assert.Equal($"C:{Path.DirectorySeparatorChar}SOMEDIR", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIR "));
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndNoSubDirectoriesExistsTheSameDirectoryIsSuggested()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(Enumerable.Empty<string>());

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}"));
			Assert.Equal($"C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar}", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar}"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar} "));
			Assert.Equal($"C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar}", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar} "));
		}

		[Fact]
		public void WhenPathIsPassedWithEndingBackslashAndSubDirectoriesExistsReturnFirstSubdirectories()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { $"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}subdira" });

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}subdira", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}subdira", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar}"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}subdira", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar} "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir{Path.DirectorySeparatorChar}subdira", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIR{Path.DirectorySeparatorChar} "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndIsOnlySubDirectoriesIsReturnedSamePath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedira", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { $"c:{Path.DirectorySeparatorChar}somedira" });

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedira"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIRA"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedira "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIRA "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashAndExistsIsReturnedNextPath()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals("c:", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedira", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedirb", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[] { $"c:{Path.DirectorySeparatorChar}somedira", $"c:{Path.DirectorySeparatorChar}somedirb" });

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedirb", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedira"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedirb", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIRA"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($"c:{Path.DirectorySeparatorChar}somedirb"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIRB"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedirb", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedira "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedirb", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIRA "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedirb "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedira", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIRB "));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashIsSearchedNextDirectoryThatStartsWithLastToken()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals("c:", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}some", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}otherdir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[]
				{
					$"c:{Path.DirectorySeparatorChar}somedir",
					$"c:{Path.DirectorySeparatorChar}otherdir",
					$"c:{Path.DirectorySeparatorChar}some"
				});

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}some"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}some"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}some", service.SuggestPath($"c:{Path.DirectorySeparatorChar}otherdir"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOME"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($"C:{Path.DirectorySeparatorChar}SOMEDIR"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}some", service.SuggestPath($"C:{Path.DirectorySeparatorChar}OTHERDIR"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}some "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}somedir "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}some", service.SuggestPath($" c:{Path.DirectorySeparatorChar}otherdir "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}somedir", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOME "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" C:{Path.DirectorySeparatorChar}SOMEDIR "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}some", service.SuggestPath($" C:{Path.DirectorySeparatorChar}OTHERDIR "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}some", service.SuggestPath($" C:{Path.DirectorySeparatorChar}s"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" C:{Path.DirectorySeparatorChar}o"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" C:{Path.DirectorySeparatorChar}somedir"));
		}

		[Fact]
		public void WhenPathIsPassedWithNoEndingBackslashIsSearchedNextDirectoryThatStartsWithLastTokenAlsoInCaseOfSubdir()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}subdir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"c:{Path.DirectorySeparatorChar}subdir", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(new[]
				{
					$"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir",
					$"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir",
					$"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some"
				});

			var service = new PathService(fileSystemService.Object);

			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}SOME"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}SOMEDIR"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", service.SuggestPath($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}OTHERDIR"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}SOME "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}SOMEDIR "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}OTHERDIR "));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}some", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}s"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}o"));
			Assert.Equal($"c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}otherdir", service.SuggestPath($" c:{Path.DirectorySeparatorChar}subdir{Path.DirectorySeparatorChar}somedir"));
		}

		[Fact]
		public void WhenPathIsAnIncompletedNetworkPathIsReturnedNull()
		{
			var fileSystemService = new Mock<IFileSystemService>();
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.DirectoryExists(It.Is<string>(s => s.Equals($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath{Path.DirectorySeparatorChar}share", StringComparison.InvariantCultureIgnoreCase))))
				.Returns(true);
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath", StringComparison.InvariantCultureIgnoreCase))))
				.Throws<ArgumentException>();
			fileSystemService
				.Setup(d => d.GetDirectories(It.Is<string>(s => s.Equals($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase))))
				.Throws<ArgumentException>();

			var service = new PathService(fileSystemService.Object);

			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath"));
			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath{Path.DirectorySeparatorChar}"));
			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}networkpath{Path.DirectorySeparatorChar}share"));
		}

		[Fact]
		public void WhenNetworkPathDoesntExistsIsReturnedNull()
		{
			var fileSystemService = new Mock<IFileSystemService> { DefaultValue = DefaultValue.Mock };
			fileSystemService
				.Setup(d => d.GetDirectories(It.IsAny<string>()))
				.Throws<IOException>();

			var service = new PathService(fileSystemService.Object);

			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}invalid{Path.DirectorySeparatorChar}share{Path.DirectorySeparatorChar}"));
			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}invalid{Path.DirectorySeparatorChar}share{Path.DirectorySeparatorChar}folder"));
			Assert.Null(service.SuggestPath($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}invalid{Path.DirectorySeparatorChar}share{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}subfolder"));
		}
	}
}
