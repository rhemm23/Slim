using System.Text;

namespace CassandraORM.Tokens
{
    public class UpdateParameter : IToken
    {
        public UpdateParameterTypes UpdateParameterType { get; }
        public IRestrictedValue ParameterValue { get; }

        public enum UpdateParameterTypes
        {
            Timestamp,
            TTL
        }

        public UpdateParameter(UpdateParameterTypes updateParameterType, IRestrictedValue parameterValue)
        {
            UpdateParameterType = updateParameterType;
            ParameterValue = parameterValue;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (UpdateParameterType)
            {
                case UpdateParameterTypes.Timestamp:
                    queryBuilder.Append("TIMESTAMP ");
                    break;

                case UpdateParameterTypes.TTL:
                    queryBuilder.Append("TTL ");
                    break;
            }

            ParameterValue.WriteCql(queryBuilder);
        }
    }
}
