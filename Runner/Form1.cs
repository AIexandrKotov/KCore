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
        public TextRow TextRow;
        public TextInput TextInput;
        public Trigger CloseTrigger;

        public Form1()
        {
            Root.AddWidget(TextRow = new TextRow(fillWidth: true, text: "%=>Red%Super%=>reset%row"));

            ActiveWidget = TextInput = new TextInput(this);
            TextInput.OnAnyInput = input =>
            {
                TextRow.Text = input.String;
                TextRow.Redraw();
            };
            TextInput.Activate(TextRow.Text);

            Bind(CloseTrigger = new Trigger(this, form => (form as Form).Close()));
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.Tab || key == Key.Escape) CloseTrigger.Pull();
        }
    }
}
