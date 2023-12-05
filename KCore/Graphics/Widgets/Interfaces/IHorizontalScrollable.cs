namespace KCore.Graphics.Widgets
{
    public interface IHorizontalScrollable
    {
        int Length { get; }
        int CurrentIndent { get; }
        int Width { get; }
        Widget Scroll { get; set; }
    }
}
