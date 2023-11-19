using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    public interface IDrawable
    {
        (int, int) Draw(int left, int top);
        (int, int) Clear(int left, int top);
    }
}
