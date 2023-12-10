using KCore.Extensions;
using KCore.Extensions.InsteadSLThree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    public abstract class SuperText : ICloneable
    {
        public (int, int) CachedCorner { get; set; }
        public int CachedHeight { get; set; }

        public abstract void Invoke();

        public static readonly Func<(ConsoleColor, ConsoleColor)> DefaultResetColorRedirect = () => (Theme.Fore, Theme.Back);
        public static readonly Func<(ConsoleColor, ConsoleColor)> ResetColorDisableText = () => (Theme.Disabled, Theme.Back);
        public static IEnumerable<SuperText> PrepareToWindow(IEnumerable<SuperText> actions, IContainer container, Func<(ConsoleColor, ConsoleColor)> ResetColorRedirect = null, TextAlignment alignment = TextAlignment.Left)
        {
            if (ResetColorRedirect == null) ResetColorRedirect = DefaultResetColorRedirect;

            var x = 0;
            var y = 0;
            var max_height = container.Height - container.Top - 1;

            foreach (var action in actions)
            {
                action.CachedCorner = (container.Left, container.Top);
                action.CachedHeight = container.Height;
                if (action is SuperTextOut ot)
                {
                    ot.EndlineSpace = false;
                    if (x + ot.Text.Length > container.Width)
                    {
                        if (alignment != TextAlignment.Center && ot.Text == " ")
                        {
                            ot.EndlineSpace = true;
                            x = 0;
                            y++;
                        }
                        else ot.Position = (x = 0, ++y);
                    }
                    else ot.Position = (x, y);
                    if (!ot.EndlineSpace) x += ot.Text.Length;
                }
                if (action is SuperTextNewLine)
                {
                    y++;
                    x = 0;
                }
                if (action is SuperTextColorChange stcc)
                    stcc.ResetColor = ResetColorRedirect;
            }

            if (alignment != TextAlignment.Left)
            {
                for (var i = 0; i < Terminal.FixedWindowHeight; i++)
                {
                    var t = actions.Where(a => a is SuperTextOut st && st.Position.Item2 == i).ToArray().ConvertAll(z => (SuperTextOut)z);

                    if (!t.Any()) continue;

                    var last = t.LastOrDefault(z => z.Text != " ");
                    if (last == null) continue;

                    var free = container.Width - (last.Position.Item1 + last.Text.Length);
                    if (alignment == TextAlignment.Center) if (free > 1) free = free / 2 + free % 2;

                    foreach (var ts in t) ts.Position = (ts.Position.Item1 + free, ts.Position.Item2);
                }
            }

            return actions;
        }

        private static readonly string nline = ((char)10).ToString();
        public static IEnumerable<SuperText> PrepareToInvoke(string description)
        {
            description = description.Replace("\\n", nline);

            var i = 0;
            var last = 0;

            var ret = new List<string>();
            var sb = new StringBuilder();

            var spaces = false;

            while (i < description.Length)
            {
                if (description[i] == 10)
                {
                    spaces = false;
                    if (i - last > 0) ret.Add(description.Substring(last, i - last));
                    ret.Add(nline);
                    last = i + 1;
                }
                else
                {
                    if (description[i] == '%' && i + 3 < description.Length && description[i + 1] == '=' && description[i + 2] == '>')
                    {
                        var start = i;
                        i = description.IndexOf('%', i + 1);
                        if (i == -1) break;
                        else
                        {
                            if (start - last > 0) ret.Add(description.Substring(last, start - last));
                            last = i + 1;
                            ret.Add(description.Substring(start, i - start + 1));
                        }
                    }
                    var ws = char.IsWhiteSpace(description[i]);
                    if (!spaces && ws)
                    {
                        spaces = true;
                        if (i - last > 0)
                        {
                            ret.Add(description.Substring(last, i - last));
                            last = i;
                        }
                    }
                    if (spaces && !ws)
                    {
                        spaces = false;
                        if (i - last > 0)
                        {
                            ret.Add(description.Substring(last, i - last));
                            last = i;
                        }
                    }
                }
                i++;
            }
            ret.Add(description.Substring(last));

            foreach (var x in ret)
            {
                if (x.StartsWith("%=>"))
                {
                    if (x.Length - 4 < 0)
                    {
                        yield return new SuperTextOut(x);
                        continue;
                    }
                    var s = x.Substring(3, x.Length - 4);
                    if (s.StartsWith("!"))
                    {
                        s = s.Substring(1).Trim();
                        //todo Methods))
                    }
                    else
                    {
                        s = s.Trim();
                        var tw = s.ToWords(' ', ',');
                        if (tw.Length == 2 && tw.Contains("reset") && tw.Contains("back")) yield return new SuperTextColorChange() { IsReset = true, IsBackground = true, Text = tw };
                        else if (tw.Length == 1 && tw[0] == "reset") yield return new SuperTextColorChange() { IsReset = true };
                        else
                        {
                            if (tw.Contains("disable") || tw.Contains("error")) yield return new SuperTextColorChange() { Text = tw, IsBackground = tw.Contains("back") };
                            else
                            {
                                var color = tw.FirstOrDefault(z => z.IsEnum<ConsoleColor>());
                                if (color != null)
                                {
                                    var cc = new SuperTextColorChange();
                                    cc.Color = color.ToEnum<ConsoleColor>();
                                    cc.IsDirect = tw.Contains("direct");
                                    cc.IsBackground = tw.Contains("back");
                                    cc.IsReset = tw.Contains("reset");
                                    cc.Text = tw;
                                    yield return cc;
                                }
                                else yield return new SuperTextOut(x);
                            }
                        }
                    }
                }
                else if (x == nline) yield return new SuperTextNewLine();
                else
                {
                    yield return new SuperTextOut(x);
                }
            }
            yield return new SuperTextNewLine();
        }
        public class SuperTextOut : SuperText
        {
            public string Text;

            public (int, int) Position;

            public bool EndlineSpace;

            private SuperTextOut() { }

            public SuperTextOut(string text)
            {
                Text = text;
            }

            public override void Invoke()
            {
                //if (EndlineSpace || Position.Item2 > CachedHeight) return;
                if (EndlineSpace) return;
                Terminal.Set(CachedCorner.Item1 + Position.Item1, CachedCorner.Item2 + Position.Item2);
                Terminal.Write(Text);
            }

            public override string ToString() => Text;

            public override object Clone()
            {
                return MemberwiseClone();
            }
        }

        public class SuperTextColorChange : SuperText
        {
            public bool IsReset { get; set; }
            public bool IsDirect { get; set; }
            public bool IsBackground { get; set; }
            public ConsoleColor Color { get; set; }
            public string[] Text { get; set; }
            public Func<(ConsoleColor, ConsoleColor)> ResetColor { get; set; } // fore, back

            public override void Invoke()
            {
                if (Text != null && Text.Contains("disable"))
                {
                    if (IsBackground) Terminal.Back = Theme.Disabled;
                    else Terminal.Back = Theme.Disabled;
                }
                else if (Text != null && Text.Contains("error"))
                {
                    if (IsBackground) Terminal.Back = Theme.Error;
                    else Terminal.Fore = Theme.Error;
                }
                else if (IsReset)
                {
                    if (IsBackground)
                    {
                        Terminal.ResetColor();
                        (Terminal.Fore, Terminal.Back) = ResetColor();
                    }
                    else
                    {
                        Terminal.ResetFore();
                        Terminal.Fore = ResetColor().Item1;
                    }
                }
                else
                {
                    if (IsBackground) Terminal.Back = Color;
                    else Terminal.Fore = Color;
                }
            }
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("[ColorChange] ");
                if (IsReset) sb.Append("reset ");
                if (IsBackground) sb.Append("back ");
                if (!IsReset)
                {
                    if (IsDirect) sb.Append("direct ");
                    sb.Append(Color);
                }

                return sb.ToString();
            }

            public override object Clone()
            {
                var o = (SuperTextColorChange)MemberwiseClone();
                o.Text = Text?.ConvertAll(x => x?.Clone() as string);
                return o;
            }
        }

        public class SuperTextNewLine : SuperText
        {
            public override void Invoke()
            {

            }

            public override object Clone()
            {
                return MemberwiseClone();
            }
        }

        public abstract object Clone();
    }
}
