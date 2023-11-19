using System.IO;

namespace KCore.Graphics.Core
{
    public struct Line
    {
        //При сохранении имеет размер в 8 байт, а не 12

        public int PositionLeft { get; set; }
        public int PositionTop { get; set; }
        public int Length { get; set; }

        public Line(int x, int y, int length)
        {
            PositionLeft = x;
            PositionTop = y;
            Length = length;
        }

        public Line(long lng)
        {
            PositionLeft = (int)(lng >> 48);
            PositionTop = (int)(lng << 16 >> 48);
            Length = (int)lng;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Length);
            bw.Write((short)PositionTop);
            bw.Write((short)PositionLeft);
        }

        public Line Clone() => new Line { PositionLeft = PositionLeft, PositionTop = PositionTop, Length = Length };

        public static Line Read(BinaryReader br) => new Line() { Length = br.ReadInt32(), PositionTop = br.ReadInt16(), PositionLeft = br.ReadInt16() };

        public long ToInt64()
        {
            return ((long)PositionLeft << 48) + ((long)PositionTop << 32) + Length;
        }

        public override string ToString()
        {
            return $"({PositionLeft}, {PositionTop}, {Length})";
        }

        public static explicit operator long(Line line)
        {
            return line.ToInt64();
        }

        public static explicit operator Line(long lng)
        {
            return new Line(lng);
        }
    }
}
