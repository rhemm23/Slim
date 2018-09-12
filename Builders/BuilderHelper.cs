using CassandraORM.Cache;
using CassandraORM.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CassandraORM.Builders
{
    public class BuilderHelper<TQuery, TItem> where TQuery : IQuery, new()
    {
        public bool UseBindings { get; }
        private QueryBuilder<TQuery, TItem> _queryBuilder;

        public BuilderHelper(QueryBuilder<TQuery, TItem> builder, bool useBindings)
        {
            _queryBuilder = builder;
            UseBindings = useBindings;
        }

        public SimpleSelection ParseSimpleSelection<T>(Expression<Func<T, object>> select)
        {
            Expression current = select?.Body;

            if (current == null)
            {
                return null;
            }

            current = RemoveConvert(current);

            return ParseSimpleSelection<T>(current);
        }

        public SimpleSelection ParseSimpleSelection<T>(Expression select)
        {
            switch (select)
            {
                case IndexExpression index:
                    if (index.Object is MemberExpression indexMember)
                    {
                        Term indexTerm = ParseTerm(index.Arguments[0]);

                        return new SimpleSelection(TableCache<T>.CachedTable.Aliases[indexMember.Member.Name], indexTerm);
                    }
                    throw new InvalidOperationException("Only table columns can be indexed");

                case MemberExpression member:
                    string primaryName = TableCache<T>.CachedTable.Aliases[member.Member.Name];

                    if (member.Expression.NodeType == ExpressionType.Parameter)
                    {
                        return new SimpleSelection(primaryName);
                    }
                    else if (member.Expression is MemberExpression secondaryMember)
                    {
                        return new SimpleSelection(TableCache<T>.CachedTable.Aliases[secondaryMember.Member.Name], primaryName);
                    }
                    throw new InvalidOperationException("Member access root expression is not supported");

                default:
                    throw new NotSupportedException("Invalid expression for assignment clause");
            }
        }

        public KeyValuePair<Select, string> ParseAliasedSelect<T, TResult>(Expression<Func<T, TResult>> select)
        {
            string alias = null;
            Expression current = select?.Body;

            if (current == null)
            {
                return default;
            }

            current = RemoveConvert(current);

            if (current is MethodCallExpression methodCall &&
                methodCall.Method.Name == "As")
            {
                alias = (methodCall.Arguments[1] as ConstantExpression).Value as string;
                current = methodCall.Arguments[0];
            }

            return new KeyValuePair<Select, string>(ParseSelect<T>(current), alias);
        }

        public Select ParseSelect<T>(Expression select)
        {
            if (select == null)
            {
                return default;
            }

            // Remove potential convert expression if the member access
            // expression selects a primitive type
            select = RemoveConvert(select);

            switch (select)
            {
                case MemberExpression memberExpression:
                    return new Select(TableCache<T>.CachedTable.Aliases[memberExpression.Member.Name]);

                case MethodCallExpression methodExpression:
                    if (methodExpression.Method.Name == "Cast")
                    {
                        Select innerSelect = ParseSelect<T>(methodExpression.Arguments[0]);

                        Expression convert = Expression.Convert(methodExpression.Arguments[1], typeof(object));
                        Expression<Func<object>> convertLambda = Expression.Lambda<Func<object>>(convert);

                        return new Select(innerSelect, convertLambda.Compile()() as CqlType);
                    }
                    else
                    {
                        Select[] functionArguments = new Select[methodExpression.Arguments.Count];

                        for (int i = 0; i < functionArguments.Length; i++)
                        {
                            functionArguments[i] = ParseSelect<T>(methodExpression.Arguments[i]);
                        }

                        return new Select(methodExpression.Method.Name, functionArguments);
                    }
            }

            return new Select(ParseTerm(select));
        }

        public Term ParseTerm(Expression expression)
        {
            if (expression == null)
            {
                return default;
            }

            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    ArithmeticOperation arithmeticOperation = new ArithmeticOperation();

                    arithmeticOperation.FirstTerm = ParseTerm(binaryExpression.Left);
                    arithmeticOperation.SecondTerm = ParseTerm(binaryExpression.Right);

                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                            arithmeticOperation.OperationType = ArithmeticOperation.ArithmeticOperationTypes.Addition;
                            break;

                        case ExpressionType.Divide:
                            arithmeticOperation.OperationType = ArithmeticOperation.ArithmeticOperationTypes.Division;
                            break;

                        case ExpressionType.Modulo:
                            arithmeticOperation.OperationType = ArithmeticOperation.ArithmeticOperationTypes.Modulus;
                            break;

                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                            arithmeticOperation.OperationType = ArithmeticOperation.ArithmeticOperationTypes.Multiplication;
                            break;

                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                            arithmeticOperation.OperationType = ArithmeticOperation.ArithmeticOperationTypes.Subtraction;
                            break;
                    }

                    return arithmeticOperation;

                case UnaryExpression unaryExpression:
                    if (unaryExpression.NodeType == ExpressionType.Negate ||
                        unaryExpression.NodeType == ExpressionType.NegateChecked)
                    {
                        return new ArithmeticOperation()
                        {
                            OperationType = ArithmeticOperation.ArithmeticOperationTypes.Negative,
                            FirstTerm = ParseTerm(unaryExpression.Operand)
                        };
                    }

                    throw new NotSupportedException("Unary expression is not supported for terms");

                case MethodCallExpression methodCall:
                    Term[] arguments = new Term[methodCall.Arguments.Count];

                    for (int i = 0; i < arguments.Length; i++)
                    {
                        arguments[i] = ParseTerm(methodCall.Arguments[i]);
                    }

                    return new FunctionCall()
                    {
                        Identifier = methodCall.Method.Name,
                        Arguments = arguments
                    };

                case ListInitExpression listInit:
                    if (typeof(IDictionary).IsAssignableFrom(listInit.Type))
                    {
                        Dictionary<Term, Term> termDictionary = new Dictionary<Term, Term>();

                        foreach (ElementInit element in listInit.Initializers)
                        {
                            termDictionary.Add(ParseTerm(element.Arguments[0]), ParseTerm(element.Arguments[1]));
                        }

                        return new CollectionLiteral(termDictionary);
                    }
                    else if ((listInit.Type.GenericTypeArguments.Length > 0) &&
                            typeof(ISet<>).MakeGenericType(listInit.Type.GenericTypeArguments[0])
                            .IsAssignableFrom(listInit.Type))
                    {
                        HashSet<Term> terms = new HashSet<Term>();

                        foreach (ElementInit element in listInit.Initializers)
                        {
                            terms.Add(ParseTerm(element.Arguments[0]));
                        }

                        return new CollectionLiteral(terms);
                    }
                    else if (typeof(IList).IsAssignableFrom(listInit.Type))
                    {
                        List<Term> terms = new List<Term>();

                        foreach (ElementInit element in listInit.Initializers)
                        {
                            terms.Add(ParseTerm(element.Arguments[0]));
                        }

                        return new CollectionLiteral(terms);
                    }

                    return null;

                case MemberInitExpression memberInit:
                    if (typeof(ITuple).IsAssignableFrom(memberInit.Type))
                    {
                        List<Term> terms = new List<Term>();

                        foreach (Expression argument in memberInit.NewExpression.Arguments)
                        {
                            terms.Add(ParseTerm(argument));
                        }

                        return new TupleLiteral() { Terms = terms };
                    }
                    else
                    {
                        Dictionary<string, Term> properties = new Dictionary<string, Term>();

                        foreach (MemberBinding binding in memberInit.Bindings)
                        {
                            if (binding is MemberAssignment assignment)
                            {
                                properties.Add(assignment.Member.Name, ParseTerm(assignment.Expression));
                            }
                        }

                        return new UserDefinedTypeLiteral() { Properties = properties };
                    }

                case MemberExpression member:
                    Stack<MemberInfo> memberInfo = new Stack<MemberInfo>();
                    Expression cur = member;

                    while (cur is MemberExpression memberExpression)
                    {
                        memberInfo.Push(memberExpression.Member);

                        cur = memberExpression.Expression;
                    }
                    if (cur is ConstantExpression constant)
                    {
                        object value = constant.Value;

                        while (memberInfo.Count > 0)
                        {
                            MemberInfo top = memberInfo.Pop();

                            if (top is PropertyInfo property)
                            {
                                value = property.GetValue(value);
                            }
                            else if (top is FieldInfo field)
                            {
                                value = field.GetValue(value);
                            }
                        }

                        return AddValue(value);
                    }
                    throw new NotSupportedException("Terms must represent a constant value");


                case ConstantExpression constantExpression:
                    return AddValue(constantExpression.Value);
            }

            throw new NotImplementedException();
        }

        private Term AddValue(object value)
        {
            if (UseBindings)
            {
                string alias = _queryBuilder.GetDistinctAlias();
                _queryBuilder.Parameters.Add(alias, value);
                return new BindMarker() { Identifier = alias };
            }
            else
            {
                switch (value)
                {
                    case null:
                        return new NullConstant();

                    case int intValue:
                        return new IntegerConstant() { Value = intValue };

                    case float floatValue:
                        return new FloatConstant() { Value = floatValue };

                    case string stringValue:
                        return new StringConstant() { Value = stringValue };

                    case byte[] byteValue:
                        return new BlobConstant() { Value = byteValue };

                    case bool boolValue:
                        return new BoolConstant() { Value = boolValue };

                    case Guid guidValue:
                        return new GuidConstant() { Value = guidValue };

                    default:
                        throw new NotSupportedException("Constant value is not supported");
                }
            }
        }

        public WhereClause ParseWhere<T>(Expression<Func<T, bool>> expression)
        {
            Expression current = expression?.Body;

            if (current == null)
            {
                return default;
            }

            return new WhereClause()
            {
                Relations = ParseRelation<T>(current)
            };
        }

        public List<Condition> ParseConditions<T>(Expression<Func<T, bool>> predicate)
        {
            Expression current = predicate?.Body;

            if (current == null)
            {
                return null;
            }

            current = RemoveConvert(current);

            return ParseConditions<T>(current);
        }

        public List<Condition> ParseConditions<T>(Expression expression)
        {
            List<Condition> conditions = new List<Condition>();

            switch (expression)
            {
                case MethodCallExpression methodCall:
                    SimpleSelection selection = ParseSimpleSelection<T>(methodCall.Arguments[0]);

                    switch (methodCall.Method.Name)
                    {
                        case "Contains":
                            Term containsTerm = ParseTerm(methodCall.Arguments[1]);
                            conditions.Add(new Condition(selection, new Operator(Operator.OperatorTypes.Contains), containsTerm));
                            break;

                        case "ContainsKey":
                            Term containsKeyTerm = ParseTerm(methodCall.Arguments[1]);
                            conditions.Add(new Condition(selection, new Operator(Operator.OperatorTypes.ContainsKey), containsKeyTerm));
                            break;

                        case "In":
                            TupleLiteral collection = new TupleLiteral()
                            {
                                Terms = methodCall.Arguments.Skip(1).Select(s => ParseTerm(s))
                            };
                            conditions.Add(new Condition(selection, new Operator(Operator.OperatorTypes.In), collection));
                            break;

                        default:
                            throw new NotSupportedException($"Cannot parse method call {methodCall.Method.Name}, it is not a supported relation");
                    }
                    break;

                case BinaryExpression binaryExpression:
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            conditions.AddRange(ParseConditions<T>(binaryExpression.Left));
                            conditions.AddRange(ParseConditions<T>(binaryExpression.Right));
                            break;

                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            throw new NotSupportedException("Cassandra does not support OR");

                        default:
                            SimpleSelection select = ParseSimpleSelection<T>(binaryExpression.Left);
                            Operator operatorObj;

                            switch (binaryExpression.NodeType)
                            {
                                case ExpressionType.Equal:
                                    operatorObj = new Operator(Operator.OperatorTypes.Equal);
                                    break;

                                case ExpressionType.LessThan:
                                    operatorObj = new Operator(Operator.OperatorTypes.Less);
                                    break;

                                case ExpressionType.LessThanOrEqual:
                                    operatorObj = new Operator(Operator.OperatorTypes.LessEqual);
                                    break;

                                case ExpressionType.GreaterThan:
                                    operatorObj = new Operator(Operator.OperatorTypes.Greater);
                                    break;

                                case ExpressionType.GreaterThanOrEqual:
                                    operatorObj = new Operator(Operator.OperatorTypes.GreaterEqual);
                                    break;

                                case ExpressionType.NotEqual:
                                    operatorObj = new Operator(Operator.OperatorTypes.NotEqual);
                                    break;

                                default:
                                    throw new NotSupportedException("Cassandra does not support specified binary operator");
                            }

                            conditions.Add(new Condition(select, operatorObj, ParseTerm(binaryExpression.Right)));
                            break;
                    }
                    break;

                default:
                    throw new NotSupportedException($"Cannot parse relation form unsupported expression of type {expression.Type}");
            }

            return conditions;
        }

        public List<Relation> ParseRelation<T>(Expression expression)
        {
            List<Relation> relations = new List<Relation>();

            switch (expression)
            {
                case MethodCallExpression methodCall:
                    if (methodCall.Arguments[0] is MemberExpression member)
                    {
                        string memberName = TableCache<T>.CachedTable.Aliases[member.Member.Name];

                        switch (methodCall.Method.Name)
                        {
                            case "Contains":
                                Term containsTerm = ParseTerm(methodCall.Arguments[1]);
                                relations.Add(new Relation(memberName, new Operator(Operator.OperatorTypes.Contains), containsTerm));
                                break;

                            case "ContainsKey":
                                Term containsKeyTerm = ParseTerm(methodCall.Arguments[1]);
                                relations.Add(new Relation(memberName, new Operator(Operator.OperatorTypes.ContainsKey), containsKeyTerm));
                                break;

                            case "In":
                                TupleLiteral collection = new TupleLiteral()
                                {
                                    Terms = methodCall.Arguments.Skip(1).Select(s => ParseTerm(s))
                                };
                                relations.Add(new Relation(memberName, new Operator(Operator.OperatorTypes.In), collection));
                                break;

                            default:
                                throw new NotSupportedException($"Cannot parse method call {methodCall.Method.Name}, it is not a supported relation");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Relation marker methods must be preceded by a member expression");
                    }
                    break;

                case BinaryExpression binaryExpression:
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            relations.AddRange(ParseRelation<T>(binaryExpression.Left));
                            relations.AddRange(ParseRelation<T>(binaryExpression.Right));
                            break;

                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            throw new NotSupportedException("Cassandra does not support OR");

                        default:
                            if (binaryExpression.Left is MemberExpression memberLeft)
                            {
                                Operator operatorObj;

                                switch (binaryExpression.NodeType)
                                {
                                    case ExpressionType.Equal:
                                        operatorObj = new Operator(Operator.OperatorTypes.Equal);
                                        break;

                                    case ExpressionType.LessThan:
                                        operatorObj = new Operator(Operator.OperatorTypes.Less);
                                        break;

                                    case ExpressionType.LessThanOrEqual:
                                        operatorObj = new Operator(Operator.OperatorTypes.LessEqual);
                                        break;

                                    case ExpressionType.GreaterThan:
                                        operatorObj = new Operator(Operator.OperatorTypes.Greater);
                                        break;

                                    case ExpressionType.GreaterThanOrEqual:
                                        operatorObj = new Operator(Operator.OperatorTypes.GreaterEqual);
                                        break;

                                    case ExpressionType.NotEqual:
                                        operatorObj = new Operator(Operator.OperatorTypes.NotEqual);
                                        break;

                                    default:
                                        throw new NotSupportedException("Cassandra does not support specified binary operator");
                                }

                                relations.Add(new Relation(TableCache<T>.CachedTable.Aliases[memberLeft.Member.Name], operatorObj, ParseTerm(binaryExpression.Right)));
                            }
                            else
                            {
                                throw new InvalidOperationException("A column name select must appear on the left hand side of the binary operator");
                            }
                            break;
                    }
                    break;

                default:
                    throw new NotSupportedException($"Cannot parse relation form unsupported expression of type {expression.Type}");
            }

            return relations;
        }

        public string ParseSimpleMemberAccess<T, TResult>(Expression<Func<T, TResult>> select)
        {
            Expression current = select?.Body;

            if (current is MemberExpression member)
            {
                return TableCache<T>.CachedTable.Aliases[member.Member.Name];
            }

            return null;
        }

        private Expression RemoveConvert(Expression select)
        {
            if (select is UnaryExpression unaryExpression &&
               (unaryExpression.NodeType == ExpressionType.Convert ||
                unaryExpression.NodeType == ExpressionType.ConvertChecked))
            {
                select = unaryExpression.Operand;
            }

            return select;
        }
    }
}
