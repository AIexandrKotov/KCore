using KCore;
using KCore.Extensions.InsteadSLThree;
using KCore.Graphics.Widgets;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Test : Form
    {
        public Trigger CloseTrigger;

        public Test()
        {
            Bind(CloseTrigger = new Trigger(this, form => (form as Form).Close()));
            KCore.Refactoring.Test.DoAll();
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.Tab || key == Key.Escape) CloseTrigger.Pull();
        }
    }
}
