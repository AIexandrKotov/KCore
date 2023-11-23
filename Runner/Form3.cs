using KCore;
using KCore.Extensions;
using KCore.Graphics.Widgets;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Form3 : Form
    {
        public Redrawable WindowRedrawable;
        public Window Window;

        public Form3()
        {
            var text = new TextWidget("%=>Red%Строка 1%=>reset%\n%=>Blue%СТРОКА%=>reset% %=>DarkCyan%2%=>reset%!\n1\n2\n3\n4\n5\n6\n7", alignment: Alignment.RightWidth | Alignment.CenterHeight);
            Window = new Window(width: 40, height: 12, padding: (2, 2, 2, 2), alignment: Alignment.CenterWidth | Alignment.CenterHeight,
                title: "%=>Red%тестируем %=>Blue%текст%=>reset%", @internal: text);
            RootWidget = WindowRedrawable = new Redrawable(Window);
            AddRedrawer(WindowRedrawable);
        }
    }
}
