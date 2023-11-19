using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    public interface IContainer
    {
        int Left { get; }
        int Top { get; }
        int Width { get; }
        int Height { get; }
    }
}
