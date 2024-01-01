using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class ConsoleRequestPack
    {
        public abstract void Invoke();
        public void Send()
        {
            ConsoleControl.MainFormThreadRequest = this;
            ConsoleControl.RequestSended = true;
        }
    }

    public class SimpleRequest : ConsoleRequestPack
    {
        public SimpleRequest(Action action) => Action = action;
        public Action Action { get; set; }
        public override void Invoke()
        {
            Action.Invoke();
        }
    }
}
