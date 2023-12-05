using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;

namespace KCore.Graphics
{
    public class RootWidget : Widget, INestedWidgets
    {
        public readonly Form Form;
        internal List<Widget> Childs { get; set; } = new List<Widget>();

        internal RootWidget(Form form)
            : base(0, 0, null, LeftUpAlignment, true, true)
        {
            Form = form;
        }

        public override void Resize()
        {
            var count = Childs.Count;
            for (var i = 0; i < count; i++)
                Childs[i].Resize();
        }

        public override (int, int) Draw(int left, int top)
        {
            return (left, top);
        }
        public override (int, int) Clear(int left, int top)
        {
            return (left, top);
        }

        public void AddWidget(Widget widget)
        {
            Childs.Add(widget);
            Form.Bind(widget);
        }
        public void RemoveWidget(Widget widget)
        {
            Childs.Remove(widget);
            Form.Unbind(widget);
        }
        public void ClearWidgets(Predicate<Widget> predicate)
        {
            var i = 0;
            while (i < Childs.Count)
            {
                if (predicate(Childs[i]))
                {
                    Form.Unbind(Childs[i]);
                    Childs.RemoveAt(i);
                }
                else i++;
            }
        }
        public void ClearWidgets()
        {
            for (var i = 0; i < Childs.Count; i++)
                Form.Unbind(Childs[i]);
            Childs.Clear();
        }
    }
}
