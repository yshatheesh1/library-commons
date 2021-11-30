using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BBCoders.Commons.Tools.QueryGenerator.Helpers
{
    public class SqlMapperHelper
    {
        private static Dictionary<Type, DbType> typeMap;
        private static Dictionary<DbType, Type> dbTypeMap;

        static SqlMapperHelper()
        {
            typeMap = new Dictionary<Type, DbType>()
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = DbType.Time,
                [typeof(byte[])] = DbType.Binary,
                [typeof(object)] = DbType.Object
            };
            dbTypeMap = typeMap.ToDictionary(x => x.Value, y => y.Key);
        }

        public static Type getClrType(DbType dbtype)
        {
            if (!dbTypeMap.ContainsKey(dbtype))
            {
                throw new Exception("Dbtype not found for mapping " + dbtype);
            }
            return dbTypeMap[dbtype];
        }

        public static DbType getDbype(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) type = nullableType;
            if (!typeMap.ContainsKey(type))
            {
                throw new Exception("type not found for mapping " + type);
            }
            return typeMap[type];
        }
    }
}