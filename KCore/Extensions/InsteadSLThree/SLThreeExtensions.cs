using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Extensions.InsteadSLThree
{
    public static class SLThreeExtensions
    {
        public static string ReadString(this Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
        public static string[] ReadStrings(this Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut[] ConvertAll<TIn, TOut>(this TIn[] array, Converter<TIn, TOut> func)
            => Array.ConvertAll(array, func);

        public static IEnumerable<object> Enumerate(this IEnumerable enumerable)
        {
            foreach (var x in enumerable)
                yield return x;
        }
        public static IEnumerable<object> Enumerate(this ITuple tuple)
        {
            for (var i = 0; i < tuple.Length; i++)
                yield return tuple[i];
        }

        public static TOut Cast<TIn, TOut>(this TIn o) where TOut : TIn => (TOut)o;
        public static T Cast<T>(this object o) => (T)o;
        public static T? TryCast<T>(this object o) where T : struct => o is T ? (T)o : default;
        public static T TryCastRef<T>(this object o) where T : class => o is T ? (T)o : null;

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

        public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> enumerable, int size)
        {
            var buf = new List<T>();
            foreach (var elem in enumerable)
            {
                buf.Add(elem);
                if (buf.Count == size)
                {
                    yield return buf.ToArray();
                    buf.Clear();
                }
            }
            if (buf.Count > 0)
                yield return buf.ToArray();
        }
    }
}
