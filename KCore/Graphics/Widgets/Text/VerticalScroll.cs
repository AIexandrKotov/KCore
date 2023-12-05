using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class VerticalScroll : Widget
    {
        public VerticalScroll(
            IVerticalScrollable scrollable,
            int? height = null,
            char? pixel = null,
            char? scrollPixel = null,
            ConsoleColor? back = null,
            ConsoleColor? fore = null,
            ConsoleColor? backScroll = null,
            ConsoleColor? foreScroll = null,
            bool enabled = true,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillHeight = true)
            : base(left, top, container, alignment, false, fillHeight)
        {
            ContentHeight = height ?? Container.Height;
            Scrollable = scrollable;
            Back = back ?? Theme.Back;
            Fore = fore ?? Theme.Fore;
            Pixel = pixel ?? '░';
            CursorBack = backScroll ?? Theme.Back;
            CursorFore = foreScroll ?? Theme.Fore;
            ScrollPixel = scrollPixel ?? '█';
            Enabled = enabled;
        }

        public IVerticalScrollable Scrollable { get; set; }
        public override int Width => 1;

        public ConsoleColor Back { get; set; }
        public ConsoleColor Fore { get; set; }
        public ConsoleColor CursorBack { get; set; }
        public ConsoleColor CursorFore { get; set; }
        public char Pixel { get; set; }
        public char ScrollPixel { get; set; }

        public override (int, int) Draw(int left, int top)
        {
            var start_cursor = (int)Math.Round(Scrollable.CurrentIndent / (double)Scrollable.Length * Height);
            var end_cursor = start_cursor + (int)Math.Round((Scrollable.Height / (double)Scrollable.Length) * Height);
            if (end_cursor > Height) end_cursor = Height;
            if (end_cursor < start_cursor)
            {
                start_cursor = 0;
                end_cursor = Height;
            }
            Terminal.Back = Back;
            Terminal.Fore = Fore;
            for (var i = 0; i < start_cursor; i++)
            {
                Terminal.Set(left, top + i);
                Terminal.Write(Pixel);
            }
            Terminal.Back = CursorBack;
            Terminal.Fore = CursorFore;
            for (var i = start_cursor; i < end_cursor; i++)
            {
                Terminal.Set(left, top + i);
                Terminal.Write(ScrollPixel);
            }
            Terminal.Back = Back;
            Terminal.Fore = Fore;
            for (var i = end_cursor; i < Height; i++)
            {
                Terminal.Set(left, top + i);
                Terminal.Write(Pixel);
            }
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Graph.FillRectangle(Theme.Back, left, top, Width, Height);
            return (left, top);
        }

        public override void Resize()
        {
            if (FillHeight) ContentHeight = Container.Height;
        }
    }
}
