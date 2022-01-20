using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace BBCoders.Commons.QueryGenerator
{
    /// <summary>
    /// Base class for query operations
    /// </summary>
    public abstract class QueryContext
    {
        /// <summary>
        /// assembly of the operations
        /// </summary>
        protected readonly Assembly assembly;
        /// <summary>
        /// start up assembly
        /// </summary>
        protected readonly Assembly startupAssembly;
        /// <summary>
        /// project directory
        /// </summary>
        protected readonly string projectDir;
        /// <summary>
        /// root namespace
        /// </summary>
        protected readonly string rootNamespace;
        /// <summary>
        /// design args
        /// </summary>
        protected readonly string[] designArgs;

        /// <summary>
        /// constructor for query operations
        /// </summary>
        /// <param name="assembly">assembly</param>
        /// <param name="startupAssembly"></param>
        /// <param name="projectDir">projectDir</param>
        /// <param name="rootNamespace">rootNamespace</param>
        /// <param name="designArgs">designArgs</param>
        public QueryContext([NotNullAttribute] Assembly assembly,
                        [NotNullAttribute] Assembly startupAssembly,
                        [NotNullAttribute] string projectDir,
                        [NotNullAttribute] string rootNamespace,
                        [NotNullAttribute] string[] designArgs)
        {
            this.assembly = assembly;
            this.startupAssembly = startupAssembly;
            this.projectDir = projectDir;
            this.rootNamespace = rootNamespace;
            this.designArgs = designArgs;
        }

        /// <summary>
        /// Add CRUD operations for given entity
        /// </summary>
        /// <typeparam name="T">entity type</typeparam>
        public abstract void Add<T>();

        /// <summary>
        /// Add CRUD operations for given sql model
        /// </summary>
        /// <param name="queryModel"></param>
        public abstract void Add(QueryModel queryModel);

        /// <summary>
        /// Add Custom query operation
        /// </summary>
        /// <param name="name">name of the method</param>
        /// <param name="inputParameters">list of parameters</param>
        /// <param name="expression">expression to compile</param>
        /// <typeparam name="T">type of expression</typeparam>
        public abstract void Add<T>(string name, List<ParameterExpression> inputParameters, Expression<T> expression);
    }
}