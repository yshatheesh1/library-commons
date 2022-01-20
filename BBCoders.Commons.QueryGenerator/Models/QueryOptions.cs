namespace BBCoders.Commons.QueryGenerator
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
        public Language Language { get; set; }

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
        /// some language require module name as well
        /// </summary>
        /// <value></value>
        public string ModuleName { get; set; }
        /// <summary>
        /// generates additional model file
        /// </summary>
        /// <value></value>
        public string ModelFileName { get; set; }
        /// <summary>
        /// generates additional model within the given package
        /// </summary>
        /// <value></value>
        public string ModelPackageName { get; set; }
        /// <summary>
        /// generates additional model file within the given directory
        /// </summary>
        /// <value></value>
        public string ModelOutputDirectory { get; set; }
    }
}