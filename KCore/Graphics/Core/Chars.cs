using KCore.Extensions;
using KCore.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KCore.Graphics.Core
{
    public static class Chars
    {
        public static Dictionary<char, float> MixingMultipliers { get; private set; }

        public static float GetMixingMultiplier(this char c) => MixingMultipliers.ContainsKey(c) ? MixingMultipliers[c] : 0;

        static Chars()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("KCore.lucida_mixmult.ini"))
            {
                var ini = Inifile.ReadIniStream(stream);
                MixingMultipliers = ini.Value["MAIN"].Keys.ToArray().ToDictionary(x => (char)x.ToInt32(), x => ini.Value["MAIN"][x].ToFloat());
            }
        }
    }
}
