using Microsoft.Extensions.CommandLineUtils;
using System.Reflection;
using System;
using BBCoders.Commons.Utilities;
using BBCoders.Commons.Utilities.Commands;

namespace BBCoders.Commons.QueryGeneratorTool
{
    public class BaseCommand
    {
        private readonly CommandLineApplication _app;

        private CommandOption _noBuild;
        private CommandOption _isVerbose;
        private CommandOption _noColor;
        private CommandOption _prefixOutput;
        protected Project project;

        public BaseCommand(CommandLineApplication commandLineApplication)
        {
            _app = commandLineApplication;
        }

        public virtual void Configure()
        {
            _isVerbose = _app.Option("--verbose <VERBOSE>", "Log options", CommandOptionType.NoValue);
            _noBuild = _app.Option("--no-build", "Don't build the project. Intended to be used when the build is up-to-date.", CommandOptionType.NoValue);
            _noColor = _app.Option("--no-color", "color the output", CommandOptionType.NoValue);
            _prefixOutput = _app.Option("--prefix-output", "color options", CommandOptionType.NoValue);
        }

        public void Execute()
        {
            Reporter.IsVerbose = _isVerbose.HasValue();
            Reporter.NoColor = _noColor.HasValue();
            Reporter.PrefixOutput = _prefixOutput.HasValue();

            project = Project.FromFile();
            if (!_noBuild.HasValue())
            {
                Reporter.WriteInformation("Build Started...");
                project.Build();
                Reporter.WriteInformation("Build Success...");
            }
        }
    }
}