using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Extensions
{
    public static class GenericExtensions
    {
        #region IEnumerable
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var keys = new HashSet<TKey>();
            foreach (var x in enumerable)
                if (keys.Add(selector(x))) yield return x;
        }
        #endregion

        static Type ac_object = typeof(object);
        /// <summary>
        /// Временное решение для клонирования. Действует как MemberwiseClone, но не меняет тип при вызове в потомках.
        /// Время выполнения увеличивается линейно количеству полей. Медленнее MemberwiseClone минимум в 200 раз.
        /// </summary>
        public static T ReflectiveClone<T>(this T value) where T : ICloneable, new()
        {
            var Result = new T();
            var type = typeof(T);
            while (type != ac_object)
            {
                foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    fieldInfo.SetValue(Result, fieldInfo.GetValue(value));
                }
                type = type.BaseType;
            }
            return Result;
        }

        public static class Typeof<T>
        {
            public static readonly Type Raw = typeof(T);
        }

        static class Enumtypeof<T> where T : Enum
        {
            public static readonly Type Raw = typeof(T);
            public static Dictionary<string, T> KeyValues = new Dictionary<string, T>();
            public static Dictionary<T, string> ValuesKeys;

            static Enumtypeof()
            {
                var names = Enum.GetNames(Raw);
                var values = Enum.GetValues(Raw);
                for (var i = 0; i < names.Length; i++) KeyValues.Add(names[i], (T)values.GetValue(i));
                ValuesKeys = KeyValues.ToDictionary(x => x.Value, x => x.Key);
            }
        }

        public static Dictionary<TKey, TValue> AppendKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TValue> obj) where TKey : Enum
        {
            foreach (var value in Enumtypeof<TKey>.KeyValues.Values)
                if (!dict.ContainsKey(value)) dict[value] = obj();
            return dict;
        }
        public static Dictionary<TKey, TValue> AppendKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TKey, TValue> obj) where TKey : Enum
        {
            foreach (var value in Enumtypeof<TKey>.KeyValues.Values)
                if (!dict.ContainsKey(value)) dict[value] = obj(value);
            return dict;
        }

        public static bool Contains<T>(this IEnumerable<string> strings, T e) where T : Enum
        {
            var str = Enumtypeof<T>.ValuesKeys[e];
            return strings.Contains(str);
        }

        public static T ToEnum<T>(this string value) where T : Enum
        {
            return (T)Enumtypeof<T>.KeyValues[value];
        }

        public static bool IsEnum<T>(this string value) where T : Enum
        {
            return Enumtypeof<T>.KeyValues.Keys.Contains(value);
        }

        public static bool IsEnum<T>(this string value, T e) where T : Enum
        {
            return Enumtypeof<T>.ValuesKeys[e] == value;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
            => enumerable.ToDictionary(x => x.Key, x => x.Value);

        public static void ForEach<T>(this T[] self, Action<T> action) => Array.ForEach(self, action);
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var x in self) action(x);
        }
    }
}
