using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Forms
{
    public class Form : BaseForm
    {
        public IControlable ActiveWidget;

        protected override void OnKeyDown(byte key)
        {
            ActiveWidget?.OnKeyDown(key);
        }

        protected override void OnKeyUp(byte key)
        {
            ActiveWidget?.OnKeyUp(key);
        }
    }
}
