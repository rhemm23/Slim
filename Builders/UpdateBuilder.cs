using Cassandra;
using CassandraORM.Tokens;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CassandraORM.Builders
{
    public class UpdateBuilder<T> : QueryBuilder<UpdateQuery, T>
    {
        public UpdateBuilder<T> UseTimestamp(long timestamp)
        {
            UpdateParameter parameter = new UpdateParameter(UpdateParameter.UpdateParameterTypes.Timestamp, GetParameterValue(timestamp));

            CurrentQuery.UpdateParameters.Add(parameter);

            return this;
        }

        public UpdateBuilder<T> UseTTL(long ttl)
        {
            UpdateParameter parameter = new UpdateParameter(UpdateParameter.UpdateParameterTypes.TTL, GetParameterValue(ttl));

            CurrentQuery.UpdateParameters.Add(parameter);

            return this;
        }

        public UpdateBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            CurrentQuery.WhereClause = BuilderHelper.ParseWhere(predicate);

            return this;
        }

        public UpdateBuilder<T> Set(Expression<Func<T, object>> select, object value)
        {
            SimpleSelection selection = BuilderHelper.ParseSimpleSelection(select);

            if (selection != null)
            {
                CurrentQuery.Assignments.Add(new Assignment(selection, GenerateBindMarker(value)));
            }

            return this;
        }

        public UpdateBuilder<T> Set(Expression<Func<T, object>> select, Assignment.CounterModifierTypes counterModifierType, object value)
        {
            string selection = BuilderHelper.ParseSimpleMemberAccess(select);

            if (selection != null)
            {
                CurrentQuery.Assignments.Add(new Assignment(selection, counterModifierType, GenerateBindMarker(value)));
            }

            return this;
        }

        public UpdateBuilder<T> AddToList<TItem>(Expression<Func<T, IList<TItem>>> select, IList<TItem> values)
        {
            BaseAlterList(select, values, Assignment.CollectionModifierTypes.Add);

            return this;
        }

        public UpdateBuilder<T> AddToMap<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> select, IDictionary<TKey, TValue> values)
        {
            string selection = BuilderHelper.ParseSimpleMemberAccess(select);

            if (selection != null &&
                values != null)
            {
                IDictionary<Term, Term> translated = new Dictionary<Term, Term>();

                foreach (KeyValuePair<TKey, TValue> value in values)
                {
                    translated.Add(GenerateBindMarker(value.Key), GenerateBindMarker(value.Value));
                }

                CollectionLiteral literal = new CollectionLiteral(translated);

                CurrentQuery.Assignments.Add(new Assignment(selection, Assignment.CollectionModifierTypes.Add, literal));
            }

            return this;
        }

        public UpdateBuilder<T> AddToSet<TItem>(Expression<Func<T, ISet<TItem>>> select, ISet<TItem> values)
        {
            BaseAlterSet(select, values, Assignment.CollectionModifierTypes.Add);

            return this;
        }

        public UpdateBuilder<T> PrependToList<TItem>(Expression<Func<T, IList<TItem>>> select, IList<TItem> values)
        {
            BaseAlterList(select, values, Assignment.CollectionModifierTypes.Prepend);

            return this;
        }

        public UpdateBuilder<T> InsertIntoList<TItem>(Expression<Func<T, IList<TItem>>> select, IList<TItem> values)
        {
            BaseAlterList(select, values, Assignment.CollectionModifierTypes.Insert);

            return this;
        }

        public UpdateBuilder<T> RemoveFromList<TItem>(Expression<Func<T, IList<TItem>>> select, IList<TItem> values)
        {
            BaseAlterList(select, values, Assignment.CollectionModifierTypes.Remove);

            return this;
        }

        public UpdateBuilder<T> RemoveFromSet<TItem>(Expression<Func<T, ISet<TItem>>> select, ISet<TItem> values)
        {
            BaseAlterSet(select, values, Assignment.CollectionModifierTypes.Remove);

            return this;
        }

        public UpdateBuilder<T> RemoveFromMap<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> select, ISet<TKey> keys)
        {
            string selection = BuilderHelper.ParseSimpleMemberAccess(select);

            if (keys != null &&
                selection != null)
            {
                ISet<Term> translatedList = new HashSet<Term>();

                foreach (TKey value in keys)
                {
                    translatedList.Add(GenerateBindMarker(value));
                }

                CollectionLiteral literal = new CollectionLiteral(translatedList);

                CurrentQuery.Assignments.Add(new Assignment(selection, Assignment.CollectionModifierTypes.Remove, literal));
            }

            return this;
        }

        public bool Execute()
        {
            RowSet rowSet = Session.Execute(BuildQuery());
            Row row = rowSet.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        public async Task<bool> ExecuteAsync()
        {
            RowSet rowSet = await Session.ExecuteAsync(BuildQuery());
            Row row = rowSet.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        public UpdateBuilder<T> IfExists()
        {
            CurrentQuery.IfExists = true;

            return this;
        }

        public UpdateBuilder<T> If(Expression<Func<T, bool>> predicate)
        {
            CurrentQuery.Conditions = BuilderHelper.ParseConditions(predicate);

            return this;
        }

        private IRestrictedValue GetParameterValue(long parameterValue)
        {
            IRestrictedValue value = null;

            if (BuilderHelper.UseBindings)
            {
                string name = GetDistinctAlias();

                value = new BindMarker()
                {
                    Identifier = name
                };

                Parameters.Add(name, parameterValue);
            }
            else
            {
                value = new IntegerConstant()
                {
                    Value = parameterValue
                };
            }

            return value;
        }

        private BindMarker GenerateBindMarker(object value)
        {
            string name = GetDistinctAlias();

            Parameters.Add(name, value);
            
            return new BindMarker()
            {
                Identifier = name
            };
        }

        private void BaseAlterList<TItem>(Expression<Func<T, IList<TItem>>> select, IList<TItem> values, Assignment.CollectionModifierTypes modifierType)
        {
            string selection = BuilderHelper.ParseSimpleMemberAccess(select);

            if (values != null &&
                selection != null)
            {
                List<Term> translatedList = new List<Term>();

                foreach (TItem value in values)
                {
                    translatedList.Add(GenerateBindMarker(value));
                }

                CollectionLiteral literal = new CollectionLiteral(translatedList);

                CurrentQuery.Assignments.Add(new Assignment(selection, modifierType, literal));
            }
        }

        private void BaseAlterSet<TItem>(Expression<Func<T, ISet<TItem>>> select, ISet<TItem> values, Assignment.CollectionModifierTypes modifierType)
        {
            string selection = BuilderHelper.ParseSimpleMemberAccess(select);

            if (values != null &&
                selection != null)
            {
                ISet<Term> translatedList = new HashSet<Term>();

                foreach (TItem value in values)
                {
                    translatedList.Add(GenerateBindMarker(value));
                }

                CollectionLiteral literal = new CollectionLiteral(translatedList);

                CurrentQuery.Assignments.Add(new Assignment(selection, modifierType, literal));
            }
        }
    }
}
