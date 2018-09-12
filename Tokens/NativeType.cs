using System.Text;
using System;   

namespace CassandraORM.Tokens
{
    public class NativeType : CqlType
    {
        public override Types Type => Types.Native;
        public NativeTypes NativeTypeValue { get; set; }

        public enum NativeTypes
        {
            Ascii,
            Bigint,
            Blob,
            Boolean,
            Counter,
            Date,
            Decimal,
            Double,
            Duration,
            Float,
            Inet,
            Int,
            Smallint,
            Text,
            Time,
            Timestamp,
            Timeuuid,
            Tinyint,
            Uuid,
            Varchar,
            Varint
        }

        public NativeType() { }

        public NativeType(NativeTypes nativeType)
        {
            NativeTypeValue = nativeType;
        }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append(Enum.GetName(typeof(NativeTypes), NativeTypeValue).ToUpper());
        }
    }
}
