using Xunit;
using Moq;
using Nest;
using elasticsearch_search_drive_CLI.Services;
using elasticsearch_search_drive_CLI.Models;

namespace elasticsearch_search_drive_CLI.Tests.Services
{
    public class SearchEngineTests : IDisposable
    {
        private readonly Mock<IElasticClient> _mockElasticClient;
        private readonly SearchEngine _searchEngine;

        public SearchEngineTests()
        {
            _mockElasticClient = new Mock<IElasticClient>();
            _searchEngine = new SearchEngine(_mockElasticClient.Object, "test-index");
        }

        public void Dispose()
        {
            _searchEngine?.Dispose();
        }

        [Fact]
        public void Constructor_WithUrl_Should_InitializeSearchEngine()
        {
            // Act
            var searchEngine = new SearchEngine("http://localhost:9200", "test-index");

            // Assert
            Assert.NotNull(searchEngine);
            searchEngine.Dispose();
        }

        [Fact]
        public void Constructor_WithElasticClient_Should_UseProvidedClient()
        {
            // Act
            var searchEngine = new SearchEngine(_mockElasticClient.Object, "test-index");

            // Assert
            Assert.NotNull(searchEngine);
            searchEngine.Dispose();
        }

        [Fact]
        public void Constructor_WithNullElasticClient_Should_ThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SearchEngine("", "test-index"));
        }

        [Fact]
        public void SearchByName_WithValidTerm_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.SearchByName("test");

            // Assert
            Assert.NotNull(result);
            _mockElasticClient.Verify(
                x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()),
                Times.Once);
        }

        [Fact]
        public void SearchByName_WithEmptyTerm_Should_ThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _searchEngine.SearchByName(""));
        }

        [Fact]
        public void SearchByName_WithWhitespaceTerm_Should_ThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _searchEngine.SearchByName("   "));
        }

        [Fact]
        public async Task SearchByNameAsync_WithValidTerm_Should_CallElasticClientAsync()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.SearchAsync<DiskItem>(
                    It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResponse.Object);

            // Act
            var result = await _searchEngine.SearchByNameAsync("test");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SearchByExtension_WithValidExtension_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.SearchByExtension(".txt");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SearchByExtension_WithEmptyExtension_Should_ThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _searchEngine.SearchByExtension(""));
        }

        [Fact]
        public void SearchByPath_WithValidPath_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.SearchByPath("C:\\Users");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SearchByPath_WithEmptyPath_Should_ThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _searchEngine.SearchByPath(""));
        }

        [Fact]
        public void GetAllDirectories_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.GetAllDirectories();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetAllFiles_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.GetAllFiles();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SearchByParentPath_WithValidPath_Should_CallElasticClient()
        {
            // Arrange
            var searchResponse = new Mock<ISearchResponse<DiskItem>>();
            searchResponse.Setup(x => x.IsValid).Returns(true);
            searchResponse.Setup(x => x.Documents).Returns(new List<DiskItem>());
            searchResponse.Setup(x => x.Total).Returns(0);

            _mockElasticClient
                .Setup(x => x.Search<DiskItem>(It.IsAny<Func<SearchDescriptor<DiskItem>, ISearchRequest>>()))
                .Returns(searchResponse.Object);

            // Act
            var result = _searchEngine.SearchByParentPath("C:\\Users");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SearchByParentPath_WithEmptyPath_Should_ThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _searchEngine.SearchByParentPath(""));
        }

        [Fact]
        public void GetTotalCount_Should_ReturnCount()
        {
            // Arrange
            var countResponse = new Mock<CountResponse>();
            countResponse.Setup(x => x.Count).Returns(42);

            _mockElasticClient
                .Setup(x => x.Count<DiskItem>(It.IsAny<Func<CountDescriptor<DiskItem>, ICountRequest>>()))
                .Returns(countResponse.Object);

            // Act
            var result = _searchEngine.GetTotalCount();

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task GetTotalCountAsync_Should_ReturnCount()
        {
            var countResponse = Mock.Of<CountResponse>(x => x.Count == 42);

            _mockElasticClient
                .Setup(x => x.CountAsync<DiskItem>(
                    It.IsAny<Func<CountDescriptor<DiskItem>, ICountRequest>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(countResponse);


            // Act
            var result = await _searchEngine.GetTotalCountAsync();

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void VerifyConnection_WithHealthyNode_Should_ReturnTrue()
        {
            // Arrange
            var pingResponse = new Mock<PingResponse>();
            pingResponse.Setup(x => x.IsValid).Returns(true);

            _mockElasticClient
                .Setup(x => x.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(pingResponse.Object);

            // Act
            var result = _searchEngine.VerifyConnection();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyConnection_WithUnhealthyNode_Should_ReturnFalse()
        {
            // Arrange
            var pingResponse = new Mock<PingResponse>();
            pingResponse.Setup(x => x.IsValid).Returns(false);

            _mockElasticClient
                .Setup(x => x.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(pingResponse.Object);

            // Act
            var result = _searchEngine.VerifyConnection();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyConnectionAsync_WithHealthyNode_Should_ReturnTrue()
        {
            // Arrange
            var pingResponse = new Mock<PingResponse>();
            pingResponse.Setup(x => x.IsValid).Returns(true);

            _mockElasticClient
                .Setup(x => x.PingAsync())
                .ReturnsAsync(pingResponse.Object);

            // Act
            var result = await _searchEngine.VerifyConnectionAsync();

            // Assert
            Assert.True(result);
        }
    }
}
