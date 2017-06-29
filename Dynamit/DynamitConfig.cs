﻿using System;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    /// <summary>
    /// The main configuration class for Dynamit
    /// </summary>
    public static class DynamitConfig
    {
        internal static bool EscapeStrings;

        /// <summary>
        /// Sets up indexes and callbacks for Dynamit database classes to 
        /// improve runtime performance.
        /// </summary>
        /// <param name="setupIndexes"></param>
        /// <param name="enableEscapeStrings">If true, all strings surrounded with '\"' will be 
        /// escaped to ordinary strings. Necessary for some string casts to work properly with RESTar</param>
        public static void Init(bool setupIndexes = true, bool enableEscapeStrings = false)
        {
            EscapeStrings = enableEscapeStrings;
            var dicts = typeof(DDictionary).GetConcreteSubclasses();
            var dictsWithMissingAttribute = dicts.Where(d => d.GetAttribute<DDictionaryAttribute>() == null).ToList();
            if (dictsWithMissingAttribute.Any())
                throw new DDictionaryException(dictsWithMissingAttribute.First());
            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            var lists = typeof(DList).GetConcreteSubclasses();
            var listsWithMissingAttribute = lists.Where(d => d.GetAttribute<DListAttribute>() == null).ToList();
            if (listsWithMissingAttribute.Any())
                throw new DListException(listsWithMissingAttribute.First());
            var elements = typeof(DElement).GetConcreteSubclasses();
            if (!setupIndexes) return;
            foreach (var kvp in pairs)
            {
                CreateIndex(kvp, "Dictionary");
                CreateIndex(kvp, "Dictionary", "Key");
                CreateIndex(kvp, "Key", "ValueHash");
            }
            foreach (var element in elements)
            {
                CreateIndex(element, "List");
                CreateIndex(element, "List", "Index");
                CreateIndex(element, "List", "ValueHash");
            }
        }

        private static string Fnuttify(this string sqlKey) => $"\"{sqlKey.Replace(".", "\".\"")}\"";

        private static void CreateIndex(Type table, params string[] cols)
        {
            var indexName = $"DYNAMIT_GENERATED_INDEX_FOR_{table.FullName.Replace('.', '_')}__{string.Join("_", cols)}";
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name =?", indexName).First == null)
                Db.SQL($"CREATE INDEX {indexName} ON {table.FullName} ({string.Join(",", cols.Select(Fnuttify))})");
        }
    }
}