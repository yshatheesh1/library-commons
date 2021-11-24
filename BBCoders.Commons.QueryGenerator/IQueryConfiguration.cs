using Microsoft.EntityFrameworkCore;

namespace BBCoders.Commons.QueryGenerator
{
    public interface IQueryConfiguration<T> where T : DbContext
    {
        void CreateQuery(T context, QueryOperations queryOperations);
    }
}