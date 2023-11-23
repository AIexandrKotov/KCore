namespace KCore.Graphics.Widgets
{
    public interface IRedrawable
    {
        bool NeedRedraw { get; set; }
        void Redraw();
    }
}
