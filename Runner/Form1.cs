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
        public Redrawable Row { get; set; }
        public TextRow TextRow { get; set; }
        public TextInput TextInput { get; set; }

        public Form1()
        {
            TextRow = new TextRow(fillWidth: true, text: "%=>Red%Super%=>reset%row");

            RootWidget = Row = new Redrawable(TextRow);
            ActiveWidget = TextInput = new TextInput(this);

            AddRedrawer(Row);

            TextInput.OnAnyInput = () =>
            {
                TextRow.Text = TextInput.String;
                Row.NeedRedraw = true;
            };
            TextInput.Activate(TextRow.Text);
        }
    }
}
