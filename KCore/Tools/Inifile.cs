using KCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace KCore.Tools
{
    public class Inifile
    {
        public Dictionary<string, Dictionary<string, string>> Value { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        internal Dictionary<ValueTuple<string, string>, string[]> Commentaries { get; set; } = new Dictionary<(string, string), string[]>();
        internal List<string> CurrentComment { get; set; } = new List<string>();

        private Inifile()
        {
            Value = new Dictionary<string, Dictionary<string, string>>();
        }

        private string currentsection = null;

        public bool SaveInfo { get; set; } = true;

        public string CurrentSection
        {
            get
            {
                return currentsection;
            }
            set
            {
                CheckOnContains(value);
                currentsection = value;
            }
        }

        public int Version { get; set; } = 0;

        public Dictionary<string, string> CurrentKeyValue { get => Value[CurrentSection]; set => Value[CurrentSection] = value; }


        private void CheckOnContains(string value)
        {
            if (!Value.ContainsKey(value)) Value.Add(value, new Dictionary<string, string>());
        }

        private void CheckOnContainsWithThrow(string section, string key)
        {
            if (!Value.ContainsKey(section)) throw new KeyNotFoundException();
            if (!Value[section].ContainsKey(key)) throw new KeyNotFoundException();
        }

        private void CheckOnContainsWithThrow(string key)
        {
            if (!Value[CurrentSection].ContainsKey(key)) throw new KeyNotFoundException();
        }

        public bool Contains(string section, string key)
        {
            return Value[section].ContainsKey(key);
        }

        public bool Contains(string key)
        {
            return Value[CurrentSection].ContainsKey(key);
        }

        public string ReadString(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key];
        }

        public string ReadString(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key];
        }

        public bool ReadBool(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            if (bool.TryParse(Value[section][key], out var result)) return result;
            else return Value[section][key].ToInt32() != 0;
        }

        public bool ReadBool(string key)
        {
            CheckOnContainsWithThrow(key);
            if (bool.TryParse(Value[CurrentSection][key], out var result)) return result;
            else return Value[CurrentSection][key].ToInt32() != 0;
        }

        public byte ReadByte(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToByte();
        }

        public byte ReadByte(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToByte();
        }

        public sbyte ReadSByte(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToSByte();
        }

        public sbyte ReadSByte(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToSByte();
        }

        public short ReadInt16(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToInt16();
        }

        public short ReadInt16(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToInt16();
        }

        public ushort ReadUInt16(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToUInt16();
        }

        public ushort ReadUInt16(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToUInt16();
        }

        public int ReadInt32(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToInt32();
        }

        public int ReadInt32(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToInt32();
        }

        public uint ReadUInt32(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToUInt32();
        }

        public uint ReadUInt32(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToUInt32();
        }

        public long ReadInt64(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToInt64();
        }

        public long ReadInt64(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToInt64();
        }

        public ulong ReadUInt64(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToUInt64();
        }

        public ulong ReadUInt64(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToUInt64();
        }

        public float ReadFloat(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToFloat();
        }

        public float ReadFloat(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToFloat();
        }

        public double ReadDouble(string section, string key)
        {
            CheckOnContainsWithThrow(section, key);
            return Value[section][key].ToDouble();
        }

        public double ReadDouble(string key)
        {
            CheckOnContainsWithThrow(key);
            return Value[CurrentSection][key].ToDouble();
        }

        public void WriteComment(string section, string key, string[] text)
        {
            if (Commentaries.ContainsKey((section, key))) Commentaries[(section, key)] = text;
            else Commentaries.Add((section, key), text);
        }

        public void WriteComment(string section, string key, string text)
        {
            if (Commentaries.ContainsKey((section, key))) Commentaries[(section, key)] = new string[1] { text };
            else Commentaries.Add((section, key), new string[1] { text });
        }

        public void WriteComment(string text)
        {
            CurrentComment.Add(text);
        }

        public void WriteComment(string[] text)
        {
            CurrentComment.AddRange(text);
        }

        public void WriteSectionComment(string section, string text)
        {
            WriteComment(section, "", text);
        }

        public void WriteSectionComment(string text)
        {
            WriteComment(CurrentSection, "", text);
        }

        public void WriteSectionComment(string section, string[] text)
        {
            WriteComment(section, "", text);
        }

        public void WriteSectionComment(string[] text)
        {
            WriteComment(CurrentSection, "", text);
        }

        public void Write<T>(string section, string key, T o)
        {
            if (CurrentComment.Count > 0)
            {
                WriteComment(section, key, CurrentComment.ToArray());
                CurrentComment.Clear();
            }
            CheckOnContains(section);
            if (Value[section].ContainsKey(key)) Value[section][key] = o.ToString();
            else Value[section].Add(key, o.ToString());
        }

        public void Write<T>(string key, T o)
        {
            if (CurrentComment.Count > 0)
            {
                WriteComment(CurrentSection, key, CurrentComment.ToArray());
                CurrentComment.Clear();
            }
            if (Value[CurrentSection].ContainsKey(key)) Value[CurrentSection][key] = o.ToString();
            else Value[CurrentSection].Add(key, o.ToString());
        }

        public static Inifile AppendIniFile(string filename) => File.Exists(filename) ? ReadIniFile(filename) : Create();

        public static Inifile Create() => new Inifile();

        public static Inifile ReadIniText(string[] text)
        {
            var Result = new Inifile();

            var CurrentBlock = "INIFILE";

            for (var i = 0; i < text.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(text[i]) && !text[i].Trim().StartsWith(";"))
                {
                    var x = text[i].Trim();

                    if (x.StartsWith("[") && x.EndsWith("]"))
                    {
                        CurrentBlock = x.Substring(1, x.Length - 2);
                        if (!Result.Value.ContainsKey(CurrentBlock)) Result.Value.Add(CurrentBlock, new Dictionary<string, string>());
                    }
                    else
                    {
                        var splitter = x.IndexOf('=');

                        var key = x.Substring(0, splitter);

                        var value = x.Substring(splitter + 1, x.Length - key.Length - 1);

                        Result.Value[CurrentBlock].Add(key, value);
                    }
                }
                else if (text[i].StartsWith("; $Version"))
                {
                    var splitter = text[i].IndexOf('=');
                    Result.Version = text[i].Substring(splitter + 1, text[i].Length - splitter - 1).ToInt32();
                }
            }

            return Result;
        }

        public static Inifile ReadIniFile(string filename)
        {
            return ReadIniText(File.ReadAllLines(filename));
        }

        internal static string[] ReadStrings(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
            }
        }

        public static Inifile ReadIniStream(Stream stream)
        {
            return ReadIniText(ReadStrings(stream));
        }

        public void WriteIniFile(string filename)
        {
            var sb = new System.Text.StringBuilder();
            if (Value.Count > 0)
            {
                if (SaveInfo)
                {
                    sb.AppendLine("; Autogenerated .ini file");
                    sb.AppendLine("; This file generated in KCore");
                }
                if (Version != 0) sb.AppendLine($"; $Version = {Version}");
                var dt = DateTime.Now;
                if (SaveInfo)
                {
                    sb.AppendLine($"; Last update {dt.ToShortDateString()} {dt.ToLongTimeString()}");
                }
                if (Version != 0 || SaveInfo) sb.AppendLine();
            }
            var sections = Value.Keys;
            var id = 0;
            foreach (var section in sections)
            {
                var keys = Value[section].Keys;
                var sectiontuple = (section, "");
                if (Commentaries.ContainsKey(sectiontuple))
                {
                    for (var i = 0; i < Commentaries[sectiontuple].Length; i++)
                        sb.AppendLine($"; {Commentaries[sectiontuple][i]}");
                }
                sb.AppendLine($"[{section}]");
                foreach (var key in keys)
                {
                    var value = Value[section][key];
                    var tuple = (section, key);
                    if (Commentaries.ContainsKey(tuple))
                    {
                        for (var i = 0; i < Commentaries[tuple].Length; i++)
                            sb.AppendLine($"; {Commentaries[tuple][i]}");
                    }
                    sb.AppendLine($"{key}={value}");
                }
                if (id != sections.Count - 1) sb.AppendLine();
                id++;
            }
            File.WriteAllText(filename, sb.ToString());
        }
    }
}
