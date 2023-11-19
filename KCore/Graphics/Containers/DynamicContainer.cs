using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Containers
{
    public class DynamicContainer : IContainer
    {
        public int Left { get => GetLeft(); }
        public int Top { get => GetTop(); }
        public int Width { get => GetWidth(); }
        public int Height { get => GetHeight(); }
        public Func<int> GetLeft { get; set; } = () => 0;
        public Func<int> GetTop { get; set; } = () => 0;
        public Func<int> GetWidth { get; set; } = () => Terminal.FixedWindowWidth;
        public Func<int> GetHeight { get; set; } = () => Terminal.FixedWindowHeight;

        public DynamicContainer()
        {

        }
    }
}
