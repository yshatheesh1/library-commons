using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBCoders.Commons.Utilities;

namespace BBCoders.Commons.Tools
{
    public class Project
    {
        private readonly string _file;
        private readonly string _framework;
        private readonly string _configuration;
        private readonly string _runtime;

        public Project(string file, string framework, string configuration, string runtime)
        {
            _file = file;
            _framework = framework;
            _configuration = configuration;
            _runtime = runtime;
            ProjectName = Path.GetFileName(file);
        }
        public string ProjectName { get; }
        public string AssemblyName { get; set; }
        public string Language { get; set; }
        public string OutputPath { get; set; }
        public string PlatformTarget { get; set; }
        public string ProjectAssetsFile { get; set; }
        public string ProjectDir { get; set; }
        public string RootNamespace { get; set; }
        public string RuntimeFrameworkVersion { get; set; }
        public string TargetFileName { get; set; }
        public string TargetFrameworkMoniker { get; set; }
        public string Nullable { get; set; }
        public string TargetFramework { get; set; }
        public string TargetPlatformIdentifier { get; set; }

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
                Path.GetFileName(file) + ".BBCoders.Commons.Tools.targets");
            using (var input = typeof(Project).Assembly.GetManifestResourceStream(
                "BBCoders.Commons.Tools.BBCoders.Commons.Tools.targets")!)
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