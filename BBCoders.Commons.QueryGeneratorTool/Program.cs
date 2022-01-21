using System;
using System.IO;
using System.Reflection;
using BBCoders.Commons.Utilities;
using Microsoft.Extensions.CommandLineUtils;

namespace BBCoders.Commons.QueryGeneratorTool
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Tool to generate queries based on query configuration",
                FullName = "Generates package from query configuration"
            };

            app.VersionOption("--version", GetVersion);
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Command("generate-query", (generate) => new QueryCommand(generate).Configure());

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
