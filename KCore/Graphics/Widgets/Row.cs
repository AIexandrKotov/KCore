using KCore.Graphics.Uncontrolable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class Row : BoundedObject
    {
        public Row(
            int? width = null, ConsoleColor? background = null,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = false)
            : base(left, top, container, alignment, fillWidth)
        {
            Background = background;
            ContentWidth = width ?? Container.Width;
        }

        public ConsoleColor? Background;
        public override int Height => 1;

        public override (int, int) Draw(int left, int top)
        {
            Terminal.Set(left, top);
            Terminal.Back = Background ?? Theme.Border;
            Graph.Row(left, top, Width);
            Terminal.ResetColor();

            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Terminal.Set(left, top);
            Terminal.Back = Theme.Back;
            Graph.Row(left, top, Width);
            Terminal.ResetColor();

            return (left, top);
        }
    }
}
