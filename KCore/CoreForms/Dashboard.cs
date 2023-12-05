using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.CoreForms
{
    internal class Dashboard : Form
    {
        public TriggerRedrawer Downline;

        public Dashboard()
        {
            Bind(Downline = new TriggerRedrawer(this, form =>
            {
                Terminal.Set(1, Terminal.FixedWindowHeight - 1);
                ($"KCore {KCoreVersion.Version} %=>DarkCyan%{Terminal.FixedWindowWidth}x{Terminal.FixedWindowHeight}%=>reset% "
                + $"%=>Red%{Terminal.UpdatesPerSecond} UPS%=>reset%").PrintSuperText();
            }));
        }

        protected override bool IsRecursiveForm() => true;

        protected override void OnAllRedraw()
        {

            var left = Terminal.FixedWindowWidth - 21;
            Terminal.Set(left, 1);
            Terminal.Write("EXTRA KEYS");
            Terminal.Set(left, 2);
            Terminal.Write("F2 - External resize");
            Terminal.Set(left, 3);
            Terminal.Write("F3/F4 - Height -/+");
            Terminal.Set(left, 4);
            Terminal.Write("F5/F6 - Width -/+");
            Terminal.Set(left, 5);
            Terminal.Write("F7 - Show UPS");
            Terminal.Set(left, 6);
            Terminal.Write("F8 - UPS unlim");
            Terminal.Set(left, 7);
            Terminal.Write("F9 - Make error");
            Terminal.Set(left, 8);
            Terminal.Write("F10 - All redraw");
            Terminal.Set(left, 9);
            Terminal.Write("Pause - Pause KCore");

            Terminal.Set(Terminal.FixedWindowWidth - 11, Terminal.FixedWindowHeight - 1);
            Terminal.Write("Close: F12");

            Terminal.Set(1, 1);
            $"Current: %=>Red%{Reference?.GetType().FullName ?? "null"}%=>reset%".PrintSuperText();
            if (Reference != null)
            {
                var req = Reference.Requests;
                Terminal.Set(4, 2);
                $"Started from: %=>Red%{Reference?.Reference.GetType().FullName ?? "null"}%=>reset%".PrintSuperText();
                Terminal.Set(4, 3);
                $"FormTimer: %=>DarkGreen%{Reference.FormTimer.TotalSeconds:0.##}%=>reset% sec".PadRight(50).PrintSuperText();
                Terminal.Set(4, 4);
                $"Delay: %=>DarkGreen%{Reference.RealUPS.TotalMilliseconds:0.##}%=>reset% ms".PadRight(50).PrintSuperText();
                Terminal.Set(4, 5);
                $"Total redrawers/requests: %=>DarkGreen%{req.Count(x => x.AllRedraw)}%=>reset%/%=>DarkGreen%{req.Length}%=>reset%".PrintSuperText();
                Terminal.Set(4, 6);
                $"Root widgets: %=>DarkGreen%{Reference.Root.Childs.Count}%=>reset%".PrintSuperText();
                var max = Math.Min(Terminal.FixedWindowHeight - 8, Reference.Root.Childs.Count);
                for (var i = 0; i < max; i++)
                {
                    Terminal.Set(7, 7 + i);
                    var type = Reference.Root.Childs[i].GetType();
                    var assembly = type.Assembly.GetName().Name;
                    var name = type.Name;
                    $"%=>Blue%[{assembly}]%=>reset% {name}".PrintSuperText();
                }
            }
        }

        protected override void OnKeyDown(byte key)
        {
            if (key == Key.F12) Close();
            if (key == Key.OemPlus && Terminal.UpdatesPerSecond > 0)
            {
                Terminal.UpdatesPerSecond += 10;
                Downline.Do();
            }
            if (key == Key.OemMinus && Terminal.UpdatesPerSecond > 10)
            {
                Terminal.UpdatesPerSecond -= 10;
                Downline.Do();
            }
        }
    }
}
