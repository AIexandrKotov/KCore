using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Theming
{
    public class ColorTheme
    {
        public const int ForeID = 0;
        public const int BackID = 1;
        public const int DisabledID = 2;
        public const int ErrorID = 3;
        public const int BorderID = 4;
        
        public ConsoleColor[] All = new ConsoleColor[256];
        public ColorTheme() { }

        public ConsoleColor Fore { get => All[ForeID]; set => All[ForeID] = value; }
        public ConsoleColor Back { get => All[BackID]; set => All[BackID] = value; }
        public ConsoleColor Disabled { get => All[DisabledID]; set => All[DisabledID] = value; }
        public ConsoleColor Error { get => All[ErrorID]; set => All[ErrorID] = value; }
        public ConsoleColor Border { get => All[BorderID]; set => All[BorderID] = value; }
    }
}
