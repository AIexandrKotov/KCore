using KCore;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Editor
{
    public class ConsoleControl : Form
    {
        private object sync = new object();
        public static ConsoleRequestPack ConsoleThreadRequest;
        public static ConsoleRequestPack MainFormThreadRequest;
        public static bool RequestSended;

        public ConsoleControl() : base()
        {
            AllowedDashboard = false;
            Bind(new DelegateRequest(this, form => true, form =>
            {
                Program.GetWindowRect(Program.ConsoleHandler, ref Program.ConsoleRect);
                Program.MainForm.MoveRequest = true;
            }));
            Bind(new DelegateRequest(this, form => RequestSended, form =>
            {
                lock (sync) ConsoleThreadRequest = MainFormThreadRequest;
                RequestSended = false;
            }));
            Bind(new DelegateRequest(this, form => 
            {
                lock (sync) return ConsoleThreadRequest != null;
            }, form =>
            {
                lock (sync)
                {
                    ConsoleThreadRequest.Invoke();
                    ConsoleThreadRequest = null;
                }
            }));
        }

        protected override void OnOpening()
        {
            while (Program.MainForm == null)
                Thread.Sleep(100);
        }

        protected override void OnKeyDown(byte key)
        {
            if (key == 1)
                if (Program.MainForm != null)
                    Program.MainForm.ClickRequest = true;
        }
    }
}
