using KCore.Extensions.InsteadSLThree;
using KCore.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Storage
{
    public sealed class Localization
    {
        private Dictionary<string, string> Data;
        public string this[string text] => Data.TryGetValue(text, out var value) ? value : text;

        public readonly string Name;
        public readonly string LocalName;

        private static readonly Localization EnglishLocalization;
        public static Localization[] Localizations = new Localization[0];
        public static Localization Current { get; set; }

        public static void Next()
        {
            var ind = Array.IndexOf(Localizations, Current);
            ind++;
            if (ind >= Localizations.Length) ind = 0;
            Current = Localizations[ind];
        }
        public static void Prev()
        {
            var ind = Array.IndexOf(Localizations, Current);
            ind--;
            if (ind < 0) ind = Localizations.Length - 1;
            Current = Localizations[ind];
        }

        private Localization()
        {
            Name = "English";
            LocalName = "English";
            Data = Initial.FromIniText(KCoreAssembly.GetManifestResourceStream("KCore.__.Localizations.en.ini").ReadString())[""].ToDictionary(x => x.Key, x => x.Value);
        }

        public Localization(Initial initial)
        {
            var info = initial["INFO"];
            Name = info["Name"];
            LocalName = info["LocalName"];

            Data = initial[""].ToDictionary(x => x.Key, x => x.Value);

            foreach (var x in EnglishLocalization.Data)
                if (!Data.ContainsKey(x.Key)) Data.Add(x.Key, x.Value);
        }

        static Localization()
        {
            EnglishLocalization = new Localization();
        }
        static Assembly KCoreAssembly = Assembly.GetExecutingAssembly();

        internal static void InitDefault()
        {
            Localizations = KCoreAssembly.GetManifestResourceNames().Where(x => x.StartsWith("KCore.__.Localizations.")).Select(x => new Localization(Initial.FromIniText(KCoreAssembly.GetManifestResourceStream(x).ReadString()))).ToArray();
            Current = Localizations[0];
        }

        public static void InitStorage()
        {
            var Localizations = Directory.GetFiles(Path.Combine(KCoreStorage.KCorePath, "Localizations"), "*.ini").Select(x => new Localization(Initial.FromIniText(File.ReadAllText(x)))).ToArray();
            var ind = Array.FindIndex(Localizations, x => x.Name == Current?.Name);
            if (Localizations.Length != 0)
                Localization.Localizations = Localizations;
            if (ind != -1) Current = Localizations[ind];
        }
    }
}
