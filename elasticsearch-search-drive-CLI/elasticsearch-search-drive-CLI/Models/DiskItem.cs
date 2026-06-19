namespace elasticsearch_search_drive_CLI.Models
{
    /// <summary>
    /// Represents a file or directory item on the disk to be indexed.
    /// </summary>
    public class DiskItem
    {
        /// <summary>
        /// Unique identifier for the item.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Full path of the file or directory.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Name of the file or directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// File extension (e.g., ".txt", ".pdf"). Empty string for directories.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Size in bytes. 0 for directories.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Indicates whether the item is a directory.
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Date and time the item was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time the item was last modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Date and time the item was last accessed.
        /// </summary>
        public DateTime? AccessedDate { get; set; }

        /// <summary>
        /// Optional content or metadata summary for text files.
        /// </summary>
        public string ContentSummary { get; set; }

        /// <summary>
        /// Parent directory path.
        /// </summary>
        public string ParentPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the DiskItem class.
        /// </summary>
        public DiskItem()
        {
            Id = Guid.NewGuid().ToString();
            ContentSummary = string.Empty;
        }

        /// <summary>
        /// Returns a string representation of the DiskItem.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({(IsDirectory ? "Directory" : Extension)}) - {(SizeBytes / 1024.0):F2} KB";
        }
    }
}
