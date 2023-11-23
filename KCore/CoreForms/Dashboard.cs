using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.CoreForms
{
    internal class Dashboard : Form
    {
        protected override bool IsRecursiveForm() => true;

        protected override void OnAllRedraw()
        {
            Terminal.Set(1, 1);
            Terminal.Write($"KCore {KCoreVersion.Version} dashboard");
        }

        protected override void OnKeyDown(byte key)
        {
            if (key == Key.F12) Close();
        }
    }
}
