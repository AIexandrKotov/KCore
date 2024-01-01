using KCore.Graphics.Special;
using KCore.Graphics.Widgets;
using KCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KCore.Graphics;
using KCore.Extensions.InsteadSLThree;

namespace Runner
{
    public class Form4 : Form
    {
        public ListBox List;
        public Trigger CloseTrigger;

        public Form4()
        {
            var r = new Random();
            var wvs = new WithVerticalScroll(List = new ListBox(), scrollPosition: WithVerticalScroll.Position.Right, expanding: (0, -1));
            List.Childs = Enumerable.Range(0, 30)
                .Select(x => {
                    var h = r.Next(3, 6);
                    return new ListBox.ListItem(new Window(
                        fillHeight: false,
                        height: h,
                        fillWidth: true,
                        hasBorder: false,
                        child: new Window(
                            title: $"item {x,-2} height {h}",
                            titleAlignment: TextAlignment.Center,
                            child: new TextWidget(Enumerable.Repeat("some text", h-2).JoinIntoString("\n")),
                            fillWidth: true,
                            fillHeight: true,
                            borderColor: (ConsoleColor)r.Next(8, 15),
                            alignment: Alignment.CenterWidth | Alignment.UpHeight)));
                    })
                .ToList();
            Root.AddWidget(List);
            Root.AddWidget(wvs.Scroll);
            Root.AddWidget(wvs);

            ActiveWidget = List;

            Bind(CloseTrigger = new Trigger(this, form => (form as Form).Close()));
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.Tab || key == Key.Escape) CloseTrigger.Pull();
        }
    }
}
