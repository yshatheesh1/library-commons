using System;

namespace BBCoders.Commons.SourceGenerator
{
    /// <summary>
    /// Extend the class to generate design time source
    /// </summary>
    public interface ISourceGenerator
    {
        /// <summary>
        /// Source Context
        /// </summary>
        /// <param name="context">context</param>
        void Execute(SourceContext context);
    }
}
