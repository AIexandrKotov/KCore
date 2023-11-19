using KCore.Graphics.Containers;
using KCore.Graphics.Uncontrolable;
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
            string title = "", TextAlignment titleAlignment = TextAlignment.Center,
            ConsoleColor? borderColor = null, ConsoleColor? titleTextColor = null,
            ConsoleColor? textBackColor = null, ConsoleColor? textForeColor = null,
            (int, int, int, int)? padding = null, BoundedObject @internal = null,


            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = false, bool fillHeight = false)
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {
            ContentWidth = width ?? Container.Width;
            ContentHeight = height ?? Container.Height;
            Title = title;
            TitleAlignment = titleAlignment;
            BorderColor = borderColor;
            TitleTextColor = titleTextColor;
            TextForeColor = textForeColor;
            TextBackColor = textBackColor;
            Padding = padding ?? (1, 1, 1, 1);
            Internal = @internal;
        }

        public ConsoleColor? BorderColor;
        public ConsoleColor? TitleTextColor;
        public ConsoleColor? TextForeColor;
        public ConsoleColor? TextBackColor;
        public string Title;
        public TextAlignment TitleAlignment;
        /// <summary>
        /// (left, top, right, bottom)
        /// </summary>
        public (int, int, int, int) Padding;

        public BoundedObject Internal;

        public IContainer GetTitleContainer()
        {
            var cont = this as IContainer;
            return new StaticContainer(cont.Left, cont.Top, Width, 1);
        }

        public IContainer GetInternalContainer()
        {
            var cont = this as IContainer;
            return new StaticContainer(cont.Left + Padding.Item1,
                                       cont.Top + Padding.Item2,
                                       cont.Width - Padding.Item1 - Padding.Item3,
                                       cont.Height - Padding.Item2 - Padding.Item4);
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
            Title.PrintSuperText(GetTitleContainer(), () => (titleColor, border));
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
                Internal.Container = GetInternalContainer();
                if (Internal is IWidget widget) widget.UpdateSizes();
            }    
        }
    }
}
