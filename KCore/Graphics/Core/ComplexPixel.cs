using System;
using System.IO;

namespace KCore.Graphics.Core
{
    public struct ComplexPixel : IComparable<ComplexPixel>
    {
        // Как и у Line при чтении и записи имеет размер 8 байт
        public int PositionLeft { get; set; }
        public int PositionTop { get; set; }
        public char Character { get; set; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public ComplexPixel Update(char? @char = null, ConsoleColor? fore = null, ConsoleColor? back = null)
        {
            return new ComplexPixel(PositionLeft, PositionTop, @char ?? Character, fore ?? ForegroundColor, back ?? BackgroundColor);
        }

        public ComplexPixel(int left, int top, char ch, ConsoleColor fore, ConsoleColor back)
        {
            PositionLeft = left;
            PositionTop = top;
            Character = ch;
            ForegroundColor = fore;
            BackgroundColor = back;
        }

        public ComplexPixel(long lng)
        {
            PositionLeft = (int)(lng >> 48);
            PositionTop = (int)(lng << 16 >> 48);
            Character = (char)(lng << 32 >> 48);
            ForegroundColor = (ConsoleColor)(byte)(lng << 48 >> 56);
            BackgroundColor = (ConsoleColor)(byte)(lng << 56 >> 56);
        }

        public ComplexPixel Clone()
        {
            return new ComplexPixel(PositionLeft, PositionTop, Character, ForegroundColor, BackgroundColor);
        }

        public long ToInt64()
        {
            return ((long)PositionLeft << 48) + ((long)PositionTop << 32) + ((long)Character << 16) + ((long)ForegroundColor << 8) + ((long)BackgroundColor);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((short)PositionLeft);
            bw.Write((short)PositionTop);
            bw.Write((short)Character);
            bw.Write((byte)ForegroundColor);
            bw.Write((byte)BackgroundColor);
        }

        public static ComplexPixel Read(BinaryReader br) => new ComplexPixel(br.ReadInt16(), br.ReadInt16(), (char)br.ReadInt16(), (ConsoleColor)br.ReadByte(), (ConsoleColor)br.ReadByte());

        public override string ToString()
        {
            return $"({PositionLeft}, {PositionTop}, {Character}, {ForegroundColor}, {BackgroundColor})";
        }

        public static explicit operator long(ComplexPixel complexPixel)
        {
            return complexPixel.ToInt64();
        }

        public static explicit operator ComplexPixel(long lng)
        {
            return new ComplexPixel(lng);
        }

        public bool EqualsAsAbstract(ComplexPixel cp)
        {
            return cp.Character == Character && cp.ForegroundColor == ForegroundColor && cp.BackgroundColor == BackgroundColor;
        }

        public int CompareTo(ComplexPixel other)
        {
            return ((PositionTop * 100000) + PositionLeft) - ((other.PositionTop * 100000) + other.PositionLeft);
        }
    }
}
