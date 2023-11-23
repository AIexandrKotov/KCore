using KCore.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public class TextWidget : BoundedObject, IWidget
    {
        public TextWidget(
            string text = "",

            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = true, bool fillHeight = true)
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {
            Alignment = alignment ?? Alignment.CenterWidth | Alignment.CenterHeight;
            Text = text;
            TextAlignment = GetTextAlignment(Alignment);
            UpdateSizes();
        }

        private int height;
        private IEnumerable<SuperText> superText;
        public override int ContentHeight { get => height; set => throw new NotImplementedException("Нельзя изменить эту величину"); }
        public string Text;
        public TextAlignment TextAlignment;

        public static TextAlignment GetTextAlignment(Alignment alignment)
        {
            var al = (Alignment)((int)alignment & 0b0011);
            switch (al)
            {
                case Alignment.LeftWidth: return TextAlignment.Left;
                case Alignment.CenterWidth: return TextAlignment.Center;
                case Alignment.RightWidth: return TextAlignment.Right;
            }
            throw new ArgumentException(nameof(alignment));
        }

        public IContainer GetTextContainer(int left, int top)
        {
            var h = Math.Min(Container.Height, height);
            var diff = Math.Abs(Container.Height - h);
            var al = (Alignment)((int)Alignment & 0b1100);
            switch (al)
            {
                case Alignment.UpHeight: return new StaticContainer(left, top, Width, h);
                case Alignment.CenterHeight: return new StaticContainer(left, top + diff / 2, Width, h);
                case Alignment.DownHeight: return new StaticContainer(left, top + diff, Width, h);
            }
            throw new ArgumentException(nameof(Alignment));
        }

        public override (int, int) Draw(int left, int top)
        {
            superText.PrintSuperText(GetTextContainer(left, top));
            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Graph.FillRectangle(Theme.Back, left, top, Width, Height);
            return (left, top);
        }

        public void UpdateSizes()
        {
            var superText = Text.GetSuperText(GetTextContainer(0, 0), null, TextAlignment);
            height = superText.OfType<SuperText.SuperTextNewLine>().Count();
            var (left, top) = GetCorner();
            this.superText = Text.GetSuperText(GetTextContainer(left, top), null, TextAlignment);
        }
    }
}
