using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    public sealed class TerminalContainer : IContainer
    {
        public static readonly IContainer This = new TerminalContainer();

        private TerminalContainer() { }

        public int Left => 0;

        public int Top => 0;

        public int Width => Terminal.FixedWindowWidth;

        public int Height => Terminal.FixedWindowHeight;
    }
}
