using System;
using System.Reflection;
using BBCoders.Commons.Utilities;
using Microsoft.Extensions.CommandLineUtils;

namespace BBCoders.Commons.Tools
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet bbcoders generate-query",
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
               generate.Description = "generates code for queries if ef core project is found";
               generate.HelpOption("-?|-h|--help");
               generate.OnExecute(() =>
               {
                   var _noBuild = generate.Option("--no-build", "Don't build the project. Intended to be used when the build is up-to-date.", CommandOptionType.SingleValue);
                   var project = Project.FromFile(System.IO.Directory.GetCurrentDirectory());
                   if (!_noBuild.HasValue())
                   {
                       Reporter.WriteInformation("Build Started...");
                       project.Build();
                       Reporter.WriteInformation("Build Success...");
                   }
                   new OperationExecutor(project).ExecuteQueryOperations();
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

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
