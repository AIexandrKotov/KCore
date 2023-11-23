using KCore.Extensions;
using KCore.Forms;
using KCore.Graphics.Containers;
using KCore.Graphics.Core;
using KCore.TerminalCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCore.Graphics.TransitionAnimation;

namespace KCore.Graphics
{
    public static class GraphicsExtensions
    {
        public static ValueTuple<int, int> CenterPads(this string s, int totalWidth)
        {
            var tw = totalWidth - s.Length;
            if (tw % 2 == 0) return (tw / 2, tw / 2);
            else return (tw / 2, tw / 2 + 1);
        }

        public static string PadCenter(this string s, int totalWidth, char paddingChar)
        {
            var tw = totalWidth - s.Length;
            if (tw <= 0) return s;
            if (tw % 2 == 0) return new string(paddingChar, tw / 2) + s + new string(paddingChar, tw / 2);
            else return new string(paddingChar, tw / 2) + s + new string(paddingChar, tw / 2 + 1);
        }

        public static string PadCenter(this string s, int totalWidth) => s.PadCenter(totalWidth, ' ');

        public static string Pad(this string s, TextAlignment textAlignment, int totalWidth)
        {
            if (s.Length >= totalWidth) return s.Reduce(totalWidth);
            else
            {
                switch (textAlignment)
                {
                    case TextAlignment.Left: return s.PadRight(totalWidth);
                    case TextAlignment.Center: return s.PadCenter(totalWidth);
                    case TextAlignment.Right: return s.PadLeft(totalWidth);
                    default: throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// Сокращает строку до totalWidth символов, но добавляет многоточие в конце
        /// </summary>
        public static string Reduce(this string s, int totalWidth)
        {
            if (s.Length <= totalWidth) return s;
            else
            {
                return s.Left(totalWidth - 3) + "...";
            }
        }

        public static void PrintSuperText(this string text, Func<(ConsoleColor, ConsoleColor)> ResetColorRedirect = null, TextAlignment alignment = TextAlignment.Left)
        {
            if (ResetColorRedirect == null) ResetColorRedirect = SuperText.DefaultResetColorRedirect;
            text.PrintSuperText(new StaticContainer(Terminal.Left, Terminal.Top, Terminal.FixedWindowWidth - 1 - Terminal.Left, 1), ResetColorRedirect, alignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="container"></param>
        /// <param name="ResetColorRedirect">(Fore, Back)</param>
        /// <param name="alignment"></param>
        public static void PrintSuperText(this string text, IContainer container, Func<(ConsoleColor, ConsoleColor)> ResetColorRedirect = null, TextAlignment alignment = TextAlignment.Left)
        {
            if (ResetColorRedirect == null) ResetColorRedirect = SuperText.DefaultResetColorRedirect;
            text.GetSuperText(container, ResetColorRedirect, alignment).PrintSuperText(container);
        }

        public static IEnumerable<SuperText> GetSuperText(this string text, IContainer container, Func<(ConsoleColor, ConsoleColor)> ResetColorRedirect = null, TextAlignment alignment = TextAlignment.Left)
        {
            if (ResetColorRedirect == null) ResetColorRedirect = SuperText.DefaultResetColorRedirect;
            return SuperText.PrepareToWindow(SuperText.PrepareToInvoke(text).ToArray(), container, ResetColorRedirect, alignment);
        }

        public static IEnumerable<ComplexPixel> OnlyPixels(this IEnumerable<ComplexPixel> Pixels, int left, int top, int width, int height)
        {
            return Pixels.Where(x => x.PositionLeft >= left && x.PositionLeft < left + width && x.PositionTop >= top && x.PositionTop < top + height && !(x.PositionLeft == left + width && x.PositionTop == top + height));
        }

        public static IEnumerable<ComplexPixel> OnlyPixels(this IEnumerable<ComplexPixel> Pixels, Func<ComplexPixel, bool> predicate)
        {
            return Pixels.Where(predicate);
        }

        public static IEnumerable<ComplexPixel> ShiftPixels(this IEnumerable<ComplexPixel> Pixels, int left, int top)
        {
            return Pixels.Select(x => new ComplexPixel(x.PositionLeft + left, x.PositionTop + top, x.Character, x.ForegroundColor, x.BackgroundColor));
        }

        public static void PrintSuperText(this IEnumerable<SuperText> text, IContainer container = null)
        {
            if (container == null) container = TerminalContainer.This;
            foreach (var x in text)
            {
                if ((x is SuperText.SuperTextOut sto) && (sto.Position.Item1 + x.CachedCorner.Item1 < container.Left || sto.Position.Item1 + x.CachedCorner.Item1 >= (container.Left + container.Width)
                 || sto.Position.Item2 + x.CachedCorner.Item2 < container.Top || sto.Position.Item2 + x.CachedCorner.Item2 >= (container.Top + container.Height)
                 || sto.Text.Length + sto.Position.Item1 + x.CachedCorner.Item1 > container.Width + container.Left)) break;
                x.Invoke();
            }
        }

        public static Complexive GetEmptyAnalog(this Complexive complexive)
        {
            return new Complexive() { Width = complexive.Width, Height = complexive.Height, Pixels = new ComplexPixel[0] };
        }


        public static void StartAnyAnimations(this Form BaseForm, Form other, params Action<TerminalRedirected.DrawingRedirection>[] actions)
        {
            var red = GetDrawingRedirection(other);
            for (var i = 0; i < actions.Length; i++)
                actions[i].Invoke(red);
            BaseForm.Start(other, true);
        }

        public static void StartAnyAnimations(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            BaseForm.Start(other, true);
        }

        public static void StartAnyAnimations(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            action2.Invoke(red);
            BaseForm.Start(other, true);
        }

        public static void StartAnyAnimations(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2, Action<TerminalRedirected.DrawingRedirection> action3)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            action2.Invoke(red);
            action3.Invoke(red);
            BaseForm.Start(other, true);
        }

        public static void StartAnyAnimationsNotClear(this Form BaseForm, Form other, params Action<TerminalRedirected.DrawingRedirection>[] actions)
        {
            var red = GetDrawingRedirection(other);
            for (var i = 0; i < actions.Length; i++)
                actions[i].Invoke(red);
            BaseForm.Start(other, true, true);
        }

        public static void StartAnyAnimationsNotClear(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            BaseForm.Start(other, true, true);
        }

        public static void StartAnyAnimationsNotClear(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            action2.Invoke(red);
            BaseForm.Start(other, true, true);
        }

        public static void StartAnyAnimationsNotClear(this Form BaseForm, Form other, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2, Action<TerminalRedirected.DrawingRedirection> action3)
        {
            var red = GetDrawingRedirection(other);
            action1.Invoke(red);
            action2.Invoke(red);
            action3.Invoke(red);
            BaseForm.Start(other, true, true);
        }

        public static void ReturnAnyAnimations(this Form BaseForm, params Action<TerminalRedirected.DrawingRedirection>[] actions)
        {
            var red = GetDrawingRedirection(BaseForm, true);
            for (var i = 0; i < actions.Length; i++)
                actions[i].Invoke(red);
            BaseForm.StopAllRedraw();
        }

        public static void ReturnAnyAnimations(this Form BaseForm, Action<TerminalRedirected.DrawingRedirection> action1)
        {
            var red = GetDrawingRedirection(BaseForm, true);
            action1.Invoke(red);
            BaseForm.StopAllRedraw();
        }

        public static void ReturnAnyAnimations(this Form BaseForm, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2)
        {
            var red = GetDrawingRedirection(BaseForm, true);
            action1.Invoke(red);
            action2.Invoke(red);
            BaseForm.StopAllRedraw();
        }

        public static void ReturnAnyAnimations(this Form BaseForm, Action<TerminalRedirected.DrawingRedirection> action1, Action<TerminalRedirected.DrawingRedirection> action2, Action<TerminalRedirected.DrawingRedirection> action3)
        {
            var red = GetDrawingRedirection(BaseForm, true);
            action1.Invoke(red);
            action2.Invoke(red);
            action3.Invoke(red);
            BaseForm.StopAllRedraw();
        }

        public static void Animate(this TerminalRedirected.DrawingRedirection redirection, Preset preset, int speed = 10, IContainer container = null)
        {
            TransitionAnimation.Animate(redirection, preset, speed, container);
        }

        public static bool AllowTransitionAnimations = true;
        public static int TransitionAnimationsSpeed = 10;

        private static TransitionAnimation.Preset GetStart(bool horizontal = false) => horizontal ? TransitionAnimation.Preset.FromTheTop : TransitionAnimation.Preset.FromTheRight;
        private static TransitionAnimation.Preset GetReturn(bool horizontal = false) => horizontal ? TransitionAnimation.Preset.FromTheBottom : TransitionAnimation.Preset.FromTheLeft;
        public static void RealizeAnimation(this Form b, Form other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.StartAnyAnimationsNotClear(other, r => r.Animate(GetStart(horizontal), TransitionAnimationsSpeed));
                b.ReturnAnyAnimations(r => r.Animate(GetReturn(horizontal), TransitionAnimationsSpeed));
            }
            else
            {
                b.Start(other);
            }
        }
        /*public static void GameAnimation(this BaseForm b, BaseForm other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.StartAnyAnimationsNotClear(other, r => r.Animate(GetStart(horizontal), TransitionAnimationsSpeed, Upperface.GameContainer));
                b.ReturnAnyAnimations(r => r.Animate(GetReturn(horizontal), TransitionAnimationsSpeed, Upperface.GameContainer));
            }
            else
            {
                b.Start(other);
            }
        }
        public static void GameStartAnimation(this BaseForm b, BaseForm other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.StartAnyAnimationsNotClear(other, r => r.Animate(GetStart(horizontal), TransitionAnimationsSpeed, Upperface.GameContainer));
            }
            else
            {
                b.Start(other);
            }
        }
        public static void GameReturnAnimation(this BaseForm b, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.ReturnAnyAnimations(r => r.Animate(GetReturn(horizontal), TransitionAnimationsSpeed, Upperface.GameContainer));
            }
            else
            {

            }
        }
*/
        public static void StartAnimation(this Form b, Form other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.StartAnyAnimations(other, r => r.Animate(GetStart(horizontal), TransitionAnimationsSpeed));
            }
            else
            {
                b.Start(other);
            }
        }

        public static void StartAnimation(this Form other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                var red = TransitionAnimation.GetDrawingRedirection(other);
                red.Animate(GetStart(horizontal), TransitionAnimationsSpeed);
                other.Start(true);
            }
            else
            {
                other.Start(true);
            }
        }

        public static void StartAnimationNotClear(this Form b, Form other, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.StartAnyAnimationsNotClear(other, r => r.Animate(GetStart(horizontal), TransitionAnimationsSpeed));
            }
            else
            {
                b.Start(other);
            }
        }

        public static void ReturnAnimation(this Form b, bool horizontal = false)
        {
            if (AllowTransitionAnimations)
            {
                b.ReturnAnyAnimations(r => r.Animate(GetReturn(horizontal), TransitionAnimationsSpeed));
            }
            else
            {

            }
        }
    }
}
