using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class PaddingContainer : Widget
    {
        public PaddingContainer(
            BoundedObject @internal,
            (int, int, int, int)? padding = null,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null)
            : base(left, top, container, alignment)
        {
            Internal = @internal;
            Padding = padding ?? (1, 1, 1, 1);
            Resize();
        }

        public BoundedObject Internal;
        public (int, int, int, int) Padding;

        public override (int, int) Draw(int left, int top)
        {
            return Internal.Draw();
        }

        public override (int, int) Clear(int left, int top)
        {
            return Internal.Clear();
        }

        public IContainer GetContainer(int left, int top)
        {
            return new StaticContainer(
                left + Padding.Item1,
                top + Padding.Item2,
                Width - Padding.Item1 - Padding.Item3,
                Height - Padding.Item2 - Padding.Item4);
        }

        public override void Resize()
        {
            var (left, top) = GetCorner();
            Internal.Container = GetContainer(left, top);
            if (Internal is Widget widget)
                widget.Resize();
        }
    }
}
