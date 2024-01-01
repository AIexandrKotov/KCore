using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class ListBox : Widget, IControlable, IVerticalScrollable
    {
        public ListBox(
            List<ListItem> childs = null,
            Location? selectLocation = null,
            RelativeLocation? selectRelativeChildLocation = null,
            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = true, bool fillHeight = true)
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {
            Childs = childs ?? new List<ListItem>();
            SelectingLocation = selectLocation ?? Location.Left | Location.Right;
            SelectingRelativeChildLocation = selectRelativeChildLocation ?? RelativeLocation.Center;
        }

        public class ListItem : Widget
        {
            public ListItem(
                Widget child,
                int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = false, bool fillHeight = false)
                : base(left, top, container, alignment, fillWidth, fillHeight)
            {
                Child = child;
            }

            public Widget Child;
            public override int Height => Child?.Height ?? 1;

            public override void Resize()
            {
                Child.Resize();
            }

            public override (int, int) Draw(int left, int top)
            {
                Child.Draw();
                return (left, top);
            }

            public override (int, int) Clear(int left, int top)
            {
                Child.Clear();
                return (left, top);
            }
        }

        public List<ListItem> Childs;
        private ListItem[] PlacedOnScreenChilds = new ListItem[0];
        private int[] CachedHeights;
        private void UpdateCachedHeights()
        {
            CachedHeights = Childs.Count == 0 ? new int[1] { 1 } : Childs.Select(x => x.Height).ToArray();
        }

        public int UpPosition;
        public int Position;

        public event Action<ListBox> OnChanged;

        #region Controls
        private class WidgetScrollUp : WidgetRequest<ListBox>
        {
            public WidgetScrollUp(Form form, ListBox widget) : base(form, widget) { }
            public override bool Condition() => Widget.NeedScrollUp;
            public override void Cancel() => Widget.NeedScrollUp = false;
            public override void Send() => Widget.NeedScrollUp = true;
            public override void Invoke() => Widget.ScrollUp();
        }
        private class WidgetScrollDown : WidgetRequest<ListBox>
        {
            public WidgetScrollDown(Form form, ListBox widget) : base(form, widget) { }
            public override bool Condition() => Widget.NeedScrollDown;
            public override void Cancel() => Widget.NeedScrollDown = false;
            public override void Send() => Widget.NeedScrollDown = true;
            public override void Invoke() => Widget.ScrollDown();
        }
        private bool SelectingRedrawTrigger;
        private class WidgetSelectingRedraw : WidgetRequest<ListBox>
        {
            public WidgetSelectingRedraw(Form form, ListBox widget) : base(form, widget)
            {
            }

            public override bool AllRedraw => true;
            public override void Cancel() => Widget.SelectingRedrawTrigger = false;
            public override void Send() => Widget.SelectingRedrawTrigger = true;
            public override bool Condition() => Widget.SelectingRedrawTrigger;
            public override void Invoke() => Widget.SelectingRedraw();
        }
        public override IList<Request> GetBinds(Form form)
        {
            var ret = new Request[3];
            ret[0] = new WidgetScrollUp(form, this);
            ret[1] = new WidgetScrollDown(form, this);
            ret[2] = new WidgetSelectingRedraw(form, this);
            return ret;
        }
        private bool NeedScrollUp { get; set; }
        private bool NeedScrollDown { get; set; }
        private bool Overheight => PlacedOnScreenChilds.Length < Childs.Count;
        private int MaxUpPosition()
        {
            var h = Height;
            var i = Childs.Count - 1;
            if (i == -1) return 0;
            while (h >= 0)
            {
                h -= Childs[i--].Child.Height;
            }
            return i + 1;
        }
        public void ScrollUp()
        {
            NeedScrollUp = false;
            OnChanged?.Invoke(this);
            if (Scroll != null) Scroll.Redraw();
            if (Overheight)
            {
                var up = UpPosition;
                Position--;
                if (Position < UpPosition)
                {
                    UpPosition--;
                }
                if (Position < 0)
                {
                    UpPosition = MaxUpPosition() + 1;
                    Position = Childs.Count - 1;
                }
                if (up != UpPosition)
                {
                    Resize();
                    Redraw();
                }
                OnChanged?.Invoke(this);
            }
            else
            {
                Position--;
                if (Position < 0) Position = Childs.Count - 1;
                OnChanged?.Invoke(this);
            }
            SelectingRedrawTrigger = true;
        }
        public void ScrollDown()
        {
            NeedScrollDown = false;
            OnChanged?.Invoke(this);
            if (Scroll != null) Scroll.Redraw();
            if (Overheight)
            {
                var up = UpPosition;
                Position++;
                if (Position >= UpPosition + PlacedOnScreenChilds.Length)
                {
                    if (Position < Childs.Count)
                    {
                        var height = Childs[Position].Height;
                        var add = PlacedOnScreenChilds.TakeWhile(x =>
                        {
                            var more = height > 0;
                            height -= x.Height;
                            return more;
                        }).Count();
                        if (add == 0) add = 1;
                        UpPosition += add;
                    }
                }
                if (Position >= Childs.Count)
                {
                    UpPosition = 0;
                    Position = 0;
                }
                if (up != UpPosition)
                {
                    Resize();
                    Redraw();
                }
                OnChanged?.Invoke(this);
            }
            else
            {
                Position++;
                if (Position >= Childs.Count) Position = 0;
                OnChanged?.Invoke(this);
            }
            SelectingRedrawTrigger = true;
        }

        public void OnKeyDown(byte key)
        {
            if (key == Key.W || key == Key.UpArrow) NeedScrollUp = true;
            if (key == Key.S || key == Key.DownArrow) NeedScrollDown = true;
        }

        public void OnKeyUp(byte key)
        {

        }
        #endregion

        [Flags]
        public enum Location
        {
            Left = 0b01,
            Right = 0b10,
        }
        public enum RelativeLocation
        {
            Up,
            Center,
            Down,
        }
        public (int, int) SelectingPadding { get; set; }
        public RelativeLocation SelectingRelativeChildLocation { get; set; }
        public Location SelectingLocation { get; set; }
        public (int, int) OverallPadding 
            => (SelectingLocation.HasFlag(Location.Left) ? SelectingPadding.Item1 + 1 : 0
            , SelectingLocation.HasFlag(Location.Right) ? SelectingPadding.Item2 + 1 : 0);
        private void SelectingRedraw()
        {
            SelectingRedrawTrigger = false;
            var left = GetLeftCornerValue();
            for (var i = 0; i < PlacedOnScreenChilds.Length; i++)
            {
                var child = PlacedOnScreenChilds[i].Child;
                var top = child.GetTopCornerValue();
                if (SelectingRelativeChildLocation == RelativeLocation.Center) top += child.Height / 2 + ((child.Height % 2 == 0) ? -1 : 0);
                else if (SelectingRelativeChildLocation == RelativeLocation.Down) top += child.Height - 1;
                if (SelectingLocation == (Location.Left | Location.Right))
                {
                    Terminal.Set(left, top);
                    Terminal.Write(UpPosition + i == Position ? "►" : " ");
                    Terminal.Set(left + Width - 1, top);
                    Terminal.Write(UpPosition + i == Position ? "◄" : " ");
                }
                else if (SelectingLocation == Location.Left)
                {
                    Terminal.Set(left, top);
                    Terminal.Write(UpPosition + i == Position ? "►" : " ");
                }
                else
                {
                    Terminal.Set(left + Width - 1, top);
                    Terminal.Write(UpPosition + i == Position ? "◄" : " ");
                }
            }
        }
        public override bool AlwaysClear => true;
        private void UpdatePlacingOnScreen()
        {
            var h = 0;
            var i = UpPosition;
            var ret = new List<ListItem>();
            while (h <= Height && i < Childs.Count)
            {
                h += Childs[i].Height;
                if (h <= Height) ret.Add(Childs[i++]);
            }
            PlacedOnScreenChilds = ret.ToArray();
            UpdateCachedHeights();
        }

        public override (int, int) Draw(int left, int top)
        {
            for (var i = 0; i < PlacedOnScreenChilds.Length; i++)
                PlacedOnScreenChilds[i].Child.Draw();
            SelectingRedraw();
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Graph.FillRectangle(Theme.Back, left, top, Width, Height);
            return (left, top);
        }

        public override void Resize()
        {
            UpdatePlacingOnScreen();
            var (left, top) = GetCorner();
            var h = 0;
            var op = OverallPadding;
            var sum = op.Item1 + op.Item2;
            for (var i = 0; i < PlacedOnScreenChilds.Length; i++)
            {
                PlacedOnScreenChilds[i].Child.Container = new StaticContainer(
                    left + op.Item1,
                    top + h,
                    Width - sum,
                    PlacedOnScreenChilds[i].Height);
                PlacedOnScreenChilds[i].Child.Resize();
                h += PlacedOnScreenChilds[i].Child.Height;
            }
        }

        public int Length => CachedHeights.Sum();

        public int CurrentIndent => CachedHeights.Take(UpPosition).Sum();

        public Widget Scroll { get; set; }
    }
}
