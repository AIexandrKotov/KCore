using KCore.Graphics.Containers;
using KCore.Graphics.Special;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class TextRow : BoundedObject
    {
        public TextRow(
            int? width = null,
            string text = "", TextAlignment textAlignment = TextAlignment.Center,
            ConsoleColor? foreground = null, ConsoleColor? background = null,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = true)
            : base(left, top, container, alignment, fillWidth)
        {
            Fore = foreground;
            Back = background;
            Text = text;
            TextAlignment = textAlignment;
            ContentWidth = width ?? Container.Width;
        }

        public TextAlignment TextAlignment;
        public string Text;
        public ConsoleColor? Fore;
        public ConsoleColor? Back;
        public override int Height => 1;

        public IContainer GetTextContainer(int left, int top)
        {
            return new StaticContainer(left, top, Width, Height);
        }

        public override (int, int) Draw(int left, int top)
        {
            Terminal.Set(left, top);
            var fore = Terminal.Fore = Fore ?? Theme.Fore;
            var back = Terminal.Back = Back ?? Theme.Border;
            Graph.Row(left, top, Width);
            if (!string.IsNullOrEmpty(Text))
                Text.PrintSuperText(GetTextContainer(left, top), () => (fore, back), TextAlignment);
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
