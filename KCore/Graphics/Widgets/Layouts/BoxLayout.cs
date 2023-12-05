using KCore.Extensions;
using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class BoxLayout : Widget, INestedWidgets
    {
        public enum BoxOrientation
        {
            Horizontal,
            Vertical,
        }

        public BoxLayout(
            BoxOrientation? orientation = null,
            List<Widget> widgets = null,
            bool fillRest = true,

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null)
            : base(left, top, container, alignment)
        {
            Widgets = widgets ?? new List<Widget>();
            Orientation = orientation ?? BoxOrientation.Horizontal;
            FillRest = fillRest;
        }

        public override int Width => Container.Width;
        public override int Height => Container.Height;

        public bool FillRest;
        public BoxOrientation Orientation;
        public List<Widget> Widgets;

        public override void Resize()
        {
            if (Widgets.Count == 0) return;
            var value = Orientation == BoxOrientation.Vertical ? Container.Height : Container.Width;
            var containers_sizes = Enumerable.Repeat(value / Widgets.Count, Widgets.Count).ToArray();

            if (FillRest)
            {
                var rest = value - containers_sizes.Sum();
                for (var i = 0; rest > 0; rest--)
                    containers_sizes[i++]++;
            }

            var containers = Orientation == BoxOrientation.Vertical
                ? containers_sizes.ConvertAll(x => new StaticContainer(0, 0, Width, x))
                : containers_sizes.ConvertAll(x => new StaticContainer(0, 0, x, Height));

            var sum = 0;
            for (var i = 1; i < containers.Length; i++)
            {
                if (Orientation == BoxOrientation.Vertical)
                {
                    sum += containers[i - 1].Height;
                    containers[i].Top = sum;
                }
                else
                {
                    sum += containers[i - 1].Width;
                    containers[i].Left = sum;
                }
            }

            for (var i = 0; i < containers.Length; i++)
            {
                containers[i].Left += (this as IContainer).Left;
                containers[i].Top += (this as IContainer).Top;
                Widgets[i].Container = containers[i];
                Widgets[i].Resize();
            }
        }

        public override (int, int) Draw(int left, int top)
        {
            for (var i = 0; i < Widgets.Count; i++)
                Widgets[i].Draw();
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            for (var i = 0; i < Widgets.Count; i++)
                Widgets[i].Clear();
            return (left, top);
        }

        public void AddWidget(Widget widget)
        {
            Widgets.Add(widget);
            Resize();
        }

        public void RemoveWidget(Widget widget)
        {
            Widgets.Remove(widget);
            Resize();
        }

        public void ClearWidgets(Predicate<Widget> predicate)
        {
            Widgets.RemoveAll(predicate);
        }

        public void ClearWidgets()
        {
            Widgets.Clear();
        }
    }
}
