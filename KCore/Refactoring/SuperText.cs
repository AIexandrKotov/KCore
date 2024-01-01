using KCore.Graphics.Widgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Refactoring
{
    public class SuperText : ICloneable
    {
        public static readonly Func<(ConsoleColor, ConsoleColor)> DefaultResetColorRedirect = () => (Theme.Fore, Theme.Back);
        public static readonly Func<(ConsoleColor, ConsoleColor)> ResetColorDisableText = () => (Theme.Disabled, Theme.Back);

        public (int, int) ContainerCorner { get; set; }

        public ICarriage Carriage { get; set; }
        public Form Form { get; set; }
        public (ConsoleColor, ConsoleColor) CarriageColors { get; set; }
        public TimeSpan LastCarriage { get; set; }
        public Node CarriageNext { get; set; }
        public Node[] Nodes { get; set; }

        public abstract class Node : ICloneable
        {
            public SuperText Reference { get; set; }
            public (int, int) RelativeCorner { get; set; }

            public void Invoke() => InvokeInstantly();
            public bool Carriage(TimeSpan newTime)
            {
                Reference.LastCarriage = newTime;
                return InvokeCarriage();
            }

            public abstract void InvokeInstantly();
            public abstract bool InvokeCarriage();
            public abstract object Clone();
        }

        public interface ICarriage
        {
            TimeSpan CurrentCarriagePause { get; set; }
        }

        public object Clone()
        {
            var ret = MemberwiseClone();
            return ret;
        }

        public void Bind(Form form)
        {
            //form.Bind(() => CarriageNext != null && Form.FormTimer >= (LastCarriage + Carriage.CurrentCarriagePause), () => CarriageNext.Carriage(form.FormTimer));
        }
    }
}
