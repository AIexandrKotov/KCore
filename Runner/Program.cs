using KCore;
using KCore.Extensions;
using KCore.Forms;
using KCore.Graphics;
using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Runner
{
    public class Choosing : Form
    {
        public ListBox List;
        public Trigger EnterTrigger;
        public static Type[] Forms;

        static Choosing()
        {
            Forms = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.BaseType == typeof(Form))
                .ToArray();
        }

        public Choosing()
        {
            List = new ListBox();
            List.Childs = Forms.ConvertAll(x => new ListBox.ListItem(new TextWidget(x.Name, fillHeight: false))).ToList();

            var wvs = new WithVerticalScroll(List, scroll: new VerticalScroll(List, pixel: ' ', scrollPixel: '▐'));
            var wnd = new Window(
                wvs,
                alignment: Alignment.CenterWidth | Alignment.CenterHeight,
                width: 40,
                height: 10,
                title: "Choose form",
                titleAlignment: TextAlignment.Center,
                fillWidth: false,
                fillHeight: false);

            Root.AddWidget(wnd);
            Root.AddWidget(wvs);
            Root.AddWidget(List);
            Root.AddWidget(wvs.Scroll);
            Bind(EnterTrigger = new Trigger(this, form => form.RealizeAnimation(Activator.CreateInstance(Forms[List.Position]) as Form)));
            wnd.Resize();

            ActiveWidget = List;
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.Enter || key == Key.E) EnterTrigger.Do();
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            var f = new SimpleEventForm();
            f.AllRedraw += () => f.StartAnimation(new Choosing());
            f.Start();
            Terminal.Abort();
        }
    }
}
