using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public interface INestedWidgets
    {
        void AddWidget(BoundedObject o);
        void RemoveWidget(BoundedObject o);
        void ClearWidgets();
    }
}
