using System.Reflection;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using BBCoders.Commons.Utilities;
using BBCoders.Commons.Tools.QueryGenerator;

namespace BBCoders.Commons.Tools
{
    public class OperationExecutor
    {
        private readonly string _projectDir;
        private readonly string _targetPath;
        private readonly string _startupTargetPath;
        private readonly string _rootNamespace;
        private readonly string[] _designArgs;
        private readonly IOperationReporter _reporter;
        private Assembly _assembly;

        public OperationExecutor(Project project)
        {
            var handler = new OperationReportHandler(
                (Action<string>)Reporter.WriteError,
                (Action<string>)Reporter.WriteWarning,
                (Action<string>)Reporter.WriteInformation,
                (Action<string>)Reporter.WriteVerbose);
            _reporter = new OperationReporter(handler);

            var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir, project.OutputPath));
            var targetPath = Path.Combine(targetDir, project.TargetFileName);
            _targetPath = targetPath;
            _projectDir = project.ProjectDir;
            _rootNamespace = project.RootNamespace;
        }

        private Assembly Assembly =>
         _assembly ??= Assembly.LoadFrom(_targetPath);

        public void ExecuteQueryOperations()
        {
            new DefaultQueryOperations(
                        _reporter,
                        Assembly,
                        Assembly,
                        _projectDir,
                        _rootNamespace,
                        _designArgs).Execute();
        }

    }
}