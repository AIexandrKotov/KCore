using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Extensions
{
    public static class NumericExtensions
    {
        public static double Median<T>(this IEnumerable<T> sourceNumbers, Func<T, double> selector)
        {
            if (sourceNumbers == null || !sourceNumbers.Any())
                throw new ArgumentException("Null or empty values");

            var sorted = sourceNumbers.Select(x => selector(x)).ToArray();
            Array.Sort(sorted);

            int size = sorted.Length;
            int mid = size / 2;
            var median = (size % 2 != 0) ? sorted[mid] : (sorted[mid] + sorted[mid - 1]) / 2.0;
            return median;
        }
        public static double Median(this IEnumerable<int> enumerable) => enumerable.Median(x => x);
        public static double Median(this IEnumerable<long> enumerable) => enumerable.Median(x => x);
        public static double Median(this IEnumerable<float> enumerable) => enumerable.Median(x => x);
        public static double Median(this IEnumerable<double> enumerable) => enumerable.Median(x => x);
        public static double Median(this IEnumerable<decimal> enumerable) => enumerable.Median(x => (double)x);
        public static double Product<T>(this IEnumerable<T> sourceNumbers, Func<T, double> selector)
        {
            var prod = 1.0;
            foreach (var x in sourceNumbers)
                prod *= selector(x);
            return prod;
        }
        public static double Product(this IEnumerable<int> enumerable) => enumerable.Product(x => x);
        public static double Product(this IEnumerable<long> enumerable) => enumerable.Product(x => x);
        public static double Product(this IEnumerable<float> enumerable) => enumerable.Product(x => x);
        public static double Product(this IEnumerable<double> enumerable) => enumerable.Product(x => x);
        public static double Product(this IEnumerable<decimal> enumerable) => enumerable.Product(x => (double)x);
        public static bool Chance(this Random rnd, double chance) => chance > rnd.NextDouble();

        public static bool Between<T>(this T target, T left, T right) where T : IComparable<T>
        {
            return target.CompareTo(left) >= 0 && target.CompareTo(right) < 0;
        }
        public static bool Between(this short target, int left, int right)
        {
            return target >= left && target <= right;
        }

        public static T UniversalBetweenize<T>(this T target, T left, T right) where T : IComparable<T>
        {
            if (target.CompareTo(left) < 0) return left;
            if (target.CompareTo(right) >= 0) return right;
            return target;
        }

        public static double Round(this double d, int digits) => Math.Round(d, digits);
        public static float Round(this float f, int digits) => (float)Math.Round((double)f, digits);

        public static decimal RoundDecimal(this double d) => Convert.ToDecimal(Math.Round(d));

        public static int Round(this double d) => Convert.ToInt32(Math.Round(d));
        public static long RoundLong(this double d) => Convert.ToInt64(Math.Round(d));

        public static int Trunc(this double d) => Convert.ToInt32(Math.Truncate(d));

        public static int Round(this float f) => Convert.ToInt32(Math.Round(f));

        public static int Trunc(this float f) => Convert.ToInt32(Math.Truncate(f));

        public static string ToNormalDate(this DateTime date)
        {
            return date.Day.ToString().PadLeft(2, '0') + '.' + date.Month.ToString().PadLeft(2, '0') + '.' + date.Year;
        }

        public static string ToNormalTime(this DateTime time)
        {
            return time.Hour + ":" + time.Minute.ToString().PadLeft(2, '0');
        }

        public static string ToNormalTime(this TimeSpan time)
        {
            return time.Hours + ":" + time.Minutes.ToString().PadLeft(2, '0');
        }

        public static string ToNormalTime(this DateTimeOffset time)
        {
            return time.Hour + ":" + time.Minute.ToString().PadLeft(2, '0');
        }
        public static double Power(this double x, double a) => Math.Pow(x, a);
        public static double Power(this int x, int a) => Math.Pow(x, a);
        public static double ToDegrees(this double x) => x * 180 / Math.PI;
        public static double ToRadians(this double x) => x * Math.PI / 180;
    }
}
