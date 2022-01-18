using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using BBCoders.Commons.SourceGenerator;
using BBCoders.Commons.Utilities;

namespace BBCoders.Commons.SourceGeneratorTool
{
    public class DefaultSourceContext : SourceContext
    {
        private readonly Assembly _assembly;
        private Dictionary<string, string> sourceFiles = new Dictionary<string, string>();

        public DefaultSourceContext([NotNull] Assembly assembly)
        {
            this._assembly = assembly;
        }
        public override void Add(string source, string filename, string extension, string directory)
        {
            string path = Path.Combine(directory, $"{filename}.{extension}");
            sourceFiles.Add(path, source);
        }

        /// <summary>
        /// execute all query configurations using reflection
        /// </summary>
        public void Execute()
        {
            Reporter.WriteInformation("Starting reading source generators");
            var type = typeof(ISourceGenerator);
            var sourceGenerators = _assembly.GetTypes().Where(p => type.IsAssignableFrom(p));
            Reporter.WriteInformation($"Found {sourceGenerators.Count()} source generators");
            foreach (var sourceGenerator in sourceGenerators)
            {
                var sourceGeneratorInstance = (ISourceGenerator)Activator.CreateInstance(sourceGenerator, new object[] { });
                sourceGeneratorInstance.Execute(this);
            }
            foreach (var source in sourceFiles)
            {
                File.WriteAllText(source.Key, source.Value);
            }
        }
    }
}