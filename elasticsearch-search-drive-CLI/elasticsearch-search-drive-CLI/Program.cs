using elasticsearch_search_drive_CLI.Models;
using elasticsearch_search_drive_CLI.Services;
using Nest;

namespace elasticsearch_search_drive_CLI
{
    internal class Program
    {
        private const string DefaultElasticsearchUrl = "http://localhost:9200";
        private const string DefaultIndexName = "disk-items";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No command provided.");
                PrintUsage();
                return;
            }

            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "search":
                        HandleSearchCommand(args);
                        break;
                    case "index":
                        HandleIndexCommand(args);
                        break;
                    case "help":
                        PrintUsage();
                        break;
                    default:
                        Console.WriteLine($"Invalid command: {command}");
                        PrintUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Handles the search command
        /// Usage: search [searchTerm] [--extension] [--path] [--all-files] [--all-dirs] [--parent-path=path]
        /// </summary>
        static void HandleSearchCommand(string[] args)
        {
            using (var searchEngine = new SearchEngine(DefaultElasticsearchUrl, DefaultIndexName))
            {
                // Verify connection
                if (!searchEngine.VerifyConnection())
                {
                    Console.WriteLine("Error: Cannot connect to Elasticsearch. Make sure it's running at " + DefaultElasticsearchUrl);
                    return;
                }

                Console.WriteLine("Connected to Elasticsearch successfully!");

                // Get total count
                var totalCount = searchEngine.GetTotalCount();
                Console.WriteLine($"Total items in index: {totalCount}\n");

                if (args.Length < 2)
                {
                    Console.WriteLine("Search usage: search [searchTerm] [options]");
                    Console.WriteLine("Options:");
                    Console.WriteLine("  --extension      Search by file extension");
                    Console.WriteLine("  --path           Search by file path");
                    Console.WriteLine("  --all-files      List all files");
                    Console.WriteLine("  --all-dirs       List all directories");
                    Console.WriteLine("  --parent-path    Search items in a specific directory\n");
                    return;
                }

                string searchTerm = args[1];
                bool isExtensionSearch = args.Length > 2 && args[2] == "--extension";
                bool isPathSearch = args.Length > 2 && args[2] == "--path";
                bool allFiles = args.Any(a => a == "--all-files");
                bool allDirs = args.Any(a => a == "--all-dirs");
                bool parentPathSearch = args.Any(a => a.StartsWith("--parent-path="));

                ISearchResponse<DiskItem> results = null;

                if (allFiles)
                {
                    Console.WriteLine("Searching for all files...\n");
                    results = searchEngine.GetAllFiles(pageSize: 20);
                }
                else if (allDirs)
                {
                    Console.WriteLine("Searching for all directories...\n");
                    results = searchEngine.GetAllDirectories(pageSize: 20);
                }
                else if (parentPathSearch)
                {
                    var parentPath = args.First(a => a.StartsWith("--parent-path=")).Split('=')[1];
                    Console.WriteLine($"Searching items in directory: {parentPath}\n");
                    results = searchEngine.SearchByParentPath(parentPath, pageSize: 20);
                }
                else if (isExtensionSearch)
                {
                    Console.WriteLine($"Searching files with extension: {searchTerm}\n");
                    results = searchEngine.SearchByExtension(searchTerm, pageSize: 20);
                }
                else if (isPathSearch)
                {
                    Console.WriteLine($"Searching by path: {searchTerm}\n");
                    results = searchEngine.SearchByPath(searchTerm, pageSize: 20);
                }
                else
                {
                    Console.WriteLine($"Searching by name: {searchTerm}\n");
                    results = searchEngine.SearchByName(searchTerm, pageSize: 20);
                }

                if (results != null && results.IsValid)
                {
                    Console.WriteLine($"Found {results.Total} results:\n");
                    int count = 0;
                    foreach (var item in results.Documents)
                    {
                        count++;
                        Console.WriteLine($"{count}. [{(item.IsDirectory ? "DIR " : "FILE")}] {item.Name}");
                        Console.WriteLine($"   Path: {item.FullPath}");
                        Console.WriteLine($"   Size: {FormatFileSize(item.SizeBytes)}");
                        Console.WriteLine($"   Modified: {item.ModifiedDate:yyyy-MM-dd HH:mm:ss}");
                        Console.WriteLine();
                    }
                }
                else if (results != null)
                {
                    Console.WriteLine("Search returned invalid results.");
                }
            }
        }

        /// <summary>
        /// Handles the index command
        /// Usage: index [directory] [--recursive]
        /// </summary>
        static void HandleIndexCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Index usage: index [directory] [--recursive]");
                Console.WriteLine("Example: index C:\\MyFiles --recursive");
                return;
            }

            string directoryPath = args[1];
            bool recursive = args.Length > 2 && args[2] == "--recursive";

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Error: Directory does not exist: {directoryPath}");
                return;
            }

            using (var indexer = new Indexer(DefaultElasticsearchUrl, DefaultIndexName))
            {
                // Verify connection
                if (!indexer.VerifyConnection())
                {
                    Console.WriteLine("Error: Cannot connect to Elasticsearch. Make sure it's running at " + DefaultElasticsearchUrl);
                    return;
                }

                Console.WriteLine("Connected to Elasticsearch successfully!");
                Console.WriteLine($"Indexing directory: {directoryPath}");
                Console.WriteLine($"Recursive: {(recursive ? "Yes" : "No")}\n");

                // Get all items to index
                var items = GetDiskItems(directoryPath, recursive);
                Console.WriteLine($"Found {items.Count} items to index.\n");

                if (items.Count > 0)
                {
                    // Index items
                    int successCount = indexer.IndexItems(items);
                    Console.WriteLine($"\nSuccessfully indexed {successCount} items.");
                }
                else
                {
                    Console.WriteLine("No items to index.");
                }
            }
        }

        /// <summary>
        /// Handles the search command asynchronously
        /// Usage: search [searchTerm] [options]
        /// </summary>
        static void HandleSearchCommandAsync(string[] args)
        {
            HandleSearchCommandAsyncInternal(args).GetAwaiter().GetResult();
        }

        static async Task HandleSearchCommandAsyncInternal(string[] args)
        {
            using (var searchEngine = new SearchEngine(DefaultElasticsearchUrl, DefaultIndexName))
            {
                // Verify connection asynchronously
                if (!await searchEngine.VerifyConnectionAsync())
                {
                    Console.WriteLine("Error: Cannot connect to Elasticsearch. Make sure it's running at " + DefaultElasticsearchUrl);
                    return;
                }

                Console.WriteLine("Connected to Elasticsearch successfully!");

                // Get total count
                var totalCount = await searchEngine.GetTotalCountAsync();
                Console.WriteLine($"Total items in index: {totalCount}\n");

                if (args.Length < 2)
                {
                    Console.WriteLine("Search usage: search [searchTerm] [options]");
                    Console.WriteLine("Options:");
                    Console.WriteLine("  --extension      Search by file extension");
                    Console.WriteLine("  --path           Search by file path");
                    Console.WriteLine("  --all-files      List all files");
                    Console.WriteLine("  --all-dirs       List all directories");
                    Console.WriteLine("  --parent-path    Search items in a specific directory\n");
                    return;
                }

                string searchTerm = args[1];
                bool isExtensionSearch = args.Length > 2 && args[2] == "--extension";
                bool isPathSearch = args.Length > 2 && args[2] == "--path";
                bool allFiles = args.Any(a => a == "--all-files");
                bool allDirs = args.Any(a => a == "--all-dirs");
                bool parentPathSearch = args.Any(a => a.StartsWith("--parent-path="));

                ISearchResponse<DiskItem> results = null;

                if (allFiles)
                {
                    Console.WriteLine("Searching for all files...\n");
                    results = await searchEngine.GetAllFilesAsync(pageSize: 20);
                }
                else if (allDirs)
                {
                    Console.WriteLine("Searching for all directories...\n");
                    results = await searchEngine.GetAllDirectoriesAsync(pageSize: 20);
                }
                else if (parentPathSearch)
                {
                    var parentPath = args.First(a => a.StartsWith("--parent-path=")).Split('=')[1];
                    Console.WriteLine($"Searching items in directory: {parentPath}\n");
                    results = await searchEngine.SearchByParentPathAsync(parentPath, pageSize: 20);
                }
                else if (isExtensionSearch)
                {
                    Console.WriteLine($"Searching files with extension: {searchTerm}\n");
                    results = await searchEngine.SearchByExtensionAsync(searchTerm, pageSize: 20);
                }
                else if (isPathSearch)
                {
                    Console.WriteLine($"Searching by path: {searchTerm}\n");
                    results = await searchEngine.SearchByPathAsync(searchTerm, pageSize: 20);
                }
                else
                {
                    Console.WriteLine($"Searching by name: {searchTerm}\n");
                    results = await searchEngine.SearchByNameAsync(searchTerm, pageSize: 20);
                }

                if (results != null && results.IsValid)
                {
                    Console.WriteLine($"Found {results.Total} results:\n");
                    int count = 0;
                    foreach (var item in results.Documents)
                    {
                        count++;
                        Console.WriteLine($"{count}. [{(item.IsDirectory ? "DIR " : "FILE")}] {item.Name}");
                        Console.WriteLine($"   Path: {item.FullPath}");
                        Console.WriteLine($"   Size: {FormatFileSize(item.SizeBytes)}");
                        Console.WriteLine($"   Modified: {item.ModifiedDate:yyyy-MM-dd HH:mm:ss}");
                        Console.WriteLine();
                    }
                }
                else if (results != null)
                {
                    Console.WriteLine("Search returned invalid results.");
                }
            }
        }

        /// <summary>
        /// Handles the index command asynchronously
        /// Usage: index [directory] [--recursive]
        /// </summary>
        static void HandleIndexCommandAsync(string[] args)
        {
            HandleIndexCommandAsyncInternal(args).GetAwaiter().GetResult();
        }

        static async Task HandleIndexCommandAsyncInternal(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Index usage: index [directory] [--recursive]");
                Console.WriteLine("Example: index C:\\MyFiles --recursive");
                return;
            }

            string directoryPath = args[1];
            bool recursive = args.Length > 2 && args[2] == "--recursive";

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Error: Directory does not exist: {directoryPath}");
                return;
            }

            using (var indexer = new Indexer(DefaultElasticsearchUrl, DefaultIndexName))
            {
                // Verify connection asynchronously
                if (!await indexer.VerifyConnectionAsync())
                {
                    Console.WriteLine("Error: Cannot connect to Elasticsearch. Make sure it's running at " + DefaultElasticsearchUrl);
                    return;
                }

                Console.WriteLine("Connected to Elasticsearch successfully!");
                Console.WriteLine($"Indexing directory: {directoryPath}");
                Console.WriteLine($"Recursive: {(recursive ? "Yes" : "No")}\n");

                // Get all items to index
                var items = GetDiskItems(directoryPath, recursive);
                Console.WriteLine($"Found {items.Count} items to index.\n");

                if (items.Count > 0)
                {
                    // Index items asynchronously
                    int successCount = await indexer.IndexItemsAsync(items);
                    Console.WriteLine($"\nSuccessfully indexed {successCount} items.");
                }
                else
                {
                    Console.WriteLine("No items to index.");
                }
            }
        }

        /// <summary>
        /// Recursively gets all disk items from a directory
        /// </summary>
        static List<DiskItem> GetDiskItems(string directoryPath, bool recursive)
        {
            var items = new List<DiskItem>();

            try
            {
                var dirInfo = new DirectoryInfo(directoryPath);

                // Add directories
                foreach (var dir in dirInfo.GetDirectories())
                {
                    try
                    {
                        var diskItem = new DiskItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            FullPath = dir.FullName,
                            Name = dir.Name,
                            Extension = string.Empty,
                            IsDirectory = true,
                            SizeBytes = 0,
                            CreatedDate = dir.CreationTime,
                            ModifiedDate = dir.LastWriteTime,
                            AccessedDate = dir.LastAccessTime,
                            ParentPath = dir.Parent?.FullName ?? string.Empty
                        };
                        items.Add(diskItem);

                        // Recursively add items from subdirectories
                        if (recursive)
                        {
                            items.AddRange(GetDiskItems(dir.FullName, true));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Warning: Access denied to {dir.FullName}");
                    }
                }

                // Add files
                foreach (var file in dirInfo.GetFiles())
                {
                    try
                    {
                        var diskItem = new DiskItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            FullPath = file.FullName,
                            Name = file.Name,
                            Extension = file.Extension,
                            IsDirectory = false,
                            SizeBytes = file.Length,
                            CreatedDate = file.CreationTime,
                            ModifiedDate = file.LastWriteTime,
                            AccessedDate = file.LastAccessTime,
                            ParentPath = file.DirectoryName ?? string.Empty
                        };
                        items.Add(diskItem);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not process file {file.FullName}: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Warning: Access denied to {directoryPath}");
            }

            return items;
        }

        /// <summary>
        /// Formats file size for display
        /// </summary>
        static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Prints usage information
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("\n=== Elasticsearch Search Drive CLI ===\n");
            Console.WriteLine("Usage: elasticsearch-search-drive [command] [options]\n");
            Console.WriteLine("Commands:");
            Console.WriteLine("  search [term]              Search for files/directories by name");
            Console.WriteLine("    Options:");
            Console.WriteLine("      --extension            Search by file extension");
            Console.WriteLine("      --path                 Search by file path");
            Console.WriteLine("      --all-files            List all files");
            Console.WriteLine("      --all-dirs             List all directories");
            Console.WriteLine("      --parent-path=PATH     List items in a specific directory\n");

            Console.WriteLine("  index [directory]          Index files from a directory");
            Console.WriteLine("    Options:");
            Console.WriteLine("      --recursive            Index subdirectories recursively\n");

            Console.WriteLine("Examples:");
            Console.WriteLine("  elasticsearch-search-drive search document");
            Console.WriteLine("  elasticsearch-search-drive search .txt --extension");
            Console.WriteLine("  elasticsearch-search-drive search C:\\Users --path");
            Console.WriteLine("  elasticsearch-search-drive index C:\\MyFiles --recursive");
            Console.WriteLine("  elasticsearch-search-drive search --all-files");
            Console.WriteLine("  elasticsearch-search-drive search --all-dirs");
            Console.WriteLine();
        }
    }
}
