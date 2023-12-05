using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public interface INestedWidgets
    {
        void AddWidget(Widget widget);
        void RemoveWidget(Widget widget);
        void ClearWidgets(Predicate<Widget> predicate);
        void ClearWidgets();
    }
}
