using Microsoft.EntityFrameworkCore;

namespace BBCoders.Commons.QueryConfiguration
{
    /// <summary>
    /// Extend the class to provide compile time queries
    /// </summary>
    /// <typeparam name="T">type of db context</typeparam>
    public interface IQueryConfiguration<T> where T : DbContext
    {
        /// <summary>
        /// configure query configuration options
        /// </summary>
        /// <returns>query options</returns>
        QueryOptions GetQueryOptions();

        /// <summary>
        /// create design time query
        /// </summary>
        /// <param name="context">db context</param> 
        /// <param name="queryOperations">query operations</param>
        void CreateQuery(T context, QueryOperations queryOperations);
    }
}