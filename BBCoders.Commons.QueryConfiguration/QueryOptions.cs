namespace BBCoders.Commons.QueryConfiguration
{
    /// <summary>
    /// Options for query configuration
    /// </summary>
    public class QueryOptions
    {
        /// <summary>
        /// language for queries
        /// </summary>
        /// <value></value>
        public string Language { get; set; }

        /// <summary>
        /// output directory for package
        /// </summary>
        /// <value></value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// package name
        /// </summary>
        /// <value></value>
        public string PackageName { get; set; }
        /// <summary>
        /// class name
        /// </summary>
        /// <value></value>
        public string ClassName { get; set; }
        /// <summary>
        /// file name
        /// </summary>
        /// <value></value>
        public string FileName { get; set; }
        /// <summary>
        /// file extensions
        /// </summary>
        /// <value></value>
        public string FileExtension { get; set; }
        /// <summary>
        /// generates additional model file -> uses file name with Model suffix if not provided
        /// </summary>
        /// <value></value>
        public string ModelFileName { get; set; }
    }
}