using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BBCoders.Commons.QueryGeneratorTool.Helpers
{
    public class SelectExpressionHelper
    {
        public static string GetSelectQuery(SelectExpression selectExpression)
        {
            ExpressionPrinter expressionPrinter = new ExpressionPrinter();
            var fieldMethod = expressionPrinter.GetType().GetField("_stringBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = (IndentedStringBuilder)fieldMethod.GetValue(expressionPrinter);
            fieldValue.Clear();
            expressionPrinter.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                expressionPrinter.Append("DISTINCT ");
            }

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                expressionPrinter.Append("TOP(");
                expressionPrinter.Visit(selectExpression.Limit);
                expressionPrinter.Append(") ");
            }

            if (selectExpression.Projection.Any())
            {
                expressionPrinter.VisitCollection(selectExpression.Projection);
            }
            else
            {
                expressionPrinter.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                expressionPrinter.AppendLine().Append("FROM ");

                expressionPrinter.VisitCollection(selectExpression.Tables, p => p.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                expressionPrinter.AppendLine().Append("WHERE ");
                expressionPrinter.Visit(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Any())
            {
                expressionPrinter.AppendLine().Append("GROUP BY ");
                expressionPrinter.VisitCollection(selectExpression.GroupBy);
            }

            if (selectExpression.Having != null)
            {
                expressionPrinter.AppendLine().Append("HAVING ");
                expressionPrinter.Visit(selectExpression.Having);
            }

            if (selectExpression.Orderings.Any())
            {
                expressionPrinter.AppendLine().Append("ORDER BY ");
                expressionPrinter.VisitCollection(selectExpression.Orderings);
            }
            else if (selectExpression.Offset != null)
            {
                expressionPrinter.AppendLine().Append("ORDER BY (SELECT 1)");
            }

            if (selectExpression.Offset != null)
            {
                expressionPrinter.AppendLine().Append("OFFSET ");
                expressionPrinter.Visit(selectExpression.Offset);
                expressionPrinter.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    expressionPrinter.Append(" FETCH NEXT ");
                    expressionPrinter.Visit(selectExpression.Limit);
                    expressionPrinter.Append(" ROWS ONLY");
                }
            }

            if (selectExpression.Alias != null)
            {
                expressionPrinter.AppendLine().Append(") AS " + selectExpression.Alias);
            }
            return fieldValue.ToString();
        }
    }
}