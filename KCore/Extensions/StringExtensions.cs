using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Extensions
{
    public static class StringExtensions
    {
        #region KTXCore
        private static System.Resources.ResourceManager mscorlibresources = new System.Resources.ResourceManager("mscorlib", typeof(object).Assembly);
        private static string ErrorStringFromResource(string s)
        {
            return mscorlibresources.GetString(s);
        }

        public static string[] ToSinglestringArray(this string s) => new string[1] { s };

        public static sbyte ToSByte(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = (c - 48);
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > sbyte.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_SByte"));
                Result = Result * 10 + (c - 48);
                j += 1;
            }
            if (Result < 0)
            {
                if (Result == sbyte.MinValue && sign == -1) return (sbyte)Result;
                else throw new OverflowException(ErrorStringFromResource("Overflow_SByte"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return (sbyte)Result;
        }
        public static short ToInt16(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = (c - 48);
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > short.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_Int16"));
                Result = (short)(Result * 10 + (c - 48));
                j += 1;
            }
            if (Result < 0)
            {
                if (Result == short.MinValue && sign == -1) return (short)Result;
                else throw new OverflowException(ErrorStringFromResource("Overflow_Int16"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return (short)Result;
        }
        public static int ToInt32(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = c - 48;
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > int.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_Int32"));
                Result = Result * 10 + (c - 48);
                j += 1;
            }
            if (Result < 0)
            {
                if (Result == int.MinValue && sign == -1) return Result;
                else throw new OverflowException(ErrorStringFromResource("Overflow_Int32"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return Result;
        }
        public static long ToInt64(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            long Result = c - 48;
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > long.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_Int64"));
                Result = Result * 10 + (c - 48);
                j += 1;
            }
            if (Result < 0)
            {
                if (Result == long.MinValue && sign == -1) return Result;
                else throw new OverflowException(ErrorStringFromResource("Overflow_Int64"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return Result;
        }
        public static byte ToByte(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = (c - 48);
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > byte.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_Byte"));
                Result = Result * 10 + (c - 48);
                j += 1;
            }
            if (Result < 0)
            {
                throw new OverflowException(ErrorStringFromResource("Overflow_Byte"));
            }
            if (sign == -1)
            {
                throw new OverflowException(ErrorStringFromResource("Overflow_Byte"));
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return (byte)Result;
        }
        public static ushort ToUInt16(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = (c - 48);
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > ushort.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_UInt16"));
                Result = (ushort)(Result * 10 + (c - 48));
                j += 1;
            }
            if (Result < 0)
            {
                throw new OverflowException(ErrorStringFromResource("Overflow_UInt16"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return (ushort)Result;
        }
        public static uint ToUInt32(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            long Result = (c - 48);
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > int.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_UInt32"));
                Result = (Result * 10 + (c - 48));
                j += 1;
            }
            if (Result < 0)
            {
                if (Result == int.MinValue && sign == -1) return (uint)Result;
                else throw new OverflowException(ErrorStringFromResource("Overflow_UInt32"));
            }
            if (sign == -1)
            {
                Result = -Result;
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return (uint)Result;
        }
        public static ulong ToUInt64(this string s)
        {
            var j = 1;
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1])) j += 1;
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var sign = 0;
            if (s[j - 1] == '-')
            {
                sign = -1;
                j += 1;
            }
            else if (s[j - 1] == '+')
            {
                sign = 1;
                j += 1;
            }
            if (j > s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var c = (int)s[j - 1];
            if (c < 48 || c > 57) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            var Result = (ulong)c - 48;
            j += 1;
            while (j <= s.Length)
            {
                c = (int)s[j - 1];
                if (c > 57) break;
                if (c < 48) break;
                if (Result > ulong.MaxValue) throw new OverflowException(ErrorStringFromResource("Overflow_UInt64"));
                Result = (ulong)(Result * 10 + (ulong)(c - 48));
                j += 1;
            }
            if (sign == -1)
            {
                throw new OverflowException(ErrorStringFromResource("Overflow_UInt64"));
            }
            while (j <= s.Length && char.IsWhiteSpace(s[j - 1]))
            {
                j += 1;
            }
            if (j < s.Length) throw new FormatException(ErrorStringFromResource("Format_InvalidString"));
            return Result;
        }
        public static float ToFloat(this string s) => float.Parse(s);
        public static double ToDouble(this string s) => double.Parse(s);
        public static bool ToBool(this string s) => bool.Parse(s);
        public static DateTime ToDateTime(this string s) => DateTime.Parse(s);

        public static TimeSpan HHMMToTime(this string s)
        {
            var ind = s.IndexOf(':');
            return new TimeSpan(s.Substring(0, ind).ToInt32(), s.Substring(ind + 1).ToInt32(), 0);
        }

        public static string[] ToWords(this string s, params char[] delim) => s.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        public static string Left(this string s, int length)
        {
            length = length > 0 ? length : 0;

            if (s.Length > length)
                return s.Substring(0, length);
            else return s;
        }
        public static string Right(this string s, int length)
        {
            length = length > 0 ? length : 0;

            if (s.Length > length)
                return s.Substring(s.Length - length, length);
            else return s;
        }
        public static string Remove(this string Self, params string[] targets)
        {
            var builder = new StringBuilder(Self);

            for (var i = 0; i < targets.Length; i++)
            {
                builder.Replace(targets[i], String.Empty);
            }

            return builder.ToString();
        }
        public static string Capitalize(this string s) => char.ToUpper(s[0]) + s.Substring(1, s.Length - 1).ToLower();

        public static int IndexOfMany(string s, string value1, string value2)
        {
            var a = s.IndexOf(value1);
            if (a != -1) return a;
            a = s.IndexOf(value2);
            return a;
        }

        public static int IndexOfMany(string s, string value1, string value2, string value3)
        {
            var a = s.IndexOf(value1);
            if (a != -1) return a;
            a = s.IndexOf(value2);
            if (a != -1) return a;
            a = s.IndexOf(value3);
            return a;
        }

        public static int IndexOfMany(string s, params string[] values)
        {
            for (var i = 0; i <= values.Length; i++)
            {
                var a = s.IndexOf(values[i]);
                if (a != -1) return a;
            }
            return -1;
        }
        #endregion

        private static char[] split_chars = new char[] { ' ', '\t', '\r', '\n' };
        public static string[] SizeSeparate(this string[] strings, int width, bool removeemptyentires = true)
        {
            var PreRes = new System.Collections.Generic.List<string>();
            if (strings == null) return PreRes.ToArray();
            for (var i = 0; i < strings.Length; i++)
            {
                var StrBuild = new System.Text.StringBuilder();
                var TWds = strings[i].Split(split_chars, StringSplitOptions.RemoveEmptyEntries);
                if (TWds != null)
                {
                    for (var j = 0; j < TWds.Length; j++)
                    {
                        if (StrBuild.Length + TWds[j].Length < width)
                        {
                            if (j > 0) StrBuild.Append(' ');
                            StrBuild.Append(TWds[j]);
                        }
                        else
                        {
                            PreRes.Add(StrBuild.ToString());
                            StrBuild.Clear();
                            StrBuild.Append(TWds[j]);
                        }
                    }
                }
                var str = StrBuild.ToString();
                if (!removeemptyentires || str != "") PreRes.Add(str);
            }
            return PreRes.ToArray();
        }

        public static string[] SizeSeparate(this string s, int width, bool removeemptyentires = true) => SizeSeparate(new string[1] { s }, width, removeemptyentires);
    }
}
