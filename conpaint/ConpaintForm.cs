using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace conpaint
{
    public partial class ConpaintForm : Form
    {
        public ConpaintForm()
        {
            InitializeComponent();
            var items = Enum.GetNames(typeof(ConsoleColor));
            comboBox1.Items.AddRange(items);
            comboBox2.Items.AddRange(items);
            comboBox1.Text = Console.ForegroundColor.ToString();
            comboBox2.Text = Console.BackgroundColor.ToString();
        }

        public void Apply(Action action)
        {
            ConpaintConsole.Actions.Enqueue(new Task(action));
            lock (ConpaintConsole.Sync) Monitor.Pulse(ConpaintConsole.Sync);
        }

        private void LeftButton_Click(object sender, EventArgs e)
        {
            Apply(() =>
            {
                if (Console.CursorLeft == -1)
                {
                    if (Console.CursorTop == 0)
                        return;
                    Console.CursorTop -= 1;
                    Console.CursorLeft = Console.BufferWidth - 1;
                }
                else Console.CursorLeft -= 1;
            });
        }

        private void RightButton_Click(object sender, EventArgs e)
        {
            Apply(() =>
            {
                if (Console.CursorLeft == Console.BufferWidth - 1)
                {
                    if (Console.CursorTop == Console.BufferHeight - 1)
                        return;
                    Console.CursorTop += 1;
                    Console.CursorLeft = 0;
                }
                else Console.CursorLeft += 1;
            });
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            Apply(() =>
            {
                if (Console.CursorTop == 0)
                    return;
                Console.CursorTop -= 1;
            });
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            Apply(() =>
            {
                if (Console.CursorTop == Console.BufferHeight - 1)
                    return;
                Console.CursorTop += 1;
            });
        }


        private void button5_Click(object sender, EventArgs e)
        {
            Apply(() =>
            {
                Console.Write(' ');
            });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), comboBox1.Text);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), comboBox2.Text);
        }
    }
}
