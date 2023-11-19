using System;

namespace KCore.Graphics
{
    [Flags]
    public enum Alignment
    {
        LeftWidth = 0b00001,
        CenterWidth = 0b00010,
        RightWidth = 0b00011,

        UpHeight = 0b00100,
        CenterHeight = 0b01000,
        DownHeight = 0b01100,
    }
}
