using KCore;
using KCore.Forms;
using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Form2 : Form
    {
        public BoxLayout box;

        public Form2()
        {
            RootWidget = box = new BoxLayout();
            box.Orientation = BoxLayout.BoxOrientation.Vertical;
            var box1 = new BoxLayout();
            var box2 = new BoxLayout();
            var box3 = new BoxLayout();
            box1.Orientation = box2.Orientation = box3.Orientation = BoxLayout.BoxOrientation.Vertical;
            box1.AddWidget(new TextRow(text: "row 1.1", foreground: ConsoleColor.White));
            box1.AddWidget(new TextRow(text: "row 1.2", foreground: ConsoleColor.Blue));
            box1.AddWidget(new TextRow(text: "row 1.3", foreground: ConsoleColor.Red));

            box2.Orientation = BoxLayout.BoxOrientation.Vertical;
            box2.FillRest = false;
            box2.AddWidget(new TextRow(text: "row 2.1", foreground: ConsoleColor.Magenta));
            box2.AddWidget(new TextRow(text: "row 2.2", foreground: ConsoleColor.DarkGreen));
            box2.AddWidget(new TextRow(text: "row 2.3", foreground: ConsoleColor.DarkBlue));
            box2.AddWidget(new TextRow(text: "row 2.4", foreground: ConsoleColor.Black));

            box3.AddWidget(new TextRow(text: "row 3.1", foreground: ConsoleColor.DarkYellow));
            box3.AddWidget(new TextRow(text: "row 3.2", foreground: ConsoleColor.DarkCyan));
            box.AddWidget(box1);
            box.AddWidget(new Window(fillWidth: true, fillHeight: true, @internal: box2, borderColor: ConsoleColor.Red));
            box.AddWidget(box3);
        }

        protected override void OnAllRedraw()
        {
            box.Draw();
        }
    }
}
