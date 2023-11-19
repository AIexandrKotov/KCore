using KCore.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore
{
    public static class Theme
    {
        public static ConsoleColor Back => Themes.Current.Back;
        public static ConsoleColor Fore => Themes.Current.Fore;
        public static ConsoleColor Disabled => Themes.Current.Disabled;
        public static ConsoleColor Error => Themes.Current.Error;
        public static ConsoleColor Border => Themes.Current.Border;
    }
}
