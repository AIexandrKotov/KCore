using KCore.Extensions;
using KCore.Graphics;
using KCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.CoreForms
{
    public class ResizeViewer : Form
    {
        public static TimeSpan ResizePause = new TimeSpan(1 * TimeSpan.TicksPerSecond);
        public static bool ResizeStarted;
        private static string YouAreResizingTheConsoleWindow => Localization.Current["KCore_Resizing1"];
        private static string GoingBackVia => Localization.Current["KCore_Resizing2"];

        public ResizeViewer()
        {
            AllowedDashboard = false;
            LastResize = FormTimer;
            Bind(new DelegateRequest(this, form => ((form as Form).FormTimer - LastResize) > ResizePause, form => Close()));
            Bind(new Redrawer(this, form => ((form as Form).FormTimer - LastSecsRedrawed).TotalMilliseconds > 5, form => RedrawSecs()));
        }

        public TimeSpan LastResize;
        public TimeSpan LastSecsRedrawed;

        public void RedrawSecs()
        {
            LastSecsRedrawed = FormTimer;
            Terminal.Set(10, Terminal.FixedWindowHeight / 2);
            Terminal.Write(string.Format(
                GoingBackVia,
                (LastResize - FormTimer + ResizePause)
                    .TotalSeconds.Round(1)).PadCenter(Terminal.FixedWindowWidth - 20));
        }

        protected override void OnOpening()
        {
            ResizeStarted = true;
        }

        protected override void OnClosing()
        {
            ResizeStarted = false;
        }

        protected override void OnResize()
        {
            LastResize = FormTimer;
        }

        protected override void OnAllRedraw()
        {
            Terminal.Set(10, Terminal.FixedWindowHeight / 2 - 2);
            Terminal.Write($"{Terminal.FixedWindowWidth}x{Terminal.FixedWindowHeight}".PadCenter(Terminal.FixedWindowWidth - 20));
            Terminal.Set(10, Terminal.FixedWindowHeight / 2 - 1);
            Terminal.Write(YouAreResizingTheConsoleWindow.PadCenter(Terminal.FixedWindowWidth - 20));

            Terminal.Back = Theme.Fore;
            Graph.Row((Terminal.FixedWindowWidth - 20) / 2, 0, 20);
            Graph.Row((Terminal.FixedWindowWidth - 20) / 2, Terminal.FixedWindowHeight - 1, 20);
            Graph.Column(0, (Terminal.FixedWindowHeight - 6) / 2, 6);
            Graph.Column(Terminal.FixedWindowWidth - 1, (Terminal.FixedWindowHeight - 6) / 2, 6);
            Terminal.ResetColor();
        }
    }
}
