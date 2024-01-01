using KCore.Extensions;
using KCore.Extensions.InsteadSLThree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KCore.Tools
{
    public class Initial : IEnumerable<KeyValuePair<string, Dictionary<string, string>>>
    {
        private Dictionary<string, Dictionary<string, string>> Data;

        public struct IniSection : IEnumerable<KeyValuePair<string, string>>
        {
            private Initial Reference;
            public readonly string SectionName;
            public string this[string index]
            {
                get => Reference.Data[SectionName].TryGetValue(index, out var value) ? value : null;
                set => Reference.Data[SectionName][index] = value?.ToString() ?? "";
            }
            public bool TryGetValue(string index, out string value)
            {
                var val = this[index];
                value = null;
                if (val == null) return false;
                value = val.ToString();
                return true;
            }

            private static IEnumerable<MemberInfo> GetMembers(Type type, BindingFlags addition)
            {
                //public get+set properties
                //public non-readonly fields
                foreach (var x in type.GetProperties(BindingFlags.Public | addition | BindingFlags.SetProperty | BindingFlags.GetProperty))
                    if (x.GetIndexParameters().Length == 0)
                        yield return x;
                foreach (var x in type.GetFields(BindingFlags.Public | addition | BindingFlags.SetField | BindingFlags.GetField))
                    yield return x;
            }
            private void InternalSerialize(object o, Type type, BindingFlags addition)
            {
                foreach (var x in GetMembers(type, addition))
                {
                    if (x is PropertyInfo pi)
                        this[pi.Name] = pi.GetValue(o)?.ToString();
                    if (x is FieldInfo fi)
                        this[fi.Name] = fi.GetValue(o)?.ToString();
                }
            }
            public void Serialize(object o)
            {
                if (o == null) return;
                InternalSerialize(o, o.GetType(), BindingFlags.Instance);
            }
            public void SerializeStatic(Type type)
            {
                InternalSerialize(null, type, BindingFlags.Static);
            }

            public IniSection(Initial inifile, string section)
            {
                Reference = inifile;
                SectionName = section;
                if (!inifile.Data.ContainsKey(section)) inifile.Data.Add(section, new Dictionary<string, string>());
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return Reference.Data[SectionName].GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public Initial()
        {
            Data = new Dictionary<string, Dictionary<string, string>>();
        }

        public IniSection this[string index]
        {
            get => new IniSection(this, index);
        }

        public static bool DefaultSaveInfo { get; set; } = true;
        public bool SaveInfo { get; set; } = DefaultSaveInfo;
        public static bool DefaultSaveInfoGeneration { get; set; } = false;
        public bool SaveInfoGeneration { get; set; } = DefaultSaveInfoGeneration;
        public string[] SaveInfoText { get; set; } = new string[0];

        public string ToIniText()
        {
            var sb = new System.Text.StringBuilder();
            if (SaveInfo)
            {
                if (SaveInfoGeneration)
                {
                    sb.AppendLine("; This file generated with KCore");
                    var dt = DateTime.Now;
                    sb.AppendLine($"; Last update {dt:dd.MM.yyyy} {dt:H:mm:ss}");
                    sb.AppendLine();
                }
                if (SaveInfoText.Length > 0)
                    sb.AppendLine(SaveInfoText.Select(x => "; " + x).JoinIntoString("\n"));
            }
            sb.AppendLine(this.Select(section => $"[{section.Key}]\n{section.Value.Select(pair => $"{pair.Key}={pair.Value}").JoinIntoString("\n")}").JoinIntoString("\n"));
            return sb.ToString();
        }
        public static Initial FromIniText(string all)
        {
            var Result = new Initial();
            var text = all.Split(Environment.NewLine.ToCharArray());
            Result.Data[""] = new Dictionary<string, string>();

            var CurrentSection = "";

            for (var i = 0; i < text.Length; i++)
            {
                var x = text[i].Trim();
                if (!string.IsNullOrWhiteSpace(x) && !x.StartsWith(";"))
                {

                    if (x.StartsWith("[") && x.EndsWith("]"))
                    {
                        CurrentSection = x.Substring(1, x.Length - 2);
                        if (!Result.Data.ContainsKey(CurrentSection)) Result.Data.Add(CurrentSection, new Dictionary<string, string>());
                    }
                    else
                    {
                        var splitter = x.IndexOf('=');

                        var key = x.Substring(0, splitter);

                        var value = x.Substring(splitter + 1, x.Length - key.Length - 1);

                        Result.Data[CurrentSection][key] = value;
                    }
                }
            }

            return Result;
        }

        public static void Test()
        {
            var ini = new Initial();
            var section = ini["Section"];
            section["x"] = "2";
            var x = section["x"];
            section.SerializeStatic(typeof(Console));
            File.WriteAllText("test_ini.ini", ini.ToIniText());
            var ini2 = Initial.FromIniText(File.ReadAllText("test_ini.ini"));
        }

        public IEnumerator<KeyValuePair<string, Dictionary<string, string>>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
