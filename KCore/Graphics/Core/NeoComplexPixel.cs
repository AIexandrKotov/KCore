using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Core
{
    public struct NeoComplexPixel
    {
        public uint PositionLeft;
        public uint PositionTop;
        public char Character;

        public byte ForeThemeColorId;
        public byte BackThemeColorId;
        public ConsoleColor ForegroundColor;
        public ConsoleColor BackgroundColor;

        public bool IsThemeColor;

        public ushort GetTop()
        {
            return (ushort)(((IsThemeColor ? 1 : 0) << 15) + PositionTop);
        }

        public static (bool, ushort) FromTop(ushort top)
        {
            var ret = (false, (ushort)0);
            ret.Item1 = top >> 15 == 1;
            ret.Item2 = (ushort)(top & (ushort.MaxValue >> 1));
            return ret;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((ushort)PositionLeft);
            bw.Write(GetTop());
            bw.Write((short)Character);
            if (IsThemeColor)
            {
                bw.Write(ForeThemeColorId);
                bw.Write(BackThemeColorId);
            }
            else
            {
                bw.Write((byte)ForegroundColor);
                bw.Write((byte)BackgroundColor);
            }
        }
        
        public static NeoComplexPixel Read(BinaryReader br)
        {
            var left = br.ReadUInt16();
            var (is_theme, top) = FromTop(br.ReadUInt16());
            return new NeoComplexPixel()
            {
                PositionLeft = left,
                PositionTop = top,
                IsThemeColor = is_theme,
            };
        }
    }
}
