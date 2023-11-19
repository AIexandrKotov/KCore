using KCore;
using KCore.Forms;
using KCore.Graphics;
using KCore.Graphics.Special;
using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Form1 : Form
    {
        public TextRow Row { get; set; }
        public TextInput TextInput { get; set; }

        public Form1()
        {
            Row = new TextRow(fillWidth: true, text: "%=>Red%Super%=>reset%row");

            ActiveWidget = TextInput = new TextInput(this);

            TextInput.OnAnyInput = () => Row.Text = TextInput.String;
            TextInput.Activate(Row.Text);

            AddRedrawer(Redraw);
            TextInput.RedrawMethod = Redraw;
        }

        public void Redraw()
        {
            Row.Draw();
        }

        protected override void OnAllRedraw() => Redraw();
    }
}
