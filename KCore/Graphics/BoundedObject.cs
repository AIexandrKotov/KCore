using System;

namespace KCore.Graphics
{
    public abstract class BoundedObject : IDrawable, IContainer
    {
        private static int left;
        private static int top;

        public bool FillWidth { get; set; }
        public bool FillHeight { get; set; }
        public virtual int Left { get => FillWidth ? 0 : left; set => left = value; }
        public virtual int Top { get => FillHeight ? 0: top; set => top = value; }
        public virtual int Width { get => FillWidth ? Container.Width : ContentWidth; }
        public virtual int Height { get => FillHeight ? Container.Height : ContentHeight; }
        public virtual int ContentWidth { get; set; }
        public virtual int ContentHeight { get; set; }

        int IContainer.Left => Left + Container.Left;
        int IContainer.Top => Top + Container.Top;
        int IContainer.Width => Width;
        int IContainer.Height => Height;

        public IContainer Container { get; set; }
        public Alignment Alignment { get; set; } = LeftUpAlignment;

        protected const Alignment LeftUpAlignment = Alignment.LeftWidth | Alignment.UpHeight;

        public virtual (int, int) Draw()
        {
            var (x, y) = GetCorner();
            Draw(x, y);
            return (x, y);
        }
        public virtual (int, int) Clear()
        {
            var (x, y) = GetCorner();
            Clear(x, y);
            return (x, y);
        }

        public static bool IsWrongAlignment(Alignment alignment)
        {
            return (((int)alignment | 0b0011) == 0b0011) || (((int)alignment | 0b1100) == 0b1100);
        }

        public virtual ValueTuple<int, int> Draw(ConsoleColor background, ConsoleColor foreground)
        {
            Terminal.Back = background;
            Terminal.Back = foreground;
            var result = Draw();
            Terminal.ResetColor();
            return result;
        }

        public int GetLeftCornerValue()
        {
            switch (Alignment)
            {
                case Alignment.LeftWidth | Alignment.UpHeight: return Container.Left + Left;
                case Alignment.LeftWidth | Alignment.CenterHeight: return Container.Left + Left;
                case Alignment.LeftWidth | Alignment.DownHeight: return Container.Left + Left;

                case Alignment.RightWidth | Alignment.UpHeight: return Container.Left + Container.Width - Left - Width;
                case Alignment.RightWidth | Alignment.CenterHeight: return Container.Left + Container.Width - Left - Width;
                case Alignment.RightWidth | Alignment.DownHeight: return Container.Left + Container.Width - Left - Width;

                case Alignment.CenterWidth | Alignment.UpHeight: return Container.Left + (Container.Width - Width) / 2 + Left;
                case Alignment.CenterWidth | Alignment.CenterHeight: return Container.Left + (Container.Width - Width) / 2 + Left;
                case Alignment.CenterWidth | Alignment.DownHeight: return Container.Left + (Container.Width - Width) / 2 + Left;
            }
            throw new Exception("corner for this object not supported");
        }

        public int GetTopCornerValue()
        {
            switch (Alignment)
            {
                case Alignment.LeftWidth | Alignment.UpHeight: return Container.Top + Top;
                case Alignment.LeftWidth | Alignment.CenterHeight: return Container.Top + (Container.Height - Height) / 2 + Top;
                case Alignment.LeftWidth | Alignment.DownHeight: return Container.Top + Container.Height - Height - Top;

                case Alignment.RightWidth | Alignment.UpHeight: return Container.Top + Top;
                case Alignment.RightWidth | Alignment.CenterHeight: return Container.Top + (Container.Height - Height) / 2 + Top;
                case Alignment.RightWidth | Alignment.DownHeight: return Container.Top + Container.Height - Height - Top;

                case Alignment.CenterWidth | Alignment.UpHeight: return Container.Top + Top;
                case Alignment.CenterWidth | Alignment.CenterHeight: return Container.Top + (Container.Height - Height) / 2 + Top;
                case Alignment.CenterWidth | Alignment.DownHeight: return Container.Top + Container.Height - Height - Top;
            }
            throw new Exception("corner for this object not supported");
        }

        public (int, int) GetCorner()
        {
            switch (Alignment)
            {
                case Alignment.LeftWidth | Alignment.UpHeight: return (Container.Left + Left, Container.Top + Top);
                case Alignment.LeftWidth | Alignment.CenterHeight: return (Container.Left + Left, Container.Top + (Container.Height - Height) / 2 + Top);
                case Alignment.LeftWidth | Alignment.DownHeight: return (Container.Left + Left, Container.Top + Container.Height - Height - Top);

                case Alignment.RightWidth | Alignment.UpHeight: return (Container.Left + Container.Width - Left - Width, Container.Top + Top);
                case Alignment.RightWidth | Alignment.CenterHeight: return (Container.Left + Container.Width - Left - Width, Container.Top + (Container.Height - Height) / 2 + Top);
                case Alignment.RightWidth | Alignment.DownHeight: return (Container.Left + Container.Width - Left - Width, Container.Top + Container.Height - Height - Top);

                case Alignment.CenterWidth | Alignment.UpHeight: return (Container.Left + (Container.Width - Width) / 2 + Left, Container.Top + Top);
                case Alignment.CenterWidth | Alignment.CenterHeight: return (Container.Left + (Container.Width - Width) / 2 + Left, Container.Top + (Container.Height - Height) / 2 + Top);
                case Alignment.CenterWidth | Alignment.DownHeight: return (Container.Left + (Container.Width - Width) / 2 + Left, Container.Top + Container.Height - Height - Top);
            }
            throw new Exception("corner for this object not supported");
        }

        public void UpdateOffset(int left, int top, int height, int width)
        {
            Left = left;
            Top = top;
            ContentHeight = height;
            ContentWidth = width;
        }

        public abstract (int, int) Draw(int left, int top);
        public abstract (int, int) Clear(int left, int top);

        public BoundedObject() { }

        public BoundedObject(int left, int top, IContainer container, Alignment? alignment, bool fillWidth = false, bool fillHeight = false) 
        {
            Left = left;
            Top = top;
            Container = container ?? TerminalContainer.This;
            Alignment = alignment ?? LeftUpAlignment;
            FillWidth = fillWidth;
            FillHeight = fillHeight;
        }
    }
}
