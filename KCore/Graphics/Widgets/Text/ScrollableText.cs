using KCore.Extensions;
using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class ScrollableText : Widget, IControlable, IVerticalScrollable
    {
        public ScrollableText(
            string text = "",
            TextAlignment textAlignment = TextAlignment.Center,
            bool looped = false,
            Widget scroll = null,
            bool enabled = true,
            IContainer container = null)
            : base(0, 0, container, null, true, true)
        {
            //Alignment = alignment ?? Alignment.CenterWidth | Alignment.CenterHeight;
            Text = text;
            TextAlignment = textAlignment;
            LoopedScroll = looped;
            Scroll = scroll;
            Enabled = enabled;
            Resize();
        }
        public override int ContentHeight { get => height; set => throw new NotImplementedException("Нельзя изменить эту величину"); }
        public string Text;
        public TextAlignment TextAlignment;

        #region Drawing
        private int height;
        private SuperText[] superText;
        private SuperText[] defaultSuperText;
        private int CurrentStart = 0;

        public SuperText[] UpdateCachedCorners()
        {
            var ret = new SuperText[defaultSuperText.Length];
            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = defaultSuperText[i].Clone() as SuperText;
                var cc = ret[i].CachedCorner;
                cc.Item2 -= CurrentStart;
                ret[i].CachedCorner = cc;
            }
            return ret;
        }

        public IContainer GetTextContainer(int left, int top)
        {
            var h = Math.Min(Container.Height, height); 
            return new StaticContainer(left, top, Width, h);
        }

        public IContainer GetTextSuperContainer(int left, int top)
        {
            return new StaticContainer(left, top, Width, Text.Length / Width + Text.Count(x => x == '\n'));
        }

        public override bool AlwaysClear => true;

        public override (int, int) Draw(int left, int top)
        {
            Terminal.ResetColor();
            superText.PrintSuperText(GetTextContainer(left, top));
            Terminal.ResetColor();
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Graph.FillRectangle(Theme.Back, left, top, Width, Height);
            return (left, top);
        }
        #endregion

        public bool LoopedScroll = false;
        public int MaxStart => Math.Max(0, height - Container.Height - 1);


        #region Controls
        private class WidgetScrollUp : WidgetRequest<ScrollableText>
        {
            public WidgetScrollUp(Form form, ScrollableText widget) : base(form, widget) { }

            public override void Cancel() => Widget.NeedScrollUp = false;

            public override bool Condition() => Widget.Visible && Widget.NeedScrollUp;

            public override void Invoke()
            {
                Widget.ScrollUp();
            }
        }
        private class WidgetScrollDown : WidgetRequest<ScrollableText>
        {
            public WidgetScrollDown(Form form, ScrollableText widget) : base(form, widget) { }

            public override void Cancel() => Widget.NeedScrollDown = false;

            public override bool Condition() => Widget.Visible && Widget.NeedScrollDown;

            public override void Invoke()
            {
                Widget.ScrollDown();
            }
        }
        private class WidgetAutoscroll : WidgetRequest<ScrollableText>
        {
            public WidgetAutoscroll(Form form, ScrollableText widget) : base(form, widget) { }

            public override void Cancel() => Widget.NeedScrollDown = false;

            public override bool Condition() => Widget.Visible && Widget.Autoscroll && BindedTo.FormTimer >= (Widget.lastAutoscroll + Widget.AutoscrollInterval);

            public override void Invoke()
            {
                Widget.NeedScrollUp = true;
                Widget.lastAutoscroll = BindedTo.FormTimer;
            }
        }

        private bool NeedScrollUp { get; set; }
        private bool NeedScrollDown { get; set; }
        public bool Autoscroll { get; set; }
        public TimeSpan AutoscrollInterval { get; set; } = new TimeSpan(0, 0, 0, 0, 1000);
        private TimeSpan lastAutoscroll;
        
        public void Autoscrolling(TimeSpan n)
        {
            ScrollUp();
            lastAutoscroll = n;
        }

        public void ScrollUp()
        {
            NeedScrollUp = false;
            if (Container.Height >= height) return;
            if (!LoopedScroll && CurrentStart > MaxStart) return;
            if (Scroll != null) Scroll.Redraw();
            CurrentStart++;
            if (LoopedScroll && CurrentStart >= MaxStart) CurrentStart = 0;
            superText = UpdateCachedCorners().ToArray();
            this.Redraw();
        }

        public void ScrollDown()
        {
            NeedScrollDown = false;
            if (Container.Height >= height) return;
            if (!LoopedScroll && CurrentStart <= 0) return;
            if (Scroll != null) Scroll.Redraw();
            CurrentStart--;
            if (LoopedScroll && CurrentStart < 0) CurrentStart = MaxStart - 1;
            superText = UpdateCachedCorners().ToArray();
            this.Redraw();
        }

        public void OnKeyDown(byte key)
        {
            if (key == Key.W || key == Key.UpArrow) NeedScrollUp = true;
            if (key == Key.S || key == Key.DownArrow) NeedScrollDown = true;
        }

        public void OnKeyUp(byte key)
        {

        }

        public override IList<Request> GetBinds(Form form)
        {
            var ret = new Request[3];
            ret[0] = new WidgetScrollUp(form, this);
            ret[1] = new WidgetScrollDown(form, this);
            ret[2] = new WidgetAutoscroll(form, this);
            return ret;
        }
        #endregion

        public override void Resize()
        {
            var (left, top) = GetCorner();
            defaultSuperText = Text.GetSuperText(GetTextSuperContainer(left, top), null, TextAlignment).ToArray();
            superText = UpdateCachedCorners();
            var first = defaultSuperText.OfType<SuperText.SuperTextOut>().FirstOrDefault();
            var last = defaultSuperText.OfType<SuperText.SuperTextOut>().LastOrDefault();
            if (first == null || last == null || first == last) height = 1;
            else height = (last.CachedCorner.Item2 + last.Position.Item2) - (first.CachedCorner.Item2 + first.Position.Item2) + 1;
            CurrentStart = 0;
        }

        public int Length => height;
        public int CurrentIndent => CurrentStart;
        int IVerticalScrollable.Height => Container.Height;

        public Widget Scroll { get; set; }
    }
}
