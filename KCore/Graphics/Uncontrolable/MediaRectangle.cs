using KCore.Graphics.Core;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCore.Terminal;

namespace KCore.Graphics.Uncontrolable
{
    public class MediaRectangle : BoundedObject, IContainer
    {
        int IContainer.Left => GetLeftCornerValue();

        int IContainer.Top => GetTopCornerValue();

        int IContainer.Width => Source.Width;

        int IContainer.Height => Source.Height;

        public override int Width { get => Source.Width; }
        public override int Height { get => Source.Height; }

        public MediaComplexive Source { get; set; }

        public MediaRectangle(MediaComplexive primitive, IContainer container, int left, int top, Alignment alignment = LeftUpAlignment)
        {
            Source = primitive;
            Container = container;
            Left = left;
            Top = top;
            Alignment = alignment;
        }

        public MediaRectangle(MediaComplexive primitive, int left, int top, Alignment alignment = LeftUpAlignment)
        {
            Source = primitive;
            Container = TerminalContainer.This;
            Left = left;
            Top = top;
            Alignment = alignment;
        }

        public (int, int) AllRedraw() => AllRedraw(GetLeftCornerValue(), GetTopCornerValue());

        public (int, int) AllRedraw(int left, int top)
        {
            var result = (left, top);
            for (var i = 0; i < CurrentFrame; i++)
            {
                if (i < Frames)
                    result = DrawFrame(left, top, i);
                else if (i == Frames) result = DrawLastToFirst(left, top);
                else result = (left, top);
            }
            return result;
        }

        public (int, int) DrawFrame() => DrawFrame(GetLeftCornerValue(), GetTopCornerValue());

        public (int, int) DrawFrame(int left, int top)
        {
            if (!Loop)
            {
                if (CurrentFrame < Frames)
                    return DrawFrame(left, top, CurrentFrame++);
                else if (CurrentFrame == Frames) return DrawLastToFirst(left, top);
                else return (left, top);
            }
            else
            {
                if (CurrentFrame == Frames)
                {
                    CurrentFrame = 1;
                    return DrawLastToFirst(left, top);
                }
                else return DrawFrame(left, top, CurrentFrame++);
            }
        }

        public (int, int) DrawLastToFirst(int left, int top)
        {
            if (Source.Optimized)
            {
                var lastback = Back;
                var lastfore = Fore;
                for (var i = 0; i < Source.LastToFirst.Pixels.Length; i++)
                {
                    var px = Source.LastToFirst.OptimizedPixels[i].Pixel;
                    Set(left + px.PositionLeft, top + px.PositionTop);
                    if (lastback != px.BackgroundColor) Back = px.BackgroundColor;
                    if (lastfore != px.ForegroundColor) Fore = px.ForegroundColor;
                    Write(Graph.Chars(px.Character, Source.LastToFirst.OptimizedPixels[i].Length));
                    lastback = px.BackgroundColor;
                    lastfore = px.ForegroundColor;
                }
                ResetColor();
            }
            else
            {
                var lastback = Back;
                var lastfore = Fore;
                var lastwdh = 0;
                var lasthgt = 0;

                for (var i = 0; i < Source.LastToFirst.Pixels.Length; i++)
                {
                    var currentwidth = left + Source.LastToFirst.Pixels[i].PositionLeft;
                    var currentheight = top + Source.LastToFirst.Pixels[i].PositionTop;
                    if (FixedWindowWidth - 1 == currentwidth && FixedWindowHeight - 1 == currentheight) continue;
                    if ((lastwdh + 1 != currentwidth) || (lasthgt != currentheight)) Set(currentwidth, currentheight);
                    lastwdh = currentwidth;
                    lasthgt = currentheight;
                    if (lastback != Source.LastToFirst.Pixels[i].BackgroundColor) Back = Source.LastToFirst.Pixels[i].BackgroundColor;
                    if (lastfore != Source.LastToFirst.Pixels[i].ForegroundColor) Fore = Source.LastToFirst.Pixels[i].ForegroundColor;
                    lastback = Source.LastToFirst.Pixels[i].BackgroundColor;
                    lastfore = Source.LastToFirst.Pixels[i].ForegroundColor;
                    Write(Source.LastToFirst.Pixels[i].Character);
                }
                ResetColor();
            }

            return (left, top);
        }

        public (int, int) DrawFrame(int left, int top, int frame)
        {
            if (Source.Optimized)
            {
                var lastback = Back;
                var lastfore = Fore;
                for (var i = 0; i < Source.OptimizedList[frame].OptimizedPixels.Length; i++)
                {
                    var px = Source.OptimizedList[frame].OptimizedPixels[i].Pixel;
                    Set(left + px.PositionLeft, top + px.PositionTop);
                    if (lastback != px.BackgroundColor) Back = px.BackgroundColor;
                    if (lastfore != px.ForegroundColor) Fore = px.ForegroundColor;
                    Write(Graph.Chars(px.Character, Source.OptimizedList[frame].OptimizedPixels[i].Length));
                    lastback = px.BackgroundColor;
                    lastfore = px.ForegroundColor;
                }
                ResetColor();
            }
            else
            {
                var lastback = Back;
                var lastfore = Fore;
                var lastwdh = 0;
                var lasthgt = 0;

                for (var i = 0; i < Source.List[frame].Pixels.Length; i++)
                {
                    var currentwidth = left + Source.List[frame].Pixels[i].PositionLeft;
                    var currentheight = top + Source.List[frame].Pixels[i].PositionTop;
                    if (FixedWindowWidth - 1 == currentwidth && FixedWindowHeight - 1 == currentheight) continue;
                    if ((lastwdh + 1 != currentwidth) || (lasthgt != currentheight)) Set(currentwidth, currentheight);
                    lastwdh = currentwidth;
                    lasthgt = currentheight;
                    if (lastback != Source.List[frame].Pixels[i].BackgroundColor) Back = Source.List[frame].Pixels[i].BackgroundColor;
                    if (lastfore != Source.List[frame].Pixels[i].ForegroundColor) Fore = Source.List[frame].Pixels[i].ForegroundColor;
                    lastback = Source.List[frame].Pixels[i].BackgroundColor;
                    lastfore = Source.List[frame].Pixels[i].ForegroundColor;
                    Write(Source.List[frame].Pixels[i].Character);
                }
                ResetColor();
            }

            return (left, top);
        }

        public int Frames => Source.List.Length;

        public int CurrentFrame { get; set; }
        public bool Loop { get; set; } = true;

        public override (int, int) Draw(int left, int top)
        {
            if (Source.Optimized)
            {
                for (var frame = 0; frame < Source.OptimizedList.Length; frame++)
                {
                    var lastback = Back;
                    var lastfore = Fore;
                    for (var i = 0; i < Source.OptimizedList[frame].Pixels.Length; i++)
                    {
                        var px = Source.OptimizedList[frame].OptimizedPixels[i].Pixel;
                        Set(left + px.PositionLeft, top + px.PositionTop);
                        if (lastback != px.BackgroundColor) Back = px.BackgroundColor;
                        if (lastfore != px.ForegroundColor) Fore = px.ForegroundColor;
                        Write(Graph.Chars(px.Character, Source.OptimizedList[frame].OptimizedPixels[i].Length));
                        lastback = px.BackgroundColor;
                        lastfore = px.ForegroundColor;
                    }
                    ResetColor();
                }
            }
            else
            {
                for (var frame = 0; frame < Source.List.Length; frame++)
                {
                    var lastback = Back;
                    var lastfore = Fore;
                    var lastwdh = 0;
                    var lasthgt = 0;

                    for (var i = 0; i < Source.List[frame].Pixels.Length; i++)
                    {
                        var currentwidth = left + Source.List[frame].Pixels[i].PositionLeft;
                        var currentheight = top + Source.List[frame].Pixels[i].PositionTop;
                        if (FixedWindowWidth - 1 == currentwidth && FixedWindowHeight - 1 == currentheight) continue;
                        if ((lastwdh + 1 != currentwidth) || (lasthgt != currentheight)) Set(currentwidth, currentheight);
                        lastwdh = currentwidth;
                        lasthgt = currentheight;
                        if (lastback != Source.List[frame].Pixels[i].BackgroundColor) Back = Source.List[frame].Pixels[i].BackgroundColor;
                        if (lastfore != Source.List[frame].Pixels[i].ForegroundColor) Fore = Source.List[frame].Pixels[i].ForegroundColor;
                        lastback = Source.List[frame].Pixels[i].BackgroundColor;
                        lastfore = Source.List[frame].Pixels[i].ForegroundColor;
                        Write(Source.List[frame].Pixels[i].Character);
                    }
                    ResetColor();
                }
            }

            return (left, top);
        }

        public override (int, int) Clear(int left, int top)
        {
            Back = Theme.Back;
            for (var i = 0; i < Source.Height; i++)
            {
                Set(left, top + i);
                Graph.OutSpaces(Source.Width);
            }

            return (left, top);
        }
    }
}
