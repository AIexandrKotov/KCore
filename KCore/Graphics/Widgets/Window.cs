using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class Window : BoundedObject, IWidget
    {
        public Window(
            int? width = null, int? height = null,
            string title = "", TextAlignment titleAlignment = TextAlignment.Center, (int, int)? titlePadding = null,
            ConsoleColor? borderColor = null, ConsoleColor? titleTextColor = null,
            ConsoleColor? textBackColor = null, ConsoleColor? textForeColor = null,
            (int, int, int, int)? padding = null, BoundedObject @internal = null,


            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = false, bool fillHeight = false)
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {
            ContentWidth = width ?? Container.Width;
            ContentHeight = height ?? Container.Height;
            Title = title;
            TitlePadding = titlePadding ?? (1, 1);
            TitleAlignment = titleAlignment;
            TitleTextColor = titleTextColor;
            BorderColor = borderColor;
            TextForeColor = textForeColor;
            TextBackColor = textBackColor;
            Padding = padding ?? (1, 1, 1, 1);
            Internal = @internal;
            UpdateSizes();
        }

        public ConsoleColor? BorderColor;
        public ConsoleColor? TitleTextColor;
        public ConsoleColor? TextForeColor;
        public ConsoleColor? TextBackColor;
        public (int, int) TitlePadding;
        public string Title;
        public TextAlignment TitleAlignment;
        /// <summary>
        /// (left, top, right, bottom)
        /// </summary>
        public (int, int, int, int) Padding;

        public BoundedObject Internal;

        public IContainer GetTitleContainer(int left, int top)
        {
            return new StaticContainer(
                left + TitlePadding.Item1,
                top,
                Width - TitlePadding.Item1 - TitlePadding.Item2,
                1);
        }

        public IContainer GetInternalContainer(int left, int top)
        {
            return new StaticContainer(
                left + Padding.Item1,
                top + Padding.Item2,
                Width - Padding.Item1 - Padding.Item3,
                Height - Padding.Item2 - Padding.Item4);
        }

        public override (int, int) Draw(int left, int top)
        {
            Terminal.Set(left, top);
            var border = Terminal.Back = BorderColor ?? Theme.Border;
            Graph.Row(left, top, Width);
            Graph.Row(left, top + Height - 1, Width);
            Graph.Column(left, top, Height);
            Graph.Column(left + Width - 1, top, Height);
            var titleColor = Terminal.Fore = TitleTextColor ?? Theme.Fore;
            Title.PrintSuperText(GetTitleContainer(left, top), () => (titleColor, border));
            Terminal.ResetColor();
            Internal?.Draw();

            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Graph.FillRectangle(Theme.Back, left, top, Width, Height);
            return (left, top);
        }

        public void UpdateSizes()
        {
            if (Internal != null)
            {
                var (left, top) = GetCorner();
                Internal.Container = GetInternalContainer(left, top);
                if (Internal is IWidget widget) widget.UpdateSizes();
            }    
        }
    }
}
