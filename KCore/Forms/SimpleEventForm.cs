using KCore.CoreForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Forms
{
    public class SimpleEventForm : BaseForm
    {
        public event Action AllRedraw;
        public event Action Closing;
        public event Action<byte> KeyDown;
        public event Action<byte> KeyUp;
        public event Action Opening;
        public event Action Resize;
        public event Action Returned;
        public event Action Showing;
        public event Action TopAllRedraw;

        protected override void OnAllRedraw() => AllRedraw?.Invoke();
        protected override void OnClosing() => Closing?.Invoke();
        protected override void OnKeyDown(byte key) => KeyDown?.Invoke(key);
        protected override void OnKeyUp(byte key) => KeyUp?.Invoke(key);
        protected override void OnOpening() => Opening?.Invoke();
        protected override void OnResize() => Resize?.Invoke();
        protected override void OnReturned() => Returned?.Invoke();
        protected override void OnShowing() => Showing?.Invoke();
        protected override void OnTopAllRedraw() => TopAllRedraw?.Invoke();
    }
}
