using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class WithVerticalScroll : Widget
    {
        public WithVerticalScroll(
            IVerticalScrollable content,
            VerticalScroll scroll = null,
            Position scrollPosition = Position.Right,
            bool scrollBindingWithContent = true,
            (int, int)? spacing = null,
            (int, int)? expanding = null,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = true, bool fillHeight = true)
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {
            Content = content as BoundedObject;
            Scroll = scroll ?? new VerticalScroll(content);
            if (scrollBindingWithContent)
                content.Scroll = Scroll;
            ScrollPosition = scrollPosition;
            Spacing = spacing ?? (0, 0);
            Expanding = expanding ?? (0, 0);
        }

        public enum Position
        {
            Left,
            Right
        }

        public BoundedObject Content { get; set; }
        public VerticalScroll Scroll { get; set; }
        public Position ScrollPosition { get; set; }
        public (int, int) Spacing { get; set; }
        public (int, int) Expanding { get; set; }

        public override (int, int) Draw(int left, int top)
        {
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            return (left, top);
        }

        public IContainer GetContentContainer(int left, int top)
        {
            var l = ScrollPosition == Position.Left ? (left + Spacing.Item1 + Spacing.Item2 + 1) : left;
            return new StaticContainer(l, top, Width - 1 - Spacing.Item1 - Spacing.Item2, Height);
        }

        public IContainer GetScrollContainer(int left, int top)
        {
            var l = ScrollPosition == Position.Left ? (left + 1 + Spacing.Item1) : (left + Width - 1 - Spacing.Item2);
            return new StaticContainer(l, top - Expanding.Item1, 1, Height + Expanding.Item1 + Expanding.Item2);
        }

        public override void Resize()
        {
            var (left, top) = GetCorner();
            Content.Container = GetContentContainer(left, top);
            if (Content is Widget widget1)
                widget1.Resize();
            Scroll.Container = GetScrollContainer(left, top);
            Scroll.Resize();
        }
    }
}
