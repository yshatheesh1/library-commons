using System;
using System.IO;

namespace BBCoders.Commons.SourceGenerator
{
    /// <summary>
    /// source context
    /// </summary>
    public abstract class SourceContext
    {
        /// <summary>
        /// adds source file
        /// </summary>
        /// <param name="source">source text</param>
        public void Add(string source) => Add(source, GenerateId());

        /// <summary>
        /// add source file
        /// </summary>
        /// <param name="source">source text</param>
        /// <param name="filename">file name</param>
        public void Add(string source, string filename) => Add(source, filename, "cs");

        /// <summary>
        /// add source file
        /// </summary>
        /// <param name="source">source text</param>
        /// <param name="filename">file name</param>
        /// <param name="extension">extensions</param>
        public void Add(string source, string filename, string extension) => Add(source, filename, "cs", Directory.GetCurrentDirectory());

        /// <summary>
        /// add source file
        /// </summary>
        /// <param name="source">source text</param>
        /// <param name="filename">file name</param>
        /// <param name="extension">extension</param>
        /// <param name="directory">directory to generate file</param>        
        public abstract void Add(string source, string filename, string extension, string directory);

        private string GenerateId()
        {
            var now = DateTime.UtcNow;
            var timestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            return timestamp.ToString("yyyyMMddHHmmss");
        }
    }
}