using System.Text;

namespace CassandraORM.Tokens
{
    public class BlobConstant : Constant<byte[]>
    {
        private static readonly uint[] _lookup = CreateLookup();

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (Value != null)
            {
                queryBuilder.Append(ByteArrayToStringHex(Value));
            }
        }

        private static uint[] CreateLookup()
        {
            var result = new uint[256];

            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");

                result[i] = s[0] + ((uint)s[1] << 16);
            }

            return result;
        }

        private static string ByteArrayToStringHex(byte[] bytes)
        {
            char[] result = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = _lookup[bytes[i]];

                result[2 * i] = (char)val;

                result[2 * i + 1] = (char)(val >> 16);
            }

            return new string(result);
        }
    }
}
