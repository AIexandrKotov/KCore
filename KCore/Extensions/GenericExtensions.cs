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
        public static List<T> WhereList<T>(this IEnumerable<T> self, Predicate<T> func)
        {
            var x = self.ToList();
            x.RemoveAll(a => !func(a));
            return x;
        }

        public static string JoinIntoString<T>(this IEnumerable<T> e, string delim)
        {
            var g = e.GetEnumerator();
            var sb = new StringBuilder("");
            if (g.MoveNext())
            {
                sb.Append(g.Current.ToString());
                while (g.MoveNext()) sb.Append(delim + g.Current.ToString());
            }
            return sb.ToString();
        }

        public static string JoinIntoString<T>(this IEnumerable<T> e)
        {
            if (typeof(T) == typeof(char)) return e.JoinIntoString("");
            else return e.JoinIntoString(" ");
        }

        public static IEnumerable<int> Indices<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count; i++) yield return i;
        }

        public static T MinBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var comp = Comparer<TKey>.Default;
            return enumerable.Aggregate((min, x) => comp.Compare(selector(x), selector(min)) < 0 ? x : min);
        }

        public static T MaxBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var comp = Comparer<TKey>.Default;
            return enumerable.Aggregate((max, x) => comp.Compare(selector(x), selector(max)) > 0 ? x : max);
        }

        #endregion

        #region Array
        public static T1[] ConvertAll<T, T1>(this T[] array, Func<T, T1> converter)
        {
            return Array.ConvertAll(array, t => converter(t));
        }

        public static T[] Transform<T>(this T[] array, Func<T, T> func)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = func(array[i]);
            return array;
        }
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var keys = new HashSet<TKey>();
            foreach (var x in enumerable)
                if (keys.Add(selector(x))) yield return x;
        }
        static Random RndElement = new Random();
        public static T RandomElement<T>(this IList<T> list) => list.Count > 0 ? list[RndElement.Next(list.Count)] : default;

        public static IEnumerable<(int, T)> Enumerate<T>(this T[] values)
        {
            for (var i = 0; i < values.GetLength(0); i++)
            {
                yield return (i, values[i]);
            }
        }
        public static IEnumerable<(int, int, T)> Enumerate<T>(this T[,] values)
        {
            for (var i = 0; i < values.GetLength(0); i++)
            {
                for (var j = 0; j < values.GetLength(1); j++)
                {
                    yield return (i, j, values[i, j]);
                }
            }
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

        public static Dictionary<T, T1> Copy<T, T1>(this Dictionary<T, T1> self)
        {
            return new Dictionary<T, T1>(self);
        }

        public static Dictionary<T, T1> Clone<T, T1>(this Dictionary<T, T1> self) where T1 : ICloneable
        {
            var newdict = new Dictionary<T, T1>();
            foreach (var x in self) newdict.Add(x.Key, x.Value.CloneCast());
            return newdict;
        }

        public interface IDictKey<T>
        {
            T DictKey { get; }
        }

        public static Dictionary<TKey, TValue> Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value) where TValue : IDictKey<TKey>
        {
            dict.Add(value.DictKey, value);
            return dict;
        }
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
            => enumerable.ToDictionary(x => x.Key, x => x.Value);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue> enumerable) where TValue : IDictKey<TKey>
            => enumerable.ToDictionary(x => x.DictKey, x => x);

        public static Dictionary<TKey, TValue> CloneModifiedKey<TKey, TValue>(this Dictionary<TKey, TValue> self) where TValue : IDictKey<TKey>, ICloneable
        {
            var newdict = new Dictionary<TKey, TValue>();
            foreach (var x in self)
            {
                var value = x.Value.CloneCast();
                newdict.Add(value.DictKey, value);
            }
            return newdict;
        }

        public static T[] Clone<T>(this T[] self) where T : ICloneable
        {
            return self.ConvertAll(x => (T)x.Clone());
        }

        public static List<T> Copy<T>(this List<T> self)
        {
            return new List<T>(self);
        }

        public static List<T> Clone<T>(this List<T> self) where T : ICloneable
        {
            var newlist = new List<T>(self.Count);
            foreach (var x in self) newlist.Add((T)x.Clone());
            return newlist;
        }

        public static Queue<T> Copy<T>(this Queue<T> self)
        {
            return new Queue<T>(self);
        }

        public static Queue<T> Clone<T>(this Queue<T> self) where T : ICloneable
        {
            var newlist = new Queue<T>(self.Count);
            foreach (var x in self) newlist.Enqueue((T)x.Clone());
            return newlist;
        }

        public static void Clear<T>(this List<T>[] lists)
        {
            for (var i = 0; i < lists.Length; i++) lists[i].Clear();
        }

        public static T Cast<T>(this object o) => (T)o;
        public static T CloneCast<T>(this ICloneable o) => o != null ? (T)o.Clone() : default;
        public static T CloneCast<T>(this T o) where T : ICloneable => o != null ? (T)o.Clone() : default;
        public static T CloneCastUnsafe<T>(this ICloneable o) where T : ICloneable => (T)o.Clone();
        public static T CloneCastUnsafe<T>(this T o) where T : ICloneable => (T)o.Clone();

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int startpos) => source.Skip(startpos);
        public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int startpos, int count) => source.Skip(startpos).Take(count);
        public static List<T> Slice<T>(this List<T> source, int startpos) => new List<T>(source.Skip(startpos));
        public static List<T> Slice<T>(this List<T> source, int startpos, int count) => new List<T>(source.Skip(startpos).Take(count));
        public static bool Starts<T>(this IEnumerable<T> source, Func<T, bool> pred)
        {
            var enmr = source.GetEnumerator();
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            return pred(enmr.Current);
        }
        public static bool Starts<T>(this IEnumerable<T> source, Func<T, bool> pred1, Func<T, bool> pred2)
        {
            var enmr = source.GetEnumerator();
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            var x1 = enmr.Current;
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            var x2 = enmr.Current;
            return pred1(x1) && pred2(x2);
        }
        public static bool Starts<T>(this IEnumerable<T> source, Func<T, bool> pred1, Func<T, bool> pred2, Func<T, bool> pred3)
        {
            var enmr = source.GetEnumerator();
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            var x1 = enmr.Current;
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            var x2 = enmr.Current;
            if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
            var x3 = enmr.Current;
            return pred1(x1) && pred2(x2) && pred3(x3);
        }
        public static bool Starts<T>(this IEnumerable<T> source, params Func<T, bool>[] preds)
        {
            var enmr = source.GetEnumerator();
            for (var i = 0; i < preds.Length; i++)
            {
                if (!enmr.MoveNext()) throw new ArgumentException("Wrong elements count in sequence", nameof(source));
                if (!preds[i](enmr.Current)) return false;
            }
            return true;
        }
        public static bool Ends<T>(this IEnumerable<T> source, Func<T, bool> pred) => source.Reverse().Starts(pred);
        public static bool Ends<T>(this IEnumerable<T> source, Func<T, bool> pred1, Func<T, bool> pred2) => source.Reverse().Starts(pred1, pred2);
        public static bool Ends<T>(this IEnumerable<T> source, Func<T, bool> pred1, Func<T, bool> pred2, Func<T, bool> pred3) => source.Reverse().Starts(pred1, pred2, pred3);
        public static bool Ends<T>(this IEnumerable<T> source, params Func<T, bool>[] pred) => source.Reverse().Starts(pred);

        public static void Add<T, T1>(this Dictionary<T, T1> self, KeyValuePair<T, T1> pair)
        {
            self.Add(pair.Key, pair.Value);
        }

        public static void Add<T, T1>(this Dictionary<T, T1> self, ValueTuple<T, T1> pair)
        {
            self.Add(pair.Item1, pair.Item2);
        }

        public static void Add<T, T1>(this Dictionary<T, T1> self, Tuple<T, T1> pair)
        {
            self.Add(pair.Item1, pair.Item2);
        }

        public static void Add<T, T1>(this Dictionary<T, T1> self, T key, T1? value) where T1 : struct
        {
            if (value.HasValue) self.Add(key, value.Value);
        }

        public static void Add<T, T1>(this Dictionary<T, T1> self, Dictionary<T, T1> other)
        {
            foreach (var pair in other)
                self[pair.Key] = pair.Value;
        }

        public static void Add<T>(this List<T> self, List<T> other)
        {
            for (var i = 0; i < other.Count; i++) self.Add(other[i]);
        }

        public static void ForEach<T>(this T[] self, Action<T> action) => Array.ForEach(self, action);

        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var x in self) action(x);
        }

        public static TOut FirstOfType<TIn, TOut>(this IEnumerable<TIn> self) where TOut : TIn => (TOut)self.First(x => x is TOut);

        public static void Distillate<T>(this (List<T>, List<T>) distcontext, int index)
        {
            distcontext.Item2.Add(distcontext.Item1[index]);
            distcontext.Item1.RemoveAt(index);
        }

        /// <summary>
        /// Универсальная перегонка
        /// </summary>
        /// <typeparam name="T">Перегоняемый тип</typeparam>
        /// <param name="inpt">Входная последовательность</param>
        /// <param name="action">Дополнительные действия перед перегонкой (аргументы копия входной и выходная последовательности)</param>
        /// <param name="selector">Селектор перегонки, зависимый от вывода и текущего значения</param>
        /// <returns>Выходная последовательность</returns>
        public static IEnumerable<T> Distillation<T>(this IList<T> inpt, Action<List<T>, List<T>> action, Func<List<T>, T, bool> selector)
        {
            var input = inpt.ToList();
            var output = new List<T>();

            action(input, output);

            var index = 0;
            var infinity = false;
            while (input.Count > 0)
            {
                var current = input[index];
                if (selector(output, current))
                {
                    infinity = false;
                    output.Add(input[index]);
                    input.RemoveAt(index);
                    index = 0;
                }
                else index++;
                if (index >= input.Count)
                {
                    if (infinity) break;
                    infinity = true;
                    index = 0;
                }
            }
            return output;
        }

        /// <summary>
        /// Универсальная перегонка
        /// </summary>
        /// <typeparam name="T">Перегоняемый тип</typeparam>
        /// <param name="inpt">Входная последовательность</param>
        /// <param name="action">Дополнительные действия перед перегонкой (аргументы копия входной и выходная последовательности)</param>
        /// <param name="selector">Селектор перегонки, зависимый от вывода и текущего значения</param>
        /// <returns>Выходная последовательность</returns>
        public static IEnumerable<T> SortedDistillation<T>(this IList<T> inpt, Action<List<T>, Queue<T>> action, Func<Queue<T>, T, bool> selector)
        {
            var input = inpt.ToList();
            var output = new Queue<T>();

            action(input, output);

            var index = 0;
            var infinity = false;
            while (input.Count > 0)
            {
                var current = input[index];
                if (selector(output, current))
                {
                    infinity = false;
                    output.Enqueue(input[index]);
                    input.RemoveAt(index);
                    index = 0;
                }
                else index++;
                if (index >= input.Count)
                {
                    if (infinity) break;
                    infinity = true;
                    index = 0;
                }
            }
            return output;
        }
    }
}
