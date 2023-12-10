using KCore.Extensions.InsteadSLThree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Core
{
    public class MediaComplexive
    {
        public Complexive[] List { get; set; }

        public Complexive[] OptimizedList { get; set; }

        public Complexive LastToFirst { get; set; }

        public bool Optimized => OptimizedList != null;

        public int Width { get; set; }
        public int Height { get; set; }

        public MediaComplexive()
        {

        }

        public MediaComplexive(params Complexive[] complexives)
        {
            List = complexives;
            Width = List[0].Width;
            Height = List[0].Height;
            if (!CheckOnSizes()) throw new ArgumentException();
        }

        public bool CheckOnSizes()
        {
            for (var i = 1; i < List.Length; i++)
            {
                if (List[i].Width != Width || List[i].Height != Height) return false;
            }
            return true;
        }

        public struct MediaPixel : IEquatable<MediaPixel>
        {
            public char Character { get; set; }
            public ConsoleColor ForegroundColor { get; set; }
            public ConsoleColor BackgroundColor { get; set; }

            public MediaPixel(ComplexPixel pixel)
            {
                Character = pixel.Character;
                ForegroundColor = pixel.ForegroundColor;
                BackgroundColor = pixel.BackgroundColor;
            }

            public ComplexPixel ToComplexPixel(int left, int top) => new ComplexPixel(left, top, Character, ForegroundColor, BackgroundColor);

            public bool Equals(MediaPixel other)
            {
                return Character == other.Character && ForegroundColor == other.ForegroundColor && BackgroundColor == other.BackgroundColor;
            }
        }

        public static void Fill(MediaPixel[,] render, Complexive complexive)
        {
            for (var i = 0; i < complexive.Pixels.Length; i++)
            {
                render[complexive.Pixels[i].PositionLeft, complexive.Pixels[i].PositionTop] = new MediaPixel(complexive.Pixels[i]);
            }
        }

        public static List<ComplexPixel> FoundNonEquals(MediaPixel[,] currentframe, MediaPixel[,] nextframe, ConsoleColor? background)
        {
            var ret = new List<ComplexPixel>();
            for (var i = 0; i < currentframe.GetLength(0); i++)
                for (var j = 0; j < currentframe.GetLength(1); j++)
                    if (!currentframe[i, j].Equals(nextframe[i, j]))
                    {
                        if (nextframe[i, j].Character == (char)0)
                        {
                            var stop = nextframe[i, j].ToComplexPixel(i, j);
                            stop.Character = ' ';
                            stop.BackgroundColor = background ?? Theme.Back;
                            ret.Add(stop);
                        }
                        else ret.Add(nextframe[i, j].ToComplexPixel(i, j));
                    }
            return ret;
        }

        public MediaComplexive Optimize()
        {
            var render = new MediaPixel[List[0].Width, List[0].Height];
            var nextframe = new MediaPixel[List[0].Width, List[0].Height];

            OptimizedList = new Complexive[List.Length];
            Fill(render, List[0]);
            OptimizedList[0] = List[0].Clone().Optimize();

            for (var i = 1; i < List.Length; i++)
            {
                Array.Clear(nextframe, 0, nextframe.Length);
                Fill(nextframe, List[i]);

                var complexive = new Complexive() { Width = List[0].Width, Height = List[0].Height, Background = List[0].Background };
                complexive.Pixels = FoundNonEquals(render, nextframe, complexive.Background).ToArray();

                OptimizedList[i] = complexive.Optimize();

                Array.Copy(nextframe, render, nextframe.Length);
            }

            Array.Clear(nextframe, 0, nextframe.Length);
            Fill(nextframe, List[0]);

            var complexive0 = new Complexive() { Width = List[0].Width, Height = List[0].Height, Background = List[0].Background };
            complexive0.Pixels = FoundNonEquals(render, nextframe, complexive0.Background).ToArray();

            LastToFirst = complexive0.Optimize();

            return this;
        }

        public static MediaComplexive Read(BinaryReader br)
        {
            var mc = new MediaComplexive();
            var count = br.ReadInt32();
            mc.List = new Complexive[count];
            for (var i = 0; i < count; i++)
                mc.List[i] = Complexive.Read(br);
            if (count > 0)
            {
                mc.Width = mc.List[0].Width;
                mc.Height = mc.List[0].Height;
            }
            return mc;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(List.Length);
            for (var i = 0; i < List.Length; i++)
                List[i].Write(bw);
        }

        public MediaComplexive Clone()
        {
            var rslt = new MediaComplexive();
            rslt.Width = Width;
            rslt.Height = Height;
            rslt.List = List.ConvertAll(x => x.Clone());
            return rslt;
        }

        public static MediaComplexive FromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            using (var br = new BinaryReader(stream))
                return Read(br);
        }

        public void WriteToFile(string filename)
        {
            using (var stream = File.OpenWrite(filename))
            using (var bw = new BinaryWriter(stream))
                Write(bw);
        }
    }
}
