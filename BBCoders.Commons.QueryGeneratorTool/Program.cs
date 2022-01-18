using System;
using System.IO;
using System.Reflection;
using BBCoders.Commons.Utilities;
using BBCoders.Commons.Utilities.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace BBCoders.Commons.QueryGeneratorTool
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Reporter.IsVerbose = true;
            Reporter.NoColor = false;
            Reporter.PrefixOutput = false;
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Tool to generate source at runtime",
                FullName = "Generates package from query configuration"
            };

            app.VersionOption("--version", GetVersion);
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });
            app.Command("generate-query", generate =>
            {
                generate.Description = "generates source code for queries configuration";
                generate.HelpOption("-?|-h|--help");
                Reporter.IsVerbose = generate.Option("--verbose", "Log options", CommandOptionType.NoValue).HasValue();
                var _noBuild = generate.Option("--no-build", "Don't build the project. Intended to be used when the build is up-to-date.", CommandOptionType.NoValue);
                generate.OnExecute(() =>
                {
                    var project = Project.FromFile();
                    BuildProject(project, _noBuild);
                    var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir, project.OutputPath));
                    var targetPath = Path.Combine(targetDir, project.TargetFileName);
                    Reporter.WriteInformation("target path - " + targetPath);
                    var assembly = Assembly.LoadFrom(targetPath);
                    new DefaultQueryOperations(
                                assembly,
                                assembly,
                                project.ProjectDir,
                                project.RootNamespace,
                                new string[] { }
                                ).Execute();
                    return 0;
                });
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                if (ex is CommandParsingException)
                {
                    Reporter.WriteVerbose(ex.ToString());
                }
                else
                {
                    Reporter.WriteInformation(ex.ToString());
                }

                Reporter.WriteError(ex.Message);

                return 1;
            }
        }

        private static void BuildProject(Project project, CommandOption noBuild)
        {
            if (!noBuild.HasValue())
            {
                Reporter.WriteInformation("Build Started...");
                project.Build();
                Reporter.WriteInformation("Build Success...");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
