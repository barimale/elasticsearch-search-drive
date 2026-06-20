using elasticsearch_search_drive_CLI.Models;
using Nest;

namespace elasticsearch_search_drive_CLI.Services
{
    /// <summary>
    /// Handles searching for DiskItem objects in Elasticsearch using NEST client.
    /// </summary>
    public class SearchEngine : IDisposable
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _indexName;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the SearchEngine class.
        /// </summary>
        /// <param name="elasticsearchUrl">Base URL of the Elasticsearch instance (e.g., "http://localhost:9200")</param>
        /// <param name="indexName">Name of the Elasticsearch index for disk items (default: "disk-items")</param>
        public SearchEngine(string elasticsearchUrl, string indexName = "disk-items")
        {
            _indexName = indexName;

            var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
                .DefaultIndex(_indexName)
                .DisableDirectStreaming()
                .PrettyJson();

            _elasticClient = new ElasticClient(settings);
        }

        /// <summary>
        /// Initializes a new instance of the SearchEngine class with an existing IElasticClient.
        /// </summary>
        /// <param name="elasticClient">The NEST ElasticClient instance</param>
        /// <param name="indexName">Name of the Elasticsearch index for disk items (default: "disk-items")</param>
        public SearchEngine(IElasticClient elasticClient, string indexName = "disk-items")
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _indexName = indexName;
        }

        /// <summary>
        /// Searches for DiskItems by name (partial match) synchronously.
        /// </summary>
        /// <param name="searchTerm">The search term to find in file/directory names</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing matching DiskItems</returns>
        public ISearchResponse<DiskItem> SearchByName(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Name)
                            .Query(searchTerm))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by name '{searchTerm}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for DiskItems by name (partial match) asynchronously.
        /// </summary>
        /// <param name="searchTerm">The search term to find in file/directory names</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing matching DiskItems</returns>
        public async Task<ISearchResponse<DiskItem>> SearchByNameAsync(string searchTerm, 
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Name)
                            .Query(searchTerm))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by name '{searchTerm}' asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for DiskItems by file extension synchronously.
        /// </summary>
        /// <param name="extension">File extension to search for (e.g., ".txt", ".pdf")</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing matching DiskItems</returns>
        public ISearchResponse<DiskItem> SearchByExtension(string extension, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(extension))
                    throw new ArgumentException("Extension cannot be empty", nameof(extension));

                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.Extension)
                            .Value(extension))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by extension '{extension}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for DiskItems by file extension asynchronously.
        /// </summary>
        /// <param name="extension">File extension to search for (e.g., ".txt", ".pdf")</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing matching DiskItems</returns>
        public async Task<ISearchResponse<DiskItem>> SearchByExtensionAsync(string extension, 
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(extension))
                    throw new ArgumentException("Extension cannot be empty", nameof(extension));

                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.Extension)
                            .Value(extension))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by extension '{extension}' asynchronously: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for DiskItems by path (partial match) synchronously.
        /// </summary>
        /// <param name="pathTerm">Part of the path to search for</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing matching DiskItems</returns>
        public ISearchResponse<DiskItem> SearchByPath(string pathTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathTerm))
                    throw new ArgumentException("Path term cannot be empty", nameof(pathTerm));

                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.FullPath)
                            .Query(pathTerm))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by path '{pathTerm}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for DiskItems by path (partial match) asynchronously.
        /// </summary>
        /// <param name="pathTerm">Part of the path to search for</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing matching DiskItems</returns>
        public async Task<ISearchResponse<DiskItem>> SearchByPathAsync(string pathTerm, 
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathTerm))
                    throw new ArgumentException("Path term cannot be empty", nameof(pathTerm));

                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.FullPath)
                            .Query(pathTerm))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by path '{pathTerm}' asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for DiskItems using a custom query synchronously.
        /// </summary>
        /// <param name="queryAction">Function to define the custom query</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing matching DiskItems</returns>
        public ISearchResponse<DiskItem> SearchCustom(Func<QueryContainerDescriptor<DiskItem>, QueryContainer> queryAction, 
            int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (queryAction == null)
                    throw new ArgumentNullException(nameof(queryAction));

                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(queryAction));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing custom search: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for DiskItems using a custom query asynchronously.
        /// </summary>
        /// <param name="queryAction">Function to define the custom query</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing matching DiskItems</returns>
        public async Task<ISearchResponse<DiskItem>> SearchCustomAsync(Func<QueryContainerDescriptor<DiskItem>, QueryContainer> queryAction,
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (queryAction == null)
                    throw new ArgumentNullException(nameof(queryAction));

                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(queryAction), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing custom search asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for all directories synchronously.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing all directories</returns>
        public ISearchResponse<DiskItem> GetAllDirectories(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.IsDirectory)
                            .Value(true))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all directories: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for all directories asynchronously.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing all directories</returns>
        public async Task<ISearchResponse<DiskItem>> GetAllDirectoriesAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.IsDirectory)
                            .Value(true))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all directories asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for all files (non-directories) synchronously.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing all files</returns>
        public ISearchResponse<DiskItem> GetAllFiles(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.IsDirectory)
                            .Value(false))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all files: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for all files (non-directories) asynchronously.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing all files</returns>
        public async Task<ISearchResponse<DiskItem>> GetAllFilesAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.IsDirectory)
                            .Value(false))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all files asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches for all items in a specific directory synchronously.
        /// </summary>
        /// <param name="directoryPath">The parent directory path</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <returns>Search results containing items in the specified directory</returns>
        public ISearchResponse<DiskItem> SearchByParentPath(string directoryPath, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                    throw new ArgumentException("Directory path cannot be empty", nameof(directoryPath));

                var from = (pageNumber - 1) * pageSize;

                var response = _elasticClient.Search<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.ParentPath)
                            .Value(directoryPath))));

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by parent path '{directoryPath}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for all items in a specific directory asynchronously.
        /// </summary>
        /// <param name="directoryPath">The parent directory path</param>
        /// <param name="pageNumber">Page number (starting from 1, default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns search results containing items in the specified directory</returns>
        public async Task<ISearchResponse<DiskItem>> SearchByParentPathAsync(string directoryPath, 
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                    throw new ArgumentException("Directory path cannot be empty", nameof(directoryPath));

                var from = (pageNumber - 1) * pageSize;

                var response = await _elasticClient.SearchAsync<DiskItem>(s => s
                    .Index(_indexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.ParentPath)
                            .Value(directoryPath))), cancellationToken);

                LogSearchErrors(response);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching by parent path '{directoryPath}' asynchronously: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the total count of items in the index synchronously.
        /// </summary>
        /// <returns>The total number of items indexed</returns>
        public long GetTotalCount()
        {
            try
            {
                var response = _elasticClient.Count<DiskItem>(c => c.Index(_indexName));
                return response.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting total count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the total count of items in the index asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns the total number of items indexed</returns>
        public async Task<long> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _elasticClient.CountAsync<DiskItem>(c => c.Index(_indexName), cancellationToken);
                return response.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting total count asynchronously: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Verifies that Elasticsearch is accessible.
        /// </summary>
        /// <returns>True if Elasticsearch is accessible; otherwise false</returns>
        public bool VerifyConnection()
        {
            try
            {
                var response = _elasticClient.Ping();
                if (!response.IsValid)
                {
                    Console.WriteLine($"Elasticsearch ping failed: {response.ServerError?.Error?.Reason}");
                }
                return response.IsValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pinging Elasticsearch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies that Elasticsearch is accessible asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the async operation. Returns true if Elasticsearch is accessible; otherwise false</returns>
        public async Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _elasticClient.PingAsync();
                if (!response.IsValid)
                {
                    Console.WriteLine($"Elasticsearch ping failed: {response.ServerError?.Error?.Reason}");
                }
                return response.IsValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pinging Elasticsearch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Logs search errors to console.
        /// </summary>
        private void LogSearchErrors(ISearchResponse<DiskItem> response)
        {
            if (!response.IsValid)
            {
                Console.WriteLine($"Search error: {response.ServerError?.Error?.Reason}");
            }
        }

        /// <summary>
        /// Disposes of the resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_elasticClient is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}

