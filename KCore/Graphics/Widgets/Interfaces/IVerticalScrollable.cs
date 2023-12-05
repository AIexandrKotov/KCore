namespace KCore.Graphics.Widgets
{
    public interface IVerticalScrollable
    {
        int Length { get; }
        int CurrentIndent { get; }
        int Height { get; }
        Widget Scroll { get; set; }
    }
}
