﻿using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;
using static Dynamit.Operator;

namespace Dynamit
{
    /// <summary>
    /// Provides get methods for DDictionary types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Finder<T> where T : DDictionary
    {
        /// <summary>
        /// Returns all entities of the given type
        /// </summary>
        public static IEnumerable<T> All => Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t");

        /// <summary>
        /// Returns the first DDictionary of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns the
        /// first entity found. If no entity is found, returns null.
        /// </summary>
        public static T First(params (string key, Operator op, dynamic value)?[] equalityConditions) =>
            Where(equalityConditions)?.FirstOrDefault();

        private static readonly string selectSql = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName}) " +
                                                   $"FROM {TableInfo<T>.KvpTable} t WHERE t.Key =? AND t.ValueHash";

        private static readonly string countSql = $"SELECT COUNT(t) FROM {TableInfo<T>.KvpTable} t WHERE t.Key =?";

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which the provided equality condition is true. 
        /// </summary>
        public static IEnumerable<T> Where(string key, Operator op, dynamic value) => Where((key, op, value));

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns all entities found. 
        /// </summary>
        public static IEnumerable<T> Where(params (string key, Operator op, dynamic value)?[] equalityConditions)
        {
            if (equalityConditions?.Any(c => c != null) != true) return All;
            var sqlStub = selectSql;
            var equalsNulls = new List<(string key, Operator op, dynamic value)>();
            for (var i = 0; i < equalityConditions.Length; i += 1)
            {
                var cond = equalityConditions[i];
                if (cond != null && (cond.Value.op == EQUALS && cond.Value.value == null))
                {
                    equalsNulls.Add(cond.Value);
                    equalityConditions[i] = null;
                }
            }

            string getSql(Operator op)
            {
                switch (op)
                {
                    case nil: throw new Exception("Invalid operator in Finder condition, cannot be nil");
                    case EQUALS: return "=";
                    case NOT_EQUALS: return "<>";
                    default: return null;
                }
            }

            IEnumerable<T> evaluate((string key, Operator op, dynamic value)? cond)
            {
                if (!cond.HasValue) throw new Exception("Invalid Finder condition. Cannot be null");
                var _cond = cond.Value;
                if (_cond.value == null) return Db.SQL<T>($"{sqlStub} IS NOT NULL", _cond.key);
                return Db.SQL<T>($"{sqlStub} {getSql(_cond.op)}?", _cond.key, _cond.value.GetHashCode());
            }

            var results = new HashSet<T>();
            var evaluated = false;
            equalityConditions.Where(cond => cond != null).ForEach((cond, i) =>
            {
                if (i == 0)
                {
                    results.UnionWith(evaluate(cond));
                    evaluated = true;
                }
                else results.IntersectWith(evaluate(cond));
            });
            if (!evaluated) results.UnionWith(All);
            foreach (var cond in equalsNulls)
            {
                var initialCount = Db.SQL<long>(countSql, cond.key).FirstOrDefault();
                if (initialCount > 0)
                    results.ExceptWith(Db.SQL<T>($"{sqlStub} IS NOT NULL", cond.key));
            }
            return results;
        }
    }
}