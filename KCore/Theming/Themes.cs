using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Theming
{
    public static class Themes
    {
        public static ColorTheme Default { get; } = new ColorTheme()
        {
            Back = ConsoleColor.White,
            Fore = ConsoleColor.Black,
            Disabled = ConsoleColor.Gray,
            Error = ConsoleColor.Red,
            Border = ConsoleColor.Gray,
        };

        public static ColorTheme Current { get; set; } = Default;
    }
}
