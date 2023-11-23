using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    /// <summary>
    /// Оборачивает виджет в объект-обёртку, реализуюзую интерфейс IRedrawable
    /// Вы можете легко добавлять Redrawable-объекты в форму через AddRedraw
    /// </summary>
    public class Redrawable : BoundedObject, IRedrawable, IControlable, IWidget
    {
        public bool NeedRedraw { get; set; }
        public BoundedObject Internal;

        public Redrawable(BoundedObject boundedObject) : base()
        {
            Internal = boundedObject;
        }

        public override bool FillWidth { get => Internal.FillWidth; set => Internal.FillWidth = value; }
        public override bool FillHeight { get => Internal.FillHeight; set => Internal.FillHeight = value; }
        public override int Left { get => Internal.Left; set => Internal.Left = value; }
        public override int Top { get => Internal.Top; set => Internal.Top = value; }
        public override int ContentWidth { get => Internal.ContentWidth; set => Internal.ContentWidth = value; }
        public override int ContentHeight { get => Internal.ContentHeight; set => Internal.ContentHeight = value; }
        public override IContainer Container { get => Internal.Container; set => Internal.Container = value; }
        public override Alignment Alignment { get => Internal.Alignment; set => Internal.Alignment = value; }
        public override int Height => Internal.Height;
        public override int Width => Internal.Width;
        public override (int, int) Draw(int left, int top)
        {
            return Internal.Draw(left, top);
        }
        public override (int, int) Clear(int left, int top)
        {
            return Internal.Clear(left, top);
        }

        void IRedrawable.Redraw()
        {
            base.Draw();
            NeedRedraw = false;
        }
        public void OnKeyDown(byte key)
        {
            if (Internal is IControlable controlable)
                controlable.OnKeyDown(key);
        }
        public void OnKeyUp(byte key)
        {
            if (Internal is IControlable controlable)
                controlable.OnKeyUp(key);
        }
        public void UpdateSizes()
        {
            if (Internal is IWidget widget)
                widget.UpdateSizes();
        }
    }
}
