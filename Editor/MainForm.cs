using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    public partial class MainForm : Form
    {
        public bool ClickRequest, MoveRequest;

        public int GetTopWithout()
        {
            return Program.ConsoleRect.Bottom - KCore.Terminal.FixedWindowHeight * Program.PixelSize.Item2;
        }
        public bool In(Point pos, int top_without)
        {
            return pos.X >= Program.ConsoleRect.Left && pos.X <= Program.ConsoleRect.Right && pos.Y >= top_without && pos.Y <= Program.ConsoleRect.Bottom;
        }
        public (int, int) GetConsolePosition(Point pos, int top_without)
        {
            var left = (pos.X - Program.ConsoleRect.Left - Program.Offset.Item1) / Program.PixelSize.Item1;
            var top = (pos.Y - top_without - Program.Offset.Item2) / Program.PixelSize.Item2;
            return (left, top);
        }

        public void ConsoleMouseMoveHandler()
        {
            MoveRequest = false;
            var pos = Cursor.Position;
            var top_without = GetTopWithout();
            if (In(pos, top_without))
            {
                var (left, top) = GetConsolePosition(pos, top_without);
                CurrentPositionLabel.Text = $"{left,-3} {top}";
            }
        }
        public void ConsoleMouseClickHandler()
        {
            ClickRequest = false;
            var pos = Cursor.Position;
            var top_without = GetTopWithout();

            if (!In(pos, top_without))
            {
                return;
            }
            var (left, top) = GetConsolePosition(pos, top_without);
            CurrentPositionLabel.Text = $"{left,-3} {top}";
            new SimpleRequest(() =>
            {
                KCore.Terminal.Back = KCore.Theme.Fore;
                KCore.Terminal.Set(left, top);
                KCore.Terminal.Write('-');
            }).Send();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ClickRequest) ConsoleMouseClickHandler();
            if (MoveRequest) ConsoleMouseMoveHandler();
        }

        public MainForm()
        {
            InitializeComponent();
        }
    }
}
