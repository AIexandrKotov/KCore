using KCore.Extensions;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.CoreForms
{
    public class ResizeViewer : BaseForm
    {
        public static TimeSpan ResizePause = new TimeSpan(1 * TimeSpan.TicksPerSecond);
        public static bool ResizeStarted;
        public static string YouAreResizingTheConsoleWindow = "You are resizing the console window";
        public static string GoingBackVia = "Going back via {0:0.0} secs";

        public ResizeViewer()
        {
            LastResize = FormTimer;
            Add(() => (FormTimer - LastResize) > ResizePause, Close);
            AddRedrawer(() => (FormTimer - LastSecsRedrawed).TotalMilliseconds > 5, RedrawSecs);
        }

        public TimeSpan LastResize;
        public TimeSpan LastSecsRedrawed;

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
            Terminal.Set(0, Terminal.FixedWindowHeight / 2 - 2);
            Terminal.Write($"{Terminal.FixedWindowWidth}x{Terminal.FixedWindowHeight}".PadCenter(Terminal.FixedWindowWidth));
            Terminal.Set(0, Terminal.FixedWindowHeight / 2 - 1);
            Terminal.Write(YouAreResizingTheConsoleWindow.PadCenter(Terminal.FixedWindowWidth));
        }

        public void RedrawSecs()
        {
            LastSecsRedrawed = FormTimer;
            Terminal.Set(0, Terminal.FixedWindowHeight / 2);
            Terminal.Write(string.Format(
                GoingBackVia, 
                (LastResize - FormTimer + ResizePause)
                    .TotalSeconds.Round(1)).PadCenter(Terminal.FixedWindowWidth));
        }
    }
}
