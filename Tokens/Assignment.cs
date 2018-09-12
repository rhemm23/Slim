using System.Text;

namespace CassandraORM.Tokens
{
    public class Assignment : IToken
    {
        public AssignmentTypes AssignmentType { get; }
        public CounterModifierTypes CounterModifierType { get; }
        public CollectionModifierTypes CollectionModifierType { get; }
        public string CounterColumn { get; }
        public string CollectionColumn { get; }
        public Term CounterModifier { get; }
        public SimpleSelection Selection { get; }
        public Term SelectionAssignment { get; }
        public CollectionLiteral CollectionLiteral { get; }

        public enum AssignmentTypes
        {
            SimpleAssignment,
            CounterModifier,
            CollectionModifier
        }

        public enum CounterModifierTypes
        {
            Addition,
            Subtraction
        }

        public enum CollectionModifierTypes
        {
            Add,
            Remove,
            Prepend,
            Insert
        }

        public Assignment(SimpleSelection selection, Term selectionAssignment)
        {
            Selection = selection;
            SelectionAssignment = selectionAssignment;
            AssignmentType = AssignmentTypes.SimpleAssignment;
        }

        public Assignment(string counterColumn, CounterModifierTypes counterModifierType, Term modifier)
        {
            CounterColumn = counterColumn;
            CounterModifier = modifier;
            CounterModifierType = counterModifierType;
            AssignmentType = AssignmentTypes.CounterModifier;
        }

        public Assignment(string collectionColumn, CollectionModifierTypes collectionModifierType, CollectionLiteral collectionLiteral)
        {
            CollectionColumn = collectionColumn;
            CollectionModifierType = collectionModifierType;
            CollectionLiteral = collectionLiteral;
            AssignmentType = AssignmentTypes.CollectionModifier;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (AssignmentType)
            {
                case AssignmentTypes.CounterModifier:
                    string modifierSymbol = CounterModifierType == CounterModifierTypes.Addition ? "+" : "-";
                    queryBuilder.Append($"{CounterColumn} = {CounterColumn} {modifierSymbol} ");
                    CounterModifier.WriteCql(queryBuilder);
                    break;

                case AssignmentTypes.SimpleAssignment:
                    Selection.WriteCql(queryBuilder);
                    queryBuilder.Append(" = ");
                    SelectionAssignment.WriteCql(queryBuilder);
                    break;

                case AssignmentTypes.CollectionModifier:
                    queryBuilder.Append($"{CollectionColumn} = ");

                    switch (CollectionModifierType)
                    {
                        case CollectionModifierTypes.Add:
                            queryBuilder.Append($"{CollectionColumn} + ");
                            CollectionLiteral.WriteCql(queryBuilder);
                            break;

                        case CollectionModifierTypes.Insert:
                            CollectionLiteral.WriteCql(queryBuilder);
                            break;

                        case CollectionModifierTypes.Prepend:
                            CollectionLiteral.WriteCql(queryBuilder);
                            queryBuilder.Append($" + {CollectionLiteral}");
                            break;

                        case CollectionModifierTypes.Remove:
                            queryBuilder.Append($"{CollectionColumn} - ");
                            CollectionLiteral.WriteCql(queryBuilder);
                            break;
                    }
                    break;
            }
        }
    }
}
