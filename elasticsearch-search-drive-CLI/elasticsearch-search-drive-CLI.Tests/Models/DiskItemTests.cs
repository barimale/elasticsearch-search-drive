using Xunit;
using elasticsearch_search_drive_CLI.Models;

namespace elasticsearch_search_drive_CLI.Tests.Models
{
    public class DiskItemTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithEmptyId()
        {
            // Act
            var item = new DiskItem();

            // Assert
            Assert.NotNull(item.Id);
            Assert.NotEmpty(item.Id);
            Assert.Equal(36, item.Id.Length); // GUID length
        }

        [Fact]
        public void Constructor_Should_InitializeWithEmptyContentSummary()
        {
            // Act
            var item = new DiskItem();

            // Assert
            Assert.NotNull(item.ContentSummary);
            Assert.Empty(item.ContentSummary);
        }

        [Fact]
        public void ToString_Should_ReturnFileInfo()
        {
            // Arrange
            var item = new DiskItem
            {
                Name = "test.txt",
                Extension = ".txt",
                IsDirectory = false,
                SizeBytes = 2048
            };

            // Act
            var result = item.ToString();

            // Assert
            Assert.Contains("test.txt", result);
            Assert.Contains(".txt", result);
            Assert.Contains("2.00", result); // KB
        }

        [Fact]
        public void ToString_Should_ShowDirForDirectories()
        {
            // Arrange
            var item = new DiskItem
            {
                Name = "TestFolder",
                IsDirectory = true,
                SizeBytes = 0
            };

            // Act
            var result = item.ToString();

            // Assert
            Assert.Contains("TestFolder", result);
            Assert.Contains("Directory", result);
        }

        [Theory]
        [InlineData(1024, "1.00", "KB")]
        public void ToString_Should_FormatSizeCorrectly(long bytes, string expectedSize, string expectedUnit)
        {
            // Arrange
            var item = new DiskItem
            {
                Name = "test",
                SizeBytes = bytes,
                IsDirectory = false,
                Extension = ".txt"
            };

            // Act
            var result = item.ToString();

            // Assert
            Assert.Contains(expectedSize, result);
            Assert.Contains(expectedUnit, result);
        }

        [Fact]
        public void DiskItem_Should_AllowPropertyChanges()
        {
            // Arrange
            var item = new DiskItem();
            var now = DateTime.UtcNow;

            // Act
            item.Name = "MyFile.txt";
            item.FullPath = "C:\\Users\\MyFile.txt";
            item.Extension = ".txt";
            item.SizeBytes = 5000;
            item.CreatedDate = now;
            item.ModifiedDate = now;
            item.ParentPath = "C:\\Users";

            // Assert
            Assert.Equal("MyFile.txt", item.Name);
            Assert.Equal("C:\\Users\\MyFile.txt", item.FullPath);
            Assert.Equal(".txt", item.Extension);
            Assert.Equal(5000, item.SizeBytes);
            Assert.Equal(now, item.CreatedDate);
            Assert.Equal(now, item.ModifiedDate);
            Assert.Equal("C:\\Users", item.ParentPath);
        }
    }
}
