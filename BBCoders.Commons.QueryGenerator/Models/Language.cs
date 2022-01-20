using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BBCoders.Commons.QueryGenerator
{
    /// <summary>
    /// Contains type info for given language
    /// </summary>
    public class Language
    {
        /// <summary>
        /// language identifier
        /// </summary>
        /// <value></value>
        public string Identifier { get; }
        /// <summary>
        /// file extensions used for language
        /// </summary>
        /// <value></value>
        public string FileExtension { get; }
        /// <summary>
        /// CSharp type to specific language type
        /// </summary>
        /// <value></value>
        public Dictionary<Type, string> Type { get; }
        private readonly static Dictionary<Type, InternalType> _typeHandlers = new Dictionary<Type, InternalType>()
        {
            [typeof(System.Byte)] = new InternalType() { DbType = DbType.Byte, CSharp = "Byte", NodeJs = "Int8Array", Go = "byte" },
            [typeof(System.SByte)] = new InternalType() { DbType = DbType.SByte, CSharp = "SByte", NodeJs = "Int8Array", Go = "byte" },
            [typeof(System.Int16)] = new InternalType() { DbType = DbType.Int16, CSharp = "Int16", NodeJs = "Int16Array", Go = "int16" },
            [typeof(System.UInt16)] = new InternalType() { DbType = DbType.UInt16, CSharp = "UInt16", NodeJs = "Uint16Array", Go = "uint16" },
            [typeof(System.Int32)] = new InternalType() { DbType = DbType.Int32, CSharp = "Int32", NodeJs = "Int32Array", Go = "int32" },
            [typeof(System.UInt32)] = new InternalType() { DbType = DbType.UInt32, CSharp = "UInt32", NodeJs = "UInt32Array", Go = "uint32" },
            [typeof(System.Int64)] = new InternalType() { DbType = DbType.Int64, CSharp = "Int64", NodeJs = "Int64Array", Go = "int64" },
            [typeof(System.UInt64)] = new InternalType() { DbType = DbType.UInt64, CSharp = "UInt64", NodeJs = "UInt64Array", Go = "uint64" },
            [typeof(System.Single)] = new InternalType() { DbType = DbType.Single, CSharp = "Single", NodeJs = "Float64Array", Go = "float32" },
            [typeof(System.Double)] = new InternalType() { DbType = DbType.Double, CSharp = "Double", NodeJs = "Float64Array", Go = "float64" },
            [typeof(System.Decimal)] = new InternalType() { DbType = DbType.Decimal, CSharp = "Decimal", NodeJs = "Float64Array", Go = "float64" },
            [typeof(System.Boolean)] = new InternalType() { DbType = DbType.Boolean, CSharp = "Boolean", NodeJs = "Boolean", Go = "bool" },
            [typeof(System.String)] = new InternalType() { DbType = DbType.String, CSharp = "String", NodeJs = "String", Go = "string" },
            [typeof(System.Char)] = new InternalType() { DbType = DbType.StringFixedLength, CSharp = "Char", NodeJs = "String", Go = "string" },
            [typeof(System.Guid)] = new InternalType() { DbType = DbType.Guid, CSharp = "Guid", NodeJs = "String", Go = "[]byte" },
            [typeof(System.DateTime)] = new InternalType() { DbType = null, CSharp = "DateTime", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.DateTimeOffset)] = new InternalType() { DbType = DbType.DateTimeOffset, CSharp = "DateTimeOffset", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.TimeSpan)] = new InternalType() { DbType = null, CSharp = "TimeSpan", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.Byte[])] = new InternalType() { DbType = DbType.Binary, CSharp = "Byte[]", NodeJs = "Int8Array", Go = "[]byte" },
            [typeof(System.Byte?)] = new InternalType() { DbType = DbType.Byte, CSharp = "Byte?", NodeJs = "Int8Array", Go = "byte" },
            [typeof(System.SByte?)] = new InternalType() { DbType = DbType.SByte, CSharp = "SByte?", NodeJs = "Int8Array", Go = "byte" },
            [typeof(System.Int16?)] = new InternalType() { DbType = DbType.Int16, CSharp = "Int16?", NodeJs = "Int16Array", Go = "int16" },
            [typeof(System.UInt16?)] = new InternalType() { DbType = DbType.UInt16, CSharp = "UInt16", NodeJs = "Uint16Array", Go = "uint16" },
            [typeof(System.Int32?)] = new InternalType() { DbType = DbType.Int32, CSharp = "Int32", NodeJs = "Int32Array", Go = "int32" },
            [typeof(System.UInt32?)] = new InternalType() { DbType = DbType.UInt32, CSharp = "UInt32", NodeJs = "UInt32Array", Go = "int32" },
            [typeof(System.Int64?)] = new InternalType() { DbType = DbType.Int64, CSharp = "Int64", NodeJs = "Int64Array", Go = "int64" },
            [typeof(System.UInt64?)] = new InternalType() { DbType = DbType.UInt64, CSharp = "UInt64", NodeJs = "UInt64Array", Go = "uint64" },
            [typeof(System.Single?)] = new InternalType() { DbType = DbType.Single, CSharp = "Single", NodeJs = "Float64Array", Go = "float32" },
            [typeof(System.Double?)] = new InternalType() { DbType = DbType.Double, CSharp = "Double", NodeJs = "Float64Array", Go = "float64" },
            [typeof(System.Decimal?)] = new InternalType() { DbType = DbType.Decimal, CSharp = "Decimal", NodeJs = "Float64Array", Go = "float64" },
            [typeof(System.Boolean?)] = new InternalType() { DbType = DbType.Boolean, CSharp = "Boolean", NodeJs = "Boolean", Go = "bool" },
            [typeof(System.Char?)] = new InternalType() { DbType = DbType.StringFixedLength, CSharp = "Char", NodeJs = "String", Go = "string" },
            [typeof(System.Guid?)] = new InternalType() { DbType = DbType.Guid, CSharp = "Guid", NodeJs = "String", Go = "[]byte]" },
            [typeof(System.DateTime?)] = new InternalType() { DbType = null, CSharp = "DateTime", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.DateTimeOffset?)] = new InternalType() { DbType = DbType.DateTimeOffset, CSharp = "DateTimeOffset", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.TimeSpan?)] = new InternalType() { DbType = null, CSharp = "TimeSpan", NodeJs = "Date", Go = "time.Time" },
            [typeof(System.Object)] = new InternalType() { DbType = null, CSharp = "Object", NodeJs = "any", Go = "interface{}" },
        };

        /// <summary>
        /// constructor for language
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="fileExtension"></param>
        /// <param name="type"></param>
        public Language(string identifier, string fileExtension, Dictionary<Type, string> type)
        {
            Identifier = identifier;
            FileExtension = fileExtension;
            Type = type;
        }

        /// <summary>
        /// CSharp language type
        /// </summary>
        /// <returns></returns>
        public readonly static Language CSharp = new Language("CSharp", "cs", _typeHandlers.ToDictionary(x => x.Key, y => y.Value.CSharp));
        /// <summary>
        /// javascript language type
        /// </summary>
        /// <returns></returns>
        public readonly static Language NodeJS = new Language("NodeJS", "ts", _typeHandlers.ToDictionary(x => x.Key, y => y.Value.NodeJs));
        /// <summary>
        /// Go language type
        /// </summary>
        /// <returns></returns>
        public readonly static Language Go = new Language("Go", "go", _typeHandlers.ToDictionary(x => x.Key, y => y.Value.Go));

        private class InternalType
        {
            public DbType? DbType { get; set; }
            public string CSharp { get; set; }
            public string NodeJs { get; set; }
            public string Go { get; set; }
        }
    }
}