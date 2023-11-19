using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public interface IControlable
    {
        void OnKeyDown(byte key);
        void OnKeyUp(byte key);
    }
}
