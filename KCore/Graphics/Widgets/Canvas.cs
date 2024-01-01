using KCore.Graphics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Widgets
{
    public abstract class Canvas : Widget
    {
        public List<ComplexPixel[,]> NextHistory { get; set; }
        public List<ComplexPixel[,]> PrevHistory { get; set; }
        public int HistoryDepth { get; set; } = 256;
        public bool SaveHistory { get; set; } = false;

        public ComplexPixel[,] AppliedCanvasMatrix { get; set; }
        public ComplexPixel[,] CanvasMatrix { get; set; }
        public Canvas()
        {

        }

        public abstract void Prev();
        public abstract void Next();
        public abstract void LockRedrawing();
        public abstract void UnlockRerawing();
        public abstract void Set(int left, int top);
        public abstract void Line(int left, int top, int right, int bottom);
        public abstract void Text(string text);
        public abstract void Char(char @char);
    }
}
