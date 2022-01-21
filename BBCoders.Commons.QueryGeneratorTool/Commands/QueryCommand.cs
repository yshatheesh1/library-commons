using Microsoft.Extensions.CommandLineUtils;
using System.Reflection;
using System.IO;
using BBCoders.Commons.Utilities;
using BBCoders.Commons.Utilities.Commands;

namespace BBCoders.Commons.QueryGeneratorTool
{
    public class QueryCommand : BaseCommand
    {
        private readonly CommandLineApplication _app;

        public QueryCommand(CommandLineApplication commandLineApplication) : base(commandLineApplication)
        {
            _app = commandLineApplication;
        }

        public override void Configure()
        {
            base.Configure();
            _app.Description = "generates source code for query configuration";
            _app.HelpOption("-?|-h|--help");
            _app.OnExecute(OnExecute);
        }

        private int OnExecute()
        {
            base.Execute();
            var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir, project.OutputPath));
            var targetPath = Path.Combine(targetDir, project.TargetFileName);
            Reporter.WriteVerbose("target path - " + targetPath);
            var assembly = Assembly.LoadFrom(targetPath);
            new DefaultQueryOperations(
                        assembly,
                        assembly,
                        project.ProjectDir,
                        project.RootNamespace
                        ).Execute();
            return 0;
        }
    }
}