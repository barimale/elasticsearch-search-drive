using Xunit;
using Moq;
using Nest;
using elasticsearch_search_drive_CLI.Services;
using elasticsearch_search_drive_CLI.Models;
using Elasticsearch.Net;

namespace elasticsearch_search_drive_CLI.Tests.Services
{
    public class IndexerTests : IDisposable
    {
        private readonly Mock<IElasticClient> _mockElasticClient;
        private readonly Indexer _indexer;

        public IndexerTests()
        {
            _mockElasticClient = new Mock<IElasticClient>();
            _indexer = new Indexer(_mockElasticClient.Object, "test-index");
        }

        public void Dispose()
        {
            _indexer?.Dispose();
        }

        [Fact]
        public void Constructor_WithUrl_Should_InitializeIndexer()
        {
            // Act
            var indexer = new Indexer("http://localhost:9200", "test-index");

            // Assert
            Assert.NotNull(indexer);
            indexer.Dispose();
        }

        [Fact]
        public void Constructor_WithElasticClient_Should_UseProvidedClient()
        {
            // Act
            var indexer = new Indexer(_mockElasticClient.Object, "test-index");

            // Assert
            Assert.NotNull(indexer);
            indexer.Dispose();
        }

        [Fact]
        public void IndexItem_WithValidItem_Should_CallElasticClient()
        {
            // Arrange
            var item = new DiskItem { Name = "test.txt", FullPath = "C:\\test.txt" };
            var indexResponse = new Mock<IndexResponse>();
            indexResponse.Setup(x => x.IsValid).Returns(true);

            _mockElasticClient
                .Setup(x => x.Index(It.IsAny<DiskItem>(), It.IsAny<Func<IndexDescriptor<DiskItem>, IIndexRequest<DiskItem>>>()))
                .Returns(indexResponse.Object);

            // Act
            var result = _indexer.IndexItem(item);

            // Assert
            Assert.True(result);
            _mockElasticClient.Verify(
                x => x.Index(
                    It.IsAny<DiskItem>(),
                    It.IsAny<Func<IndexDescriptor<DiskItem>, IIndexRequest<DiskItem>>>()),
                Times.Once);
        }

        [Fact]
        public void IndexItem_WithNullItem_Should_ThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _indexer.IndexItem(null!));
        }

        [Fact]
        public void IndexItem_WithInvalidResponse_Should_ReturnFalse()
        {
            // Arrange
            var item = new DiskItem { Name = "test.txt", FullPath = "C:\\test.txt" };
            var indexResponse = new Mock<IndexResponse>();
            indexResponse.Setup(x => x.IsValid).Returns(false);

            _mockElasticClient
                .Setup(x => x.Index(It.IsAny<DiskItem>(), It.IsAny<Func<IndexDescriptor<DiskItem>, IIndexRequest<DiskItem>>>()))
                .Returns(indexResponse.Object);

            // Act
            var result = _indexer.IndexItem(item);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IndexItemAsync_WithValidItem_Should_CallElasticClientAsync()
        {
            // Arrange
            var item = new DiskItem { Name = "test.txt", FullPath = "C:\\test.txt" };
            var indexResponse = new Mock<IndexResponse>();
            indexResponse.Setup(x => x.IsValid).Returns(true);

            _mockElasticClient
                .Setup(x => x.IndexAsync(
                    It.IsAny<DiskItem>(),
                    It.IsAny<Func<IndexDescriptor<DiskItem>, IIndexRequest<DiskItem>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(indexResponse.Object);

            // Act
            var result = await _indexer.IndexItemAsync(item);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IndexItems_WithEmptyList_Should_ReturnZero()
        {
            // Act
            var result = _indexer.IndexItems(new List<DiskItem>());

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void IndexItems_WithNullItems_Should_ThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _indexer.IndexItems(null!));
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
            var result = _indexer.VerifyConnection();

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
            var result = _indexer.VerifyConnection();

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
                .Setup(x => x.PingAsync(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .ReturnsAsync(pingResponse.Object);

            // Act
            var result = await _indexer.VerifyConnectionAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteItem_WithNullId_Should_ThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _indexer.DeleteItem(null!));
        }
    }
}
