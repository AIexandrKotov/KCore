using KCore.Graphics.Core;
using KCore.TerminalCore;
using KCore.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = KCore.Terminal;
using static KCore.Terminal;
using KCore.Forms;

namespace KCore.Graphics
{
    public static class TransitionAnimation
    {
        public enum Preset
        {
            FromTheRight,
            FromTheLeft,
            FromTheTop,
            FromTheBottom,
        }

        public static void RunComplexives(TerminalRedirected.DrawingRedirection orig, Action<Complexive, List<Complexive>> func)
        {
            var c = orig.ToComplexive();
            var mc = new List<Complexive>();
            func.Invoke(c, mc);
            mc.Add(c);
            var mr = new MediaRectangle(new MediaComplexive(mc.ToArray()), 0, 0);
            mr.Source.Optimize();
            while (mr.CurrentFrame < mr.Frames)
            {
                mr.DrawFrame();
            }
        }

        public static TerminalRedirected.DrawingRedirection GetDrawingRedirection(Form block, bool stop = false)
        {
            TerminalRedirected.StartRedirection();
            if (stop) block.CancelAllActions();
            block.OneTimeDrawForRedirection();
            return TerminalRedirected.StopRedirection();
        }

        public static void Animate(TerminalRedirected.DrawingRedirection red, Preset preset, int speed = 10, IContainer container = null)
        {
            if (container == null) container = TerminalContainer.This;

            try
            {
                switch (preset)
                {
                    case Preset.FromTheLeft:
                        {
                            RunComplexives(red, (c, mc) =>
                            {
                                for (var i = 0; i < FixedWindowWidth / speed - 1; i++)
                                {
                                    var step = FixedWindowWidth - speed * (i + 1);
                                    mc.Add(c.GetEmptyAnalog().UpdatePixels(c.Pixels
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)
                                        .OnlyPixels(step, 0, FixedWindowWidth - step, FixedWindowHeight)
                                        .ShiftPixels(-step, 0)));
                                }
                            });
                        }
                        break;
                    case Preset.FromTheRight:
                        {
                            RunComplexives(red, (c, mc) =>
                            {
                                for (var i = 0; i < FixedWindowWidth / speed - 1; i++)
                                {
                                    var step = FixedWindowWidth - speed * (i + 1);
                                    mc.Add(c.GetEmptyAnalog().UpdatePixels(c.Pixels
                                        .OnlyPixels(0, 0, FixedWindowWidth - step, FixedWindowHeight)
                                        .ShiftPixels(step, 0).OnlyPixels(x => !(FixedWindowWidth - 1 == x.PositionLeft && FixedWindowHeight - 1 == x.PositionTop))
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)));
                                }
                            });
                        }
                        break;
                    case Preset.FromTheTop:
                        {
                            RunComplexives(red, (c, mc) =>
                            {
                                var speedh = (int)(speed / 3.0);
                                for (var i = 0; i < FixedWindowHeight / speedh - 1; i++)
                                {
                                    var step = FixedWindowHeight - speedh * (i + 1);
                                    mc.Add(c.GetEmptyAnalog().UpdatePixels(c.Pixels
                                        .OnlyPixels(0, step, FixedWindowWidth, FixedWindowHeight - step)
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)
                                        .ShiftPixels(0, -step).OnlyPixels(x => !(FixedWindowWidth - 1 == x.PositionLeft && FixedWindowHeight - 1 == x.PositionTop))
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)));
                                }
                            });
                        }
                        break;
                    case Preset.FromTheBottom:
                        {
                            RunComplexives(red, (c, mc) =>
                            {
                                var speedh = (int)(speed / 3.0);
                                for (var i = 0; i < FixedWindowHeight / speedh - 1; i++)
                                {
                                    var step = FixedWindowHeight - speedh * (i + 1);
                                    mc.Add(c.GetEmptyAnalog().UpdatePixels(c.Pixels
                                        .OnlyPixels(0, 0, FixedWindowWidth, FixedWindowHeight - step)
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)
                                        .ShiftPixels(0, step).OnlyPixels(x => !(FixedWindowWidth - 1 == x.PositionLeft && FixedWindowHeight - 1 == x.PositionTop))
                                        .OnlyPixels(container.Left, container.Top, container.Width, container.Height)));
                                }
                            });
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Add(e.ToString());
            }
        }
    }
}
