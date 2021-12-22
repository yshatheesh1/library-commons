using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BBCoders.Commons.Utilities.Commands
{
    /// <summary>
    /// Gets current project details
    /// </summary>
    public class Project
    {
        private readonly string _file;
        private readonly string _framework;
        private readonly string _configuration;
        private readonly string _runtime;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="file"></param>
        /// <param name="framework"></param>
        /// <param name="configuration"></param>
        /// <param name="runtime"></param>
        public Project(string file, string framework, string configuration, string runtime)
        {
            _file = file;
            _framework = framework;
            _configuration = configuration;
            _runtime = runtime;
            ProjectName = Path.GetFileName(file);
        }
        /// <summary>
        /// Project Name
        /// </summary>
        /// <value></value>
        public string ProjectName { get; }
        /// <summary>
        /// Assembly Name
        /// </summary>
        /// <value></value>
        public string AssemblyName { get; set; }
        /// <summary>
        /// Language
        /// </summary>
        /// <value></value>
        public string Language { get; set; }
        /// <summary>
        /// Output path
        /// </summary>
        /// <value></value>
        public string OutputPath { get; set; }
        /// <summary>
        /// targeted platform
        /// </summary>
        /// <value></value>
        public string PlatformTarget { get; set; }
        /// <summary>
        /// project asset files
        /// </summary>
        /// <value></value>
        public string ProjectAssetsFile { get; set; }
        /// <summary>
        /// project directory
        /// </summary>
        /// <value></value>
        public string ProjectDir { get; set; }
        /// <summary>
        /// root namespace
        /// </summary>
        /// <value></value>
        public string RootNamespace { get; set; }
        /// <summary>
        /// runtime version
        /// </summary>
        /// <value></value>
        public string RuntimeFrameworkVersion { get; set; }
        /// <summary>
        /// target file name
        /// </summary>
        /// <value></value>
        public string TargetFileName { get; set; }
        /// <summary>
        /// target framework monitor
        /// </summary>
        /// <value></value>
        public string TargetFrameworkMoniker { get; set; }
        /// <summary>
        /// is nullable enabled
        /// </summary>
        /// <value></value>
        public string Nullable { get; set; }
        /// <summary>
        /// target framework
        /// </summary>
        /// <value></value>
        public string TargetFramework { get; set; }
        /// <summary>
        /// target platform identifier
        /// </summary>
        /// <value></value>
        public string TargetPlatformIdentifier { get; set; }

        /// <summary>
        /// method that retrieves project information
        /// </summary>
        /// <param name="file"></param>
        /// <param name="buildExtensionsDir"></param>
        /// <param name="framework"></param>
        /// <param name="configuration"></param>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public static Project FromFile(
            string file = null,
            string buildExtensionsDir = null,
            string framework = null,
            string configuration = null,
            string runtime = null)
        {
            file = ResolveProjects(file);
            if (buildExtensionsDir == null)
            {
                buildExtensionsDir = Path.Combine(Path.GetDirectoryName(file)!, "obj");
            }

            Directory.CreateDirectory(buildExtensionsDir);

            var efTargetsPath = Path.Combine(
                buildExtensionsDir,
                Path.GetFileName(file) + ".BBCoders.Commons.Utilities.targets");
            using (var input = typeof(Project).Assembly.GetManifestResourceStream(
                "BBCoders.Commons.Utilities.BBCoders.Commons.Utilities.targets")!)
            using (var output = File.OpenWrite(efTargetsPath))
            {
                // NB: Copy always in case it changes
                Reporter.WriteVerbose("Writing file - " + efTargetsPath);
                input.CopyTo(output);
            }

            IDictionary<string, string> metadata;
            var metadataFile = Path.GetTempFileName();
            try
            {
                var propertyArg = "/property:EFProjectMetadataFile=" + metadataFile;
                if (framework != null)
                {
                    propertyArg += ";TargetFramework=" + framework;
                }

                if (configuration != null)
                {
                    propertyArg += ";Configuration=" + configuration;
                }

                if (runtime != null)
                {
                    propertyArg += ";RuntimeIdentifier=" + runtime;
                }

                var args = new List<string>
                {
                    "msbuild",
                    "/target:GetEFProjectMetadata",
                    propertyArg,
                    "/verbosity:quiet",
                    "/nologo"
                };

                if (file != null)
                {
                    args.Add(file);
                }
                var exitCode = Exe.Run("dotnet", args);
                if (exitCode != 0)
                {
                    throw new Exception("Unable to retrieve project metadata. Ensure it's an SDK-style project. If you're using a custom BaseIntermediateOutputPath or MSBuildProjectExtensionsPath values, Use the --msbuildprojectextensionspath option.");
                }

                metadata = File.ReadLines(metadataFile).Select(l => l.Split(new[] { ':' }, 2))
                    .ToDictionary(s => s[0], s => s[1].TrimStart());
            }
            finally
            {
                File.Delete(metadataFile);
            }

            var platformTarget = metadata["PlatformTarget"];
            if (platformTarget.Length == 0)
            {
                platformTarget = metadata["Platform"];
            }

            return new Project(file, framework, configuration, runtime)
            {
                AssemblyName = metadata["AssemblyName"],
                Language = metadata["Language"],
                OutputPath = metadata["OutputPath"],
                PlatformTarget = platformTarget,
                ProjectAssetsFile = metadata["ProjectAssetsFile"],
                ProjectDir = metadata["ProjectDir"],
                RootNamespace = metadata["RootNamespace"],
                RuntimeFrameworkVersion = metadata["RuntimeFrameworkVersion"],
                TargetFileName = metadata["TargetFileName"],
                TargetFrameworkMoniker = metadata["TargetFrameworkMoniker"],
                Nullable = metadata["Nullable"],
                TargetFramework = metadata["TargetFramework"],
                TargetPlatformIdentifier = metadata["TargetPlatformIdentifier"]
            };
        }
        private static string ResolveProjects(string path = null)
        {
            if (path == null)
            {
                path = Directory.GetCurrentDirectory();
            }
            var projects = Directory.EnumerateFiles(path, "*.*proj", SearchOption.TopDirectoryOnly)
                .Where(f => !string.Equals(Path.GetExtension(f), ".xproj", StringComparison.OrdinalIgnoreCase))
                .Take(2).ToList();

            if (projects.Count > 1)
            {
                throw new Exception("Multiple projects files exists in the directory");
            }

            if (projects.Count == 0)
            {
                throw new Exception("No projects file exists in the directory");
            }
            return projects[0];
        } 

        /// <summary>
        /// Builds given project
        /// </summary>
        public void Build()
        {
            var args = new List<string> { "build" };

            if (_file != null)
            {
                args.Add(_file);
            }

            // TODO: Only build for the first framework when unspecified
            if (_framework != null)
            {
                args.Add("--framework");
                args.Add(_framework);
            }

            if (_configuration != null)
            {
                args.Add("--configuration");
                args.Add(_configuration);
            }

            if (_runtime != null)
            {
                args.Add("--runtime");
                args.Add(_runtime);
            }

            args.Add("/verbosity:quiet");
            args.Add("/nologo");

            var exitCode = Exe.Run("dotnet", args, interceptOutput: true);
            if (exitCode != 0)
            {
                throw new Exception("Build Failed...");
            }
        }
    }
}