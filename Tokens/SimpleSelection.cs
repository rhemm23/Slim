using System.Text;

namespace CassandraORM.Tokens
{
    public class SimpleSelection : IToken
    {
        public SimpleSelectionTypes SelectionType { get; }
        public string ColumnName { get; }
        public string FieldName { get; }
        public Term IndexTerm { get; }

        public enum SimpleSelectionTypes
        {
            Column,
            Index,
            Field
        }

        public SimpleSelection(string columnName)
        {
            ColumnName = columnName;
            SelectionType = SimpleSelectionTypes.Column;
        }

        public SimpleSelection(string columnName, Term indexTerm)
        {
            ColumnName = columnName;
            IndexTerm = indexTerm;
            SelectionType = SimpleSelectionTypes.Index;
        }

        public SimpleSelection(string columnName, string fieldName)
        {
            ColumnName = columnName;
            FieldName = fieldName;
            SelectionType = SimpleSelectionTypes.Field;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (SelectionType)
            {
                case SimpleSelectionTypes.Column:
                    queryBuilder.Append(ColumnName);
                    break;

                case SimpleSelectionTypes.Field:
                    queryBuilder.Append($"{ColumnName}.{FieldName}");
                    break;

                case SimpleSelectionTypes.Index:
                    queryBuilder.Append($"{ColumnName}[");
                    IndexTerm.WriteCql(queryBuilder);
                    queryBuilder.Append("]");
                    break;
            }
        }
    }
}
