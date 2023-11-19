using KCore.Extensions;
using System;

namespace KCore.CoreForms
{
    public class ExceptionViewer : BaseForm
    {
        public static string[] Description = new string[]
        {
            "This text will be displayed to the user!",
            "Change it by adding ways to contact you to report this error",
        };
        public static string ForRestartPressAnyKey = "For restart press any key";
        public static string ForExitPressEscape = "For exit press Escape key";
        public static string ForExitPressAnyKey = "For exit press any key";
        public static string UnhandledException = "Critical error!";

        protected override bool IsRecursiveForm() => true;
        protected override ConsoleColor Background { get => bsod ? ConsoleColor.Blue : ConsoleColor.Black; }

        private Exception exception;
        private bool restartavailable;
        private bool showdescription = true;
        public bool Restarted { get; private set; }
        public ExceptionViewer(Exception e, bool restart)
        {
            restartavailable = restart;
            exception = e;

            Add(Exit, Restart, ChangeStyle);

            Terminal.Back = ConsoleColor.Black;
        }
        public ExceptionViewer(Exception e, bool restart, bool showdesc)
        {
            restartavailable = restart;
            exception = e;
            showdescription = showdesc;

            Add(Exit, Restart, ChangeStyle);

            Terminal.Back = Back;
        }

        ConsoleColor Back = ConsoleColor.Black;
        ConsoleColor Border = ConsoleColor.Red;
        ConsoleColor BorderText = ConsoleColor.White;
        ConsoleColor Text = ConsoleColor.Red;
        static bool bsod = true;

        protected override void OnClosing()
        {
            Terminal.Back = ConsoleColor.White;
        }
        protected override void OnAllRedraw()
        {
            if (bsod)
            {
                Terminal.Back = ConsoleColor.Blue;
                Terminal.Fore = ConsoleColor.White;
                Terminal.Set(1, 1);
                Terminal.Write(exception.GetType().FullName.ToUpper());
                Terminal.Set(1, 3);
                var msg = exception.ToString().Split((char)10).SizeSeparate(Terminal.FixedWindowWidth - 2);
                var max = msg.Length > (Terminal.FixedWindowHeight - 10) ? Terminal.FixedWindowHeight - 10 : msg.Length;
                for (var i = 0; i < max; i++)
                {
                    Terminal.Set(1, 3 + i);
                    Terminal.Write(msg[i]);
                }
                if (showdescription)
                {
                    Terminal.Fore = ConsoleColor.Yellow;
                    for (var i = 0; i < Description.Length; i++)
                    {
                        Terminal.Set(1, Terminal.FixedWindowHeight - 4 - (Description.Length - i));
                        Terminal.Write(Description[i]);
                    }
                }
                Terminal.Fore = ConsoleColor.White;
                if (restartavailable)
                {
                    Terminal.Set(1, Terminal.FixedWindowHeight - 3);
                    Terminal.Write(ForRestartPressAnyKey);
                    Terminal.Set(1, Terminal.FixedWindowHeight - 2);
                    Terminal.Write(ForExitPressEscape);
                }
                else
                {
                    Terminal.Set(1, Terminal.FixedWindowHeight - 2);
                    Terminal.Write(ForExitPressAnyKey);
                }
                Terminal.ResetFore();
            }
            else
            {
                //new Rectangle(0, 0, Console.FixedWindowWidth - 2, Console.FixedWindowHeight - 2, Alignment.CenterHeight | Alignment.CenterWidth).Draw(Border);
                Terminal.Back = Border;
                Terminal.Fore = BorderText;
                Terminal.Set(2, 1);
                Terminal.Write(UnhandledException);
                Terminal.Back = Back;
                Terminal.Set(3, 3);
                Terminal.Fore = Text;
                var msg = exception.ToString().Split((char)10).SizeSeparate(Terminal.FixedWindowWidth - 6);
                var max = msg.Length > (Terminal.FixedWindowHeight - 14) ? Terminal.FixedWindowHeight - 14 : msg.Length;
                for (var i = 0; i < max; i++)
                {
                    Terminal.Set(3, 3 + i);
                    Terminal.Write(msg[i]);
                }

                if (showdescription)
                {
                    Terminal.Fore = BorderText;
                    for (var i = 0; i < Description.Length; i++)
                    {
                        Terminal.Set(3, Terminal.FixedWindowHeight - 6 - (Description.Length - i));
                        Terminal.Write(Description[i]);
                    }
                }

                Terminal.Fore = BorderText;
                if (restartavailable)
                {
                    Terminal.Set(3, Terminal.FixedWindowHeight - 5);
                    Terminal.Write(ForRestartPressAnyKey);
                    Terminal.Set(3, Terminal.FixedWindowHeight - 4);
                    Terminal.Write(ForExitPressEscape);
                }
                else
                {
                    Terminal.Set(3, Terminal.FixedWindowHeight - 4);
                    Terminal.Write(ForExitPressAnyKey);
                }
                Terminal.ResetFore();
            }
        }

        private void Exit()
        {
            Restarted = false;
            Close();
        }

        private void Restart()
        {
            Restarted = restartavailable;
            Close();
        }
        private void ChangeStyle()
        {
            bsod = !bsod;
            Clear();
        }

        protected override void OnKeyDown(byte key)
        {
            if (key == Key.Escape) this.Reactions["Exit"] = true;
            else if (key == Key.Enter || key == Key.E || key == Key.Spacebar) this.Reactions["Restart"] = true;
            else if (key != Key.Escape && key > 2) this.Reactions["Restart"] = true;
            if (key == Key.F5) Reactions["ChangeStyle"] = true;
        }
    }
}
