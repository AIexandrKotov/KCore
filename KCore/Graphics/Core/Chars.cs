using KCore.Extensions;
using KCore.Extensions.InsteadSLThree;
using KCore.Tools;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace KCore.Graphics.Core
{
    public static class Chars
    {
        public static readonly Dictionary<char, float> MixingMultipliers;

        public static float GetMixingMultiplier(this char c) => MixingMultipliers.ContainsKey(c) ? MixingMultipliers[c] : 0;

        static Chars()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("KCore.lucida_mixmult.ini"))
            {
                var ini = Initial.FromIniText(stream.ReadString());
                MixingMultipliers = ini["MAIN"].ToDictionary(x => (char)x.Key.ToInt32(), x => float.Parse(x.Value, CultureInfo.InvariantCulture));
            }
        }
    }
}
